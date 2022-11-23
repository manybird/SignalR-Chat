using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Chat.Web.Data;
using Chat.Web.Hubs;
using Chat.Web.MiccSdk;
using Chat.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Chat.Web.Pages
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken(Order =2000)]
    public class AgentViewModel : PageModelExt
    {
        private readonly ILogger<AgentViewModel> _logger;

        public string AdminId { get; set; }
        public string Na1ta { get; set; }

        public AgentViewModel(ILogger<AgentViewModel> logger, ApplicationDbContext context, IMapper mapper,
            IHubContext<ChatHub> hubContext, UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager, Micc micc)
            : base(context, mapper, hubContext, userManager, roleManager, micc)
        {
            _logger = logger;
        }

        public async Task<IActionResult> OnGet(string adminId)
        {
            

            if (string.IsNullOrEmpty(adminId))
            {
                return Content("adminId empty!");
            }

            var AppUser = await _userManager.FindByIdAsync(adminId);

            if (AppUser == null)
            {
                return Content("User not found: " + adminId);
            }
            
            var runner = new MiccRunner(_micc);
            var r = await runner.GetOpenMediaConversationById(adminId);

            //string url;
            if (!r.IsSuccess)
            {
                return Content(r.GetErrorOrMessage());
            }

            Na1ta = r.AgentReporting;
            AdminId = adminId;
            
            if (string.IsNullOrEmpty(Na1ta))
                return Content("Na1ta empty!");


            var appUser = await _userManager.FindByNameAsync(Na1ta);

            if (appUser == null)
            {
                appUser = ApplicationUser.FromUserName(Na1ta);
                var result = await _userManager.CreateAsync(appUser);
                if (result.Succeeded)
                {
                    return Content("Error create user fail: " + Na1ta);
                }
                else if (result.Errors != null)
                {
                    return Content("Error: " + result.Errors);
                }
            }


            //await _signInManager.SignInAsync(appUser, false);


            //return Content("1");

            TempData["adminId"] = adminId;
            return Page();
        }
    }
}
