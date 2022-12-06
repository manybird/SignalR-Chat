using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Chat.Web.Data;
using Chat.Web.Hubs;
using Chat.Web.MiccSdk;
using Chat.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chat.Web.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class PreviewModel : PageModelExt
    {
        public string msg { get; set; }
        public int? Take { get; set; }        
        public string AdminId { get; set; }
        public ApplicationUser AppUser { get; set; }

        public ICollection<MessageExt> Messages { get; set; }
        public ICollection<string> Items { get; set; }
        
        private readonly ILogger<PreviewModel> _logger;
        public PreviewModel(ILogger<PreviewModel> logger, ApplicationDbContext context, IMapper mapper,
            IHubContext<ChatHub> hubContext, UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager, Micc micc ) 
            :base(context, mapper, hubContext, userManager, roleManager,micc)
        {
            _logger = logger;
        }              

        public async Task OnGet(string adminId, string caseId, int? take)
        {
            Take= take;
            this.AppUser = await _userManager.FindByIdAsync(adminId);
            this.Messages = new List<MessageExt>();
            if (AppUser==null && take==null)
            {
                msg = "User not found!";
                return;
            }
            //var list = await _context.Rooms.ToListAsync();
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.AdminId == adminId);
            
            if (room == null)
            {
                msg = "No room found!";
                return;
            }

             var  messages = _context.Messages.Where(m => m.ToRoomId == room.Id && m.CaseId == caseId)
                .Include(m => m.FromUser)
                .Include(m => m.ToRoom)
                .OrderByDescending(m => m.Timestamp)
                .Take(take??10)
                .AsEnumerable()
                .Reverse()
                .ToList();

            this.Messages = _mapper.Map<ICollection<Message>, ICollection<MessageExt>>(messages);

            foreach(var m in Messages)
            {
                if (m.FromUser == null) continue;
                var u = m.FromUser;

                m.IsFromUserRole = await _userManager.IsInRoleAsync(m.FromUser, ApplicationRole.UserKey);

            }

        }
    }
}
