using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Chat.Web.Helpers;

namespace Chat.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBaseExt
    {
        private readonly Micc _micc;

        public MessagesController(ApplicationDbContext context,
            IMapper mapper, IHubContext<ChatHub> hubContext,
            UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,Micc micc) 
            : base(context, mapper, hubContext, userManager, roleManager) {
            _micc = micc;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> Get(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return NotFound();

            var messageViewModel = _mapper.Map<Message, MessageViewModel>(message);
            return Ok(messageViewModel);
        }

        
        [HttpGet("Room/{roomName}/{caseId}")]
        public IActionResult GetMessagesA(string roomName,string caseId)
        {
            return GetMessagesInner(roomName, caseId);
        }
        
        [HttpGet("Room/{roomName}/{caseId}/{take}")]
        public IActionResult GetMessagesA(string roomName, string caseId, int take)
        {
            return GetMessagesInner(roomName, caseId, take);
        }

        [AllowAnonymous]
        [HttpGet("RoomB/{roomName}/{caseId}")]
        public IActionResult GetMessagesB(string roomName, string caseId)
        {
            return GetMessagesInner(roomName, caseId);
        }

        [AllowAnonymous]
        [HttpGet("RoomB/{roomName}/{caseId}/{take}")]
        public IActionResult GetMessagesB(string roomName, string caseId, int take)
        {
            return GetMessagesInner(roomName, caseId, take);
        }
                
        private IActionResult GetMessagesInner(string roomName,string caseId,int take=1000)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Name == roomName);
            if (room == null)
                return BadRequest();

            var messages = _context.Messages.Where(m => m.ToRoomId == room.Id && m.CaseId == caseId)
                .Include(m => m.FromUser)
                .Include(m => m.ToRoom)
                //.Include(m=>m.Case)
                .OrderByDescending(m => m.Timestamp)
                .Take(take)
                .AsEnumerable()
                .Reverse()
                .ToList();

            var messagesViewModel = _mapper.Map<IEnumerable<Message>, IEnumerable<MessageViewModel>>(messages);

            return Ok(messagesViewModel);
        }

        [HttpPost]
        public async Task<ActionResult<Message>> Create(MessageViewModel messageViewModel)
        {
            var user = await base.GetUserByName(User.Identity.Name);
            return await CreateMessageInner(messageViewModel,user);
        }

        

        [AllowAnonymous]
        [HttpPost("ByAgent/{na1ta}")]
        public async Task<ActionResult<Message>> CreateByAgent(MessageViewModel messageViewModel,string na1ta)
        {
            var user = await base.GetUserByName(na1ta);
            return await CreateMessageInner(messageViewModel,user);
        }

        private async Task<ActionResult<Message>> CreateMessageInner(MessageViewModel messageViewModel, ApplicationUser user)
        {

            var room = _context.Rooms.FirstOrDefault(r => r.Name == messageViewModel.Room);
            if (room == null)
                return BadRequest();

            var msg = new Message()
            {
                Content = Regex.Replace(messageViewModel.Content, @"<.*?>", string.Empty),
                FromUser = user,
                ToRoom = room,
                Timestamp = DateTime.Now,
                CaseId = messageViewModel.CaseId,
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            // Broadcast the message
            var createdMessage = _mapper.Map<Message, MessageViewModel>(msg);
            await _hubContext.Clients.Group(room.Name).SendAsync("newMessage", createdMessage);

            return CreatedAtAction(nameof(Get), new { id = msg.Id }, createdMessage);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _context.Messages
                .Include(u => u.FromUser)
                .Where(m => m.Id == id && m.FromUser.UserName == User.Identity.Name)
                .FirstOrDefaultAsync();

            if (message == null)
                return NotFound();

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("removeChatMessage", message.Id);

            return Ok();
        }
       

    }
}
