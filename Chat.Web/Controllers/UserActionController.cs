using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Chat.Web.Data;
using Chat.Web.Models;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Chat.Web.Hubs;
using Chat.Web.ViewModels;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Chat.Web.MiccSdk;
using Chat.Web.MiccSdk.OpenMedia;
using NuGet.Packaging.Signing;
using Chat.Web.MiccSdk.Conversation;

using Microsoft.Extensions.Logging;
using Chat.Web.Helpers;
//using NuGet.Protocol.Plugins;

namespace Chat.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserActionController : ControllerBaseExt
    {
        private readonly Micc _micc;
        //private readonly MonitorWorker _monitorLoop;
        //private readonly ScopedProcessingService _scopedProcessingService;
        
        private readonly ILogger<UserActionController> _logger;

        public UserActionController(ApplicationDbContext context,
            //MonitorWorker monitorLoop,
            //ScopedProcessingService scopedProcessingService,
            IMapper mapper, IHubContext<ChatHub> hubContext, ILogger<UserActionController> logger,
            UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,Micc micc) 
            : base(context, mapper, hubContext, userManager, roleManager) {
            _micc = micc;
            //_monitorLoop = monitorLoop;
            //_scopedProcessingService = scopedProcessingService;
            _logger = logger;
        }

                
        private async Task<IActionResult> RequestInner(MessageUserActionViewModel messageView, bool isNeedMakeRequest)
        {   
            var room = _context.Rooms.FirstOrDefault(r => r.AdminId== messageView.AdminId);
            if (room == null) return BadRequest("Room not found");
            
            var runner = new MiccRunner(_micc);


            var responseResult = new ResponseResultConversation();
            var user = await base.GetUserByName(User.Identity.Name);
            var msg = new MessageUserAction()
            {
                //Content = "Request completed! Waiting in queue 1",
                FromUser = user,
                ToRoom = room,                
                Timestamp = DateTime.Now
            };

            ResponseResultOpenMediaConversation responseResultPost = null;

            if (isNeedMakeRequest)            
                responseResultPost = await runner.PostOpenMediaConversation(user.Id, messageView.CaseId, user.FullName, user.Email);            
            
            ResponseResultConversation resultConversation = null;
            if (!isNeedMakeRequest || ( responseResultPost != null && responseResultPost.IsSuccess))
            {                
                responseResult = await runner.GetConversationById(messageView.CaseId);
                resultConversation = responseResult;

                if (resultConversation == null)
                {
                    //should not null
                    return BadRequest("resultConversation is null");
                }

                if (responseResult.IsSuccess )
                {
                    msg.SetResult(responseResult);
                }else if (responseResult.StatusCode== 404)
                {
                    //404 should be post new conversation success
                    msg.SetSuccess();
                }
                else
                    msg.SetResult(responseResult);
            }
            else if (responseResultPost!=null)
            {
                msg.SetResult(responseResultPost);
            }

            // Broadcast the message
            var userActionViewModel = _mapper.Map<MessageUserAction, MessageUserActionViewModel>(msg);
            userActionViewModel.TempSystemMessage = 1;
            if (resultConversation != null) userActionViewModel.Conversation = resultConversation;

            //await _hubContext.Clients.Group(user.UserName).SendAsync("newSystemMessage", userActionViewModel);
            if (isNeedMakeRequest)
            {
                var c1 = await _context.Cases.FirstOrDefaultAsync(r => r.Id == messageView.CaseId && r.CaseStartingDate==null);
                if (c1 != null)
                {
                    c1.CaseStartingDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                await _hubContext.Clients.Client(messageView.ConnectionId).SendAsync("newSystemMessage", userActionViewModel);
            }                

            return Ok(userActionViewModel);
        }

        [HttpPost("RequestCustomerService")]
        public async Task<IActionResult> RequestCustomerService(MessageUserActionViewModel messageView)
        {            
            return await RequestInner(messageView, true);
           
        }

        [HttpPost("RequestMonitorState")]
        public async Task<IActionResult> RequestMonitorState(MessageUserActionViewModel messageView)
        {
            return await RequestInner(messageView, false);           
        }

        [HttpPost("ShowOrderDetail")]
        public async Task<IActionResult> ShowOrderDetail(MessageViewModel messageView)
        {
            //_logger.LogInformation("ChatHub.ConnectionsMap: {0}", ChatHub._connectionsMap.Count);
            //await _monitorLoop.QueueItemAsync();
            //_scopedProcessingService.UpdateConnectionList(ChatHub.GetConnectionsMap());

            var user = await base.GetUserByName(User.Identity.Name);
            var room = _context.Rooms.FirstOrDefault(r => r.AdminId == messageView.AdminId);
            if (room == null)
                return BadRequest("Room not found");

            var msg = new Message()
            {
                Content = "No order found!",
                FromUser = user,
                ToRoom = room,
                Timestamp = DateTime.Now
            };

            // Broadcast the message
            var newViewModel = _mapper.Map<Message, MessageViewModel>(msg);
            newViewModel.TempSystemMessage = 1;
            await _hubContext.Clients.Group(room.Name).SendAsync("newSystemMessage", newViewModel);

            return Ok(newViewModel);
        }

        


    }
}
