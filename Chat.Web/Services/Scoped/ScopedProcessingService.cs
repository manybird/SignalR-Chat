using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using Chat.Web.ViewModels;
using System.Collections.Generic;
using Chat.Web.Hubs;
using Newtonsoft.Json;
using NuGet.Packaging;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Chat.Web.MiccSdk;
using Chat.Web.Models;
using System;
using Chat.Web.MiccSdk.Conversation;
using Chat.Web.Data;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text;

namespace Chat.Web.Services.Scoped
{
    public interface IScopedProcessingService
    {
        Task DoWork(CancellationToken stoppingToken);
    }

    public class ScopedProcessingService : IScopedProcessingService
    {
        private int executionCount = 0;
        private readonly ILogger _logger;

        private readonly IHubContext<ChatHub> _hubContext;
        private readonly Micc _micc;
        private readonly AppSettings appSettings;
        private readonly int scopedServiceWaitTime = 5;
        private readonly ApplicationDbContext _context;

        public ScopedProcessingService(ILogger<ScopedProcessingService> logger,
            ApplicationDbContext context,
            IHubContext<ChatHub> hubContext, Micc micc, AppSettings settings
            )
        {
            _logger = logger;
            _hubContext = hubContext;
            _context = context;
            _micc = micc;
            appSettings = settings;
            scopedServiceWaitTime = settings.ScopedServiceWaitTimeInSecond ?? 5;
            ChatHub.OnConnectionChanged += ChatHub_OnConnectionChanged;
        }

        private void ChatHub_OnConnectionChanged(ChatHub sender, UserViewModelExt userExt, bool isAdd)
        {
            if (userExt.IsUserVisiting == 0) return;

            var s = JsonConvert.SerializeObject(userExt);
            var user = JsonConvert.DeserializeObject<UserViewModelExt>(s);

            lock (_connectionsMap)
            {
                var tmp = _connectionsMap.FirstOrDefault(w => string.Equals(w.ConnectionId, user.ConnectionId, StringComparison.InvariantCultureIgnoreCase));

                try
                {
                    if (isAdd)
                    {
                        if (tmp != null)
                        {
                            _connectionsMap.Remove(tmp);
                            if (_lastConversations.ContainsKey(user.ConnectionId))
                                _lastConversations.Remove(user.ConnectionId);
                        }
                        _connectionsMap.Add(user);
                    }
                    else
                    {
                        if (tmp != null)
                            _connectionsMap.Remove(tmp);

                        if (_lastConversations.ContainsKey(user.ConnectionId))
                            _lastConversations.Remove(user.ConnectionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ChatHub_OnConnectionChanged error");
                }

            }


            //_logger.LogInformation("ChatHub_OnConnectionChanged _connectionsMap count");
            //this.UpdateConnectionList(ChatHub.GetConnectionsMap());
        }

        private readonly static ICollection<UserViewModelExt> _connectionsMap
            = new List<UserViewModelExt>();
        private readonly static Dictionary<string, ResponseResultConversation> _lastConversations
            = new Dictionary<string, ResponseResultConversation>();

        private bool IsConversationStateChanged(ResponseResultConversation newConversation, string connectionId)
        {          

            if (newConversation != null && !newConversation.IsSuccess) return false;

            lock (_lastConversations)
            {
                if (!_lastConversations.ContainsKey(connectionId))
                {
                    if (newConversation == null)
                    {
                        return false;
                    }
                    else
                    {
                        _lastConversations.Add(connectionId, newConversation);
                        return true;
                    }
                }

                var oldConversation = _lastConversations[connectionId];
                bool isChanged = oldConversation.IsConversationStateChanged(newConversation);

                if (isChanged)
                {
                    if (newConversation == null)
                        _lastConversations.Remove(connectionId);
                    else
                        _lastConversations[connectionId] = newConversation;
                }

                return isChanged;
            }
        }

        public void UpdateConnectionList(Dictionary<string, UserViewModelExt> connectionsMap)
        {
            var s = JsonConvert.SerializeObject(connectionsMap.Values.ToList());
            var list = JsonConvert.DeserializeObject<ICollection<UserViewModelExt>>(s);

            lock (_connectionsMap)
            {
                _connectionsMap.Clear();
                _connectionsMap.AddRange(list.Where(r => r.IsUserVisiting == 1));
            }

            //update _lastConversation log
            lock (_lastConversations)
            {
                foreach (var key in _lastConversations.Keys.ToList())
                {
                    var tmp = _connectionsMap.FirstOrDefault(w => string.Equals(w.ConnectionId, key));
                    if (tmp != null) continue;

                    _lastConversations.Remove(key);
                }
            }

            _logger.LogInformation("UpdateJobList _connectionsMap count {0}", _connectionsMap.Count);
            _logger.LogInformation("UpdateJobList _lastConversation count {0}", _lastConversations.Count);
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                executionCount++;
                _logger.LogDebug(
                    "Scoped Processing Service is working: {Count} {c2}", executionCount, _connectionsMap.Count);

                var runner = new MiccRunner(_micc);


                var adminIds = (from a in _connectionsMap select a.AdminId).ToList();

                foreach (string adminId in adminIds)
                {

                    await DoWorkInnerAsync(runner, adminId);                    
                }

                await Task.Delay(scopedServiceWaitTime * 1000, stoppingToken);
            }
        }

        private async Task DoWorkInnerAsync(MiccRunner runner, string adminId)
        {
            try
            {
                var c = _connectionsMap.FirstOrDefault(w => w.AdminId == adminId);

                if (c == null) return;

                var msg = new MessageUserActionViewModel()
                {
                    AdminId = c.AdminId,
                    Timestamp = DateTime.Now
                };

                var responseResult = await runner.GetConversationById(c.CaseId);

                if (responseResult == null)
                {
                    _logger.LogError("onConversationStateUpdate responseResult is null");
                    return;
                }

                if (responseResult.IsSuccess)
                {
                    var newConversation = responseResult;

                    

                    if (IsConversationStateChanged(newConversation, c.ConnectionId))
                    {
                        if (newConversation != null && newConversation.IsCompleted(_micc.CompletedCaseFolders))
                        {
                            _logger.LogInformation("Conversation is handled, update it base!");

                            var c1 = await _context.GetCaseByAdminAndCaseIdAsync(c.AdminId, c.CaseId);
                            if (c1 != null)
                            {
                                c1.CaseCompletionDate = DateTime.Now;
                                c1.Folder = newConversation.Folder;
                                await _context.SaveChangesAsync();
                            }
                        }

                        //conversation changed
                        msg.Conversation = newConversation;
                        await _hubContext.Clients.Client(c.ConnectionId).SendAsync("onConversationStateUpdate", msg);
                    }
                    else
                    {
                        _logger.LogDebug("Micc ok, state not changed {t0} {t1},", _connectionsMap.Count, _lastConversations.Count);
                    }
                }
                else if (responseResult.StatusCode == 404)
                {
                    _logger.LogDebug("responseResult.StatusCode: ${ss}", responseResult.StatusCode);
                }
                else if (responseResult.IsSuccessStatusCode)
                {
                    //Connection to micc ok but no conversation found
                    //Skip logging
                }
                else
                {
                    _logger.LogError("onConversationStateUpdate {msg}", responseResult.GetErrorOrMessage());
                }

            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "DoWork");
            }
        }
    }
}
