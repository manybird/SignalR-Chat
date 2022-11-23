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
using Chat.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Chat.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Xml.Linq;

namespace Chat.Web.Controllers
{
    [Authorize]
    //[AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBaseExt
    {
        //private readonly ApplicationDbContext _context;
        //private readonly IMapper _mapper;
        //private readonly IHubContext<ChatHub> _hubContext;
        private readonly ITempDataDictionary _tempData;
        public RoomsController(ApplicationDbContext context,
            IMapper mapper,
            IHubContext<ChatHub> hubContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ITempDataDictionaryFactory tempDataFactory) : base(context, mapper, hubContext,userManager,roleManager)
        {
           // _tempData = tempDataFactory.GetTempData(context.htt);
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<RoomViewModel>>> Get()
        {


            //if (_loginUser == null) return BadRequest("Invalid user");
            ApplicationUser _loginUser = await GetUserByName(User.Identity.Name);
            
            if (_loginUser == null) return BadRequest("Invalid user at room controller!");
            //object adminIdObj = TempData["adminId"];
            //string adminId = null;

            //if (adminIdObj != null) adminId = adminIdObj.ToString();

            return await PostByAdminId(_loginUser.Id,true);
        }



        [AllowAnonymous]
        [HttpGet("ByAdminId/{adminId}")]
        public async Task<ActionResult<IEnumerable<RoomViewModel>>> PostByAdminId(string adminId, bool isFromIdentity = false)
        {
            //ApplicationUser _loginUser;
            //await InitUser();

            List<Room> rooms = new List<Room>();

            try
            {

                if (string.IsNullOrEmpty(adminId))
                {
                    return BadRequest("Invalid user at room controller!");
                }








                rooms = await _context.Rooms.Include(r => r.Admin).Where(r => r.AdminId == adminId).ToListAsync();
                if (rooms.Count > 0) rooms = rooms.Take(1).ToList();

                if (isFromIdentity)
                {
                    if (rooms.Count == 0)
                    {
                        ApplicationUser _loginUser = await _userManager.FindByIdAsync(adminId);

                        if (await _userManager.IsInRoleAsync(_loginUser, ApplicationRole.UserKey))
                        {
                            var room = await CreateNewRoom(_loginUser);
                            rooms.Add(room);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                var ex1 = ex;
            }


            var roomsViewModels = _mapper.Map<IEnumerable<Room>, IEnumerable<RoomViewModel>>(rooms);

            return Ok(roomsViewModels);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> Get(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return NotFound();

            

            var roomViewModel = _mapper.Map<Room, RoomViewModel>(room);
            return Ok(roomViewModel);
        }
        
        [HttpPost]
        public async Task<ActionResult<Room>> Create(RoomViewModel roomViewModel)
        {
            if (roomViewModel.OperationMode!=0) return BadRequest("Invalid Operation Mode!");

            if (_context.Rooms.Any(r => r.Name == roomViewModel.Name))
                return BadRequest("Invalid room name or room already exists");

            //var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var _loginUser = await base.GetUserByName(User.Identity.Name);
            var room = await CreateNewRoom( _loginUser, roomViewModel.Name);
            
            await _hubContext.Clients.All.SendAsync("addChatRoom", new { id = room.Id, name = room.Name });

            return CreatedAtAction(nameof(Get), new { id = room.Id }, new { id = room.Id, name = room.Name });
        }
        private async Task< Room> CreateNewRoom(ApplicationUser user)
        {
            return await CreateNewRoom(user, user.UserName);
        }
        private async Task<Room> CreateNewRoom(ApplicationUser user, string roomName)
        {
            var room = new Room()
            {
                Name = roomName,
                Admin = user
            };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return room;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, RoomViewModel roomViewModel)
        {
            if (_context.Rooms.Any(r => r.Name == roomViewModel.Name))
                return BadRequest("Invalid room name or room already exists");

            var room = await _context.Rooms
                .Include(r => r.Admin)
                .Where(r => r.Id == id && r.Admin.UserName == User.Identity.Name)
                .FirstOrDefaultAsync();

            if (room == null)
                return NotFound();

            room.Name = roomViewModel.Name;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("updateChatRoom", new { id = room.Id, room.Name});

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Admin)
                .Where(r => r.Id == id && r.Admin.UserName == User.Identity.Name)
                .FirstOrDefaultAsync();

            if (room == null)
                return NotFound();

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("removeChatRoom", room.Id);
            await _hubContext.Clients.Group(room.Name).SendAsync("onRoomDeleted", string.Format("Room {0} has been deleted.\nYou are moved to the first available room!", room.Name));

            return Ok();
        }
    }
}
