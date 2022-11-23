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
using System.Web;

namespace Chat.Web.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    //[ApiController]
    public class TargetController : ControllerBaseExt
    {
        private readonly Micc _micc;

        public TargetController(ApplicationDbContext context,
            IMapper mapper, IHubContext<ChatHub> hubContext,
            UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,Micc micc) 
            : base(context, mapper, hubContext, userManager, roleManager) {
            _micc = micc;
        }

        [HttpGet("error/{errorMsg}")]
        public IActionResult ShowErrorMsg(string errorMsg)
        {
            return Ok(errorMsg);

            //return LocalRedirect(returnUrl);
        }

        [HttpGet]
        public  IActionResult OnGet(string adminId)
        {
            return Ok("ok");

            //return LocalRedirect(returnUrl);
        }

        [HttpGet("{adminId}")]
        public async Task<ActionResult> OnMiccLoginRequest(string adminId)
        {
            var runner = new MiccRunner(_micc);

            var r = await runner.GetOpenMediaConversationById(adminId);

            string url;
            if (r.IsSuccess)
            {               
                //url = string.Format("{0}/index?na1ta={1}&adminId={2}", _micc.OpenMediaRequestBodyDefault.UrlBase, r.AgentReporting,adminId);
                url = string.Format("/?na1ta={0}&adminId={1}", r.AgentReporting, adminId);
                return Redirect(url);
            }
                //url = string.Format("/Target/error/{0}",HttpUtility.UrlEncodeUnicode(r.Error));
            //return Content(string.Format("<root>{0}</root>",r.Error),"application/xml");

            return Content(r.GetErrorOrMessage());
           
            

            //return LocalRedirect(returnUrl);
        }


    }
}
