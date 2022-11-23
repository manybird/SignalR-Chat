using AutoMapper;
using Chat.Web.Data;
using Chat.Web.Models;
using Chat.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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

        
        private readonly static Dictionary<string, UserViewModelExt> _ConnectionsMap = new Dictionary<string, UserViewModelExt>();

        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(ApplicationDbContext context, IMapper mapper
            , RoleManager<ApplicationRole> roleManager , UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task SendPrivate(string receiverName, string message)
        {
            if (string.IsNullOrEmpty(message.Trim())) return;

            foreach(var kvp in _ConnectionsMap)
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

        /// <summary>
        /// Join room
        /// </summary>
        /// <param name="roomName"></param>
        /// <returns></returns>
        public async Task Join(string roomName)
        {
            try
            {   
                var b = _ConnectionsMap.ContainsKey(ConnectionId);
                if (!b) return;

                var userView = _ConnectionsMap[ConnectionId];
                if (userView != null && userView.CurrentRoom != roomName)
                {
                    // Remove user from others list
                    if (!string.IsNullOrEmpty(userView.CurrentRoom))
                        await Clients.OthersInGroup(userView.CurrentRoom).SendAsync("removeUser", userView);

                    // Join to new chat room
                    await Leave(userView.CurrentRoom);
                    await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                    userView.CurrentRoom = roomName;

                    // Tell others to update their list of users
                    await Clients.OthersInGroup(roomName).SendAsync("addUser", userView);

                    var room = _context.Rooms.FirstOrDefault(r => r.Name == roomName);

                    if (room!=null)
                    {
                        var roomViewModel = _mapper.Map<Room, RoomViewModel>(room);
                        await Clients.Caller.SendAsync("onRoomJoinCompleted", roomViewModel);
                    }                   
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("onError", "You failed to join the chat room!" + ex.Message);
            }
        }

        public async Task Leave(string roomName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }

        public IEnumerable<UserViewModel> GetUsers(string roomName)
        {
            

            return _ConnectionsMap.Values.Where(u => u.CurrentRoom == roomName).ToList();
        }

        public async override Task OnConnectedAsync()
        {
            string na1ta = Context.GetHttpContext().Request.Query["na1ta"];

            //if (string.IsNullOrEmpty(na1ta))
            //{
            //    return base.OnConnectedAsync();
            //}

            string IdentityName = this.GetIdentityName(na1ta);
            try
            {
                var user = _context.Users.Where(u => u.UserName == IdentityName).FirstOrDefault();
                               

                if (user == null)
                {
                    //User not in DB
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

                if (!_ConnectionsMap.ContainsKey(cId))
                    _ConnectionsMap.Add(cId, userViewModel);
                else
                    _ConnectionsMap[cId] = userViewModel;

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
                if (_ConnectionsMap.ContainsKey(ConnectionId))
                {
                    var user = _ConnectionsMap[ConnectionId];
                    if (user != null)
                       await Clients.OthersInGroup(user.CurrentRoom).SendAsync("removeUser", user);

                    _ConnectionsMap.Remove(ConnectionId);
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
            foreach (var kvp in _ConnectionsMap)
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
