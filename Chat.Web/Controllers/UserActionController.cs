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

namespace Chat.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserActionController : ControllerBaseExt
    {
        private readonly Micc _micc;

        public UserActionController(ApplicationDbContext context,
            IMapper mapper, IHubContext<ChatHub> hubContext,
            UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,Micc micc) 
            : base(context, mapper, hubContext, userManager, roleManager) {
            _micc = micc;
        }


        #region "UserAction"
               

        [HttpPost("RequestCustomerService")]
        public async Task<IActionResult> RequestCustomerService(MessageViewModel messageView)
        {
            var user = await base.GetUserByName(User.Identity.Name);
            var room = _context.Rooms.FirstOrDefault(r => r.AdminId == messageView.AdminId);
            if (room == null)
                return BadRequest("Room not found");

            var runner = new MiccRunner(_micc);
            

            var responseResult=new ResponseResultConversation();

            MessageUserAction msg = new MessageUserAction()
            {
                //Content = "Request completed! Waiting in queue 1",
                FromUser = user,
                ToRoom = room,
                Timestamp = DateTime.Now
            };

            responseResult = await runner.PostOpenMediaConversation(user.Id, user.FullName, user.Email);

            if (responseResult.IsSuccess)
            {
                responseResult = await runner.GetOpenMediaConversationById(user.Id);
            }

            msg.SetResult(responseResult);

            
            // Broadcast the message
            var userActionViewModel = _mapper.Map<MessageUserAction, MessageUserActionViewModel>(msg);
            userActionViewModel.TempSystemMessage = 1;
            //newViewModel.AdminId = messageView.AdminId;

            await _hubContext.Clients.Group(room.Name).SendAsync("newSystemMessage", userActionViewModel);

            return Ok(userActionViewModel);
        }

        [HttpPost("ShowOrderDetail")]
        public async Task<IActionResult> ShowOrderDetail(MessageViewModel messageView)
        {
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

        #endregion


    }
}
