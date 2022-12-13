using AutoMapper;
using Chat.Web.Data;
using Chat.Web.Helpers;
using Chat.Web.MiccSdk;
using Chat.Web.Models;
using Chat.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chat.Web.Hubs
{
    //[Authorize]
    [AllowAnonymous]
    public class ChatHub : Hub
    {
        //public readonly static List<UserViewModel> _Connections = new List<UserViewModel>();
        //private readonly static Dictionary<string, string> _ConnectionsMap = new Dictionary<string, string>();

        public delegate void OnConnectionChangedEventHandler(ChatHub sender, UserViewModelExt userViewModelExt,bool isAdd);
        public static event OnConnectionChangedEventHandler OnConnectionChanged;

        private static void ConnectionChanged(ChatHub sender, UserViewModelExt userViewModelExt, bool isAdd)
        {
            if (OnConnectionChanged == null) return;
            OnConnectionChanged(sender, userViewModelExt,isAdd);
        }

        private readonly static Dictionary<string, UserViewModelExt> _connectionsMap = new Dictionary<string, UserViewModelExt>();
        public static Dictionary<string, UserViewModelExt> GetConnectionsMap()
        {
            lock(_connectionsMap)
            {
                return _connectionsMap;
            }
        }

        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Micc _micc;
        private readonly ILogger<ChatHub> _logger;
        public ChatHub(ApplicationDbContext context, IMapper mapper
            , RoleManager<ApplicationRole> roleManager , UserManager<ApplicationUser> userManager
            ,Micc micc, ILogger<ChatHub> logger)
        {
            _context = context;
            _mapper = mapper;
            _roleManager = roleManager;
            _userManager = userManager;
            _micc = micc;
            _logger = logger;
        }

        public async Task SendPrivate(string receiverName, string message)
        {
            if (string.IsNullOrEmpty(message.Trim())) return;

            foreach(var kvp in _connectionsMap)
            {
                var sender = kvp.Value;
                if (sender == null) continue;

                if (!string.Equals(receiverName, sender.Username, StringComparison.OrdinalIgnoreCase)) continue;    

                string connectionId = kvp.Key;

                 // _Connections.Where(u => u.Username == GetIdentityNameByConnectionId(Context.ConnectionId)).FirstOrDefault();
                                 
                                
                // Build the message
                var messageViewModel = new MessageViewModel()
                {
                    Content = Regex.Replace(message, @"<.*?>", string.Empty),
                    From = sender.FullName,
                    Avatar = sender.Avatar,
                    Room = "",
                    Timestamp = DateTime.Now
                };

                // Send the message
                await Clients.Client(connectionId).SendAsync("newMessage", messageViewModel);
                await Clients.Caller.SendAsync("newMessage", messageViewModel);
            }            
        }

        private string ConnectionId { get { return Context.ConnectionId; } }

        private async Task SendErrorToCaller(string msg)
        {
            await Clients.Caller.SendAsync("onError", msg);
        }

        /// <summary>
        /// Join room
        /// </summary>
        /// <param name="roomName"></param>
        /// <returns></returns>
        public async Task Join(string roomName)
        {
            try
            {   
                var b = _connectionsMap.ContainsKey(ConnectionId);
                if (!b) return;

                var userView = _connectionsMap[ConnectionId];
                if (userView == null) return;

                if (userView.CurrentRoom != roomName)
                {
                    // Remove user from others list
                    if (!string.IsNullOrEmpty(userView.CurrentRoom))
                        await Clients.OthersInGroup(userView.CurrentRoom).SendAsync("removeUser", userView);

                    // Join to new chat room
                    await Leave(userView.CurrentRoom);
                    await Groups.AddToGroupAsync(ConnectionId, roomName);
                    userView.CurrentRoom = roomName;

                    // Tell others to update their list of users
                    await Clients.OthersInGroup(roomName).SendAsync("addUser", userView);

                    var room = _context.Rooms.FirstOrDefault(r => r.Name == roomName);

                    if (room == null)
                    {
                        await SendErrorToCaller("Get room fail: " + roomName);
                        return;
                    }

                    Case c1 = _context.Cases.FirstOrDefault(
                         r => r.AdminId == room.AdminId && r.CaseCompletionDate == null
                    );

                    var isUser = IsUser;

                    if (isUser)
                    {
                        // Only user can create case
                        if (c1 != null)
                        {
                            var runner = new MiccRunner(_micc);
                            var r = await runner.GetConversationById(c1.Id);
                            //string url;
                            if (r.IsSuccess)
                            {
                                if (r.IsCompleted(_micc.CompletedCaseFolders))
                                {
                                    c1.CaseCompletionDate = r.LastAgentActionDate?? DateTime.Now;                                    
                                    c1.Folder = r.Folder;
                                    await _context.SaveChangesAsync();
                                    c1 = null;
                                }
                            }
                            else if (r.StatusCode == 404)
                            {
                                //Keep use this caseId if 404/not found, the case should not started 
                            }
                            else
                            {
                                
                                _logger.LogWarning("GetConversationById fail, complete it in DB. Id: {t0}", c1.Id);
                                c1.CaseCompletionDate = DateTime.Now;
                                await _context.SaveChangesAsync();
                                c1 = null;                                 
                                //await SendErrorToCaller("GetConversation fail! " );
                                //return;
                            }
                        }

                        if (c1 == null)
                        {

                            c1 = new Case(Guid.NewGuid().ToString())
                            {
                                AdminId = userView.AdminId,
                                RoomId = room.Id,
                            };

                            _context.Cases.Add(c1);
                            await _context.SaveChangesAsync();
                        }
                    }
                                           
                    var roomViewModel = _mapper.Map<Room, RoomViewModel>(room);

                    if (c1 != null) roomViewModel.CaseId = c1.Id;
                    if (IsUser) UpdateCaseID(roomViewModel);
                    
                    await Clients.Caller.SendAsync("onRoomJoinCompleted", roomViewModel);

                }
            }
            catch (Exception ex)
            {                
                await SendErrorToCaller("Failed to join room! " + ex.Message);
            }
        }

        private void UpdateCaseID(RoomViewModel roomViewModel)
        {
            UserViewModelExt userViewModel = null;
            lock (_connectionsMap)
            {
                if (_connectionsMap.ContainsKey(ConnectionId))
                {
                    userViewModel = _connectionsMap[ConnectionId];
                    userViewModel.CaseId = roomViewModel.CaseId;
                }
            }

            if (userViewModel == null) return;
            ConnectionChanged(this, userViewModel, true);
        }

        public async Task Leave(string roomName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }

        public IEnumerable<UserViewModel> GetUsers(string roomName)
        {
            

            return _connectionsMap.Values.Where(u => u.CurrentRoom == roomName).ToList();
        }

        private string Na1ta { 
            get
            {
                return Context.GetHttpContext().Request.Query["na1ta"];
            } 
        }
        private bool IsUser
        {
            get
            {
                return string.IsNullOrEmpty(Na1ta);
            }
        }

        public async override Task OnConnectedAsync()
        {            

            //if (string.IsNullOrEmpty(na1ta))
            //{
            //    return base.OnConnectedAsync();
            //}

            string IdentityName = this.GetIdentityName(Na1ta);
            try
            {
                var user = _context.Users.Where(u => u.UserName == IdentityName).FirstOrDefault();
                
                if (user == null)
                {
                    //User not in DB, should not equal to null
                    //return;
                    user = ApplicationUser.FromUserName(IdentityName);
                }else if (await _userManager.IsInRoleAsync(user, ApplicationRole.UserKey))
                {
                    user.IsUserVisiting = 1;
                }

                var userViewModel = _mapper.Map<ApplicationUser, UserViewModelExt>(user);

                var cId = Context.ConnectionId;

                userViewModel.Device = GetDevice();
                userViewModel.CurrentRoom = "";
                userViewModel.ConnectionId = cId;

                //var r = _Connections.RemoveAll(u => string.Equals(u.Username, IdentityName, StringComparison.InvariantCultureIgnoreCase));

                //_Connections.Add(userViewModel);

                if (!_connectionsMap.ContainsKey(cId))
                    _connectionsMap.Add(cId, userViewModel);
                else
                    _connectionsMap[cId] = userViewModel;

                ConnectionChanged(this, userViewModel,true);

                //Clients.Caller.SendAsync("getProfileInfo", user.FullName, user.Avatar);
                await Clients.Caller.SendAsync("getProfileInfo", userViewModel);
            }
            catch (Exception ex)
            {
               await Clients.Caller.SendAsync("onError", "OnConnected:" + ex.Message);
            }
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            try
            {                

                //string IdentityName = this.GetIdentityNameByConnectionId(Context.ConnectionId);
                //var ConnectionId = Context.ConnectionId;
                
                // Remove mapping
                if (_connectionsMap.ContainsKey(ConnectionId))
                {
                    var user = _connectionsMap[ConnectionId];
                    if (user != null)
                       await Clients.OthersInGroup(user.CurrentRoom).SendAsync("removeUser", user);

                    _connectionsMap.Remove(ConnectionId);
                    ConnectionChanged(this, user, false);
                }
                
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("onError", "OnDisconnected: " + ex.Message);
            }

            await base.OnDisconnectedAsync(exception);
        }


        #region "functions"
        private string GetIdentityNameByConnectionId(string ConnectionId)
        {
            string na1ta = "";
            foreach (var kvp in _connectionsMap)
            {
                if (!string.Equals(ConnectionId, kvp.Key, StringComparison.InvariantCultureIgnoreCase)) continue;
                na1ta = kvp.Value.Username;
                break;
            }

            return GetIdentityName(na1ta);
        }

        private string GetIdentityName(string na1ta)
        {
            if (!string.IsNullOrEmpty(na1ta)) return na1ta;

            return Context.User.Identity.Name;
        }

        private string GetDevice()
        {
            var device = Context.GetHttpContext().Request.Headers["Device"].ToString();
            if (!string.IsNullOrEmpty(device) && (device.Equals("Desktop") || device.Equals("Mobile")))
                return device;

            return "Web";
        }
        #endregion

    }
}
