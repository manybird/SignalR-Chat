using AutoMapper;
using Chat.Web.Data;
using Chat.Web.Hubs;
using Chat.Web.MiccSdk;
using Chat.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Web.Pages
{
    public class PageModelExt:PageModel
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IMapper _mapper;
        protected readonly IHubContext<ChatHub> _hubContext;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly RoleManager<ApplicationRole> _roleManager;
        protected readonly Micc _micc;

        public PageModelExt(ApplicationDbContext context,
            IMapper mapper,
            IHubContext<ChatHub> hubContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            Micc micc) : base()
        {

            _context = context;
            _mapper = mapper;
            _hubContext = hubContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _micc = micc;

        }
    }
}
