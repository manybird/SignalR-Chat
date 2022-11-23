using AutoMapper;
using Chat.Web.Data;
using Chat.Web.Hubs;
using Chat.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Web.Controllers
{
    public class ControllerBaseExt:Controller
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IMapper _mapper;
        protected readonly IHubContext<ChatHub> _hubContext;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly RoleManager<ApplicationRole> _roleManager;
        
        public ControllerBaseExt(ApplicationDbContext context,
            IMapper mapper,
            IHubContext<ChatHub> hubContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager) :base()
        {
            
            _context = context;
            _mapper = mapper;
            _hubContext = hubContext;
            _userManager = userManager;
            _roleManager = roleManager;

            
        }

        

        protected async Task< ApplicationUser> GetUserByName(string name, bool createIfNotExits = false)
        {
            var user = await _userManager.FindByNameAsync(name); 
            //_context.Users.Where(u => u.UserName == name).FirstOrDefault();

            if (user == null && createIfNotExits)
            {
                //User not in DB
                user = ApplicationUser.FromUserName(name);
                
            }
            return user;
        }

        //public bool IsRole(string roleId)
        //{
            
        //    var UserRoles = _context.AppUserRoles;
        //    if (UserRoles == null || UserRoles.Count() == 0) return false;

        //    var userrole = GetMyRoles()
        //    .Where(c => c.RoleId==roleId)
        //    .FirstOrDefault();

        //    return userrole!=null;            
        //}

        //public IQueryable<ApplicationUserRole> GetMyRoles(string userId = null)
        //{

        //    var UserRoles = _context.AppUserRoles;
        //    if (UserRoles == null || UserRoles.Count() == 0) return null;

        //    if (userId == null && _loginUser!=null) userId = _loginUser.Id;

        //    var userrole = UserRoles.Include(c => c.User)
        //    .Include(c => c.Role)
        //    .Where(c => c.UserId == userId);
            
        //    return userrole;
        //}

    }
}
