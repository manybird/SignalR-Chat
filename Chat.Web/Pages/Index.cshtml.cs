using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Chat.Web.Pages
{
    [Authorize]
    
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
                public string AllowedExtensions { get; set; }
        public string PathBase { get; set; }
        
        public IndexModel(SignInManager<ApplicationUser> signInManager,
            ILogger<IndexModel> logger, IConfiguration configuration,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _signInManager = signInManager;

            AllowedExtensions = configuration.GetValue("FileUpload:AllowedExtensions", ".jpg,.jpeg,.png");
        }

        public void OnGet(string adminId)
        {
            TempData["adminId"] = adminId;
            PathBase = HttpContext.Request.PathBase;
            
        }
    }
}
