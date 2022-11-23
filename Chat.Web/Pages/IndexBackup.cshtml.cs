using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Chat.Web.Pages
{
    [Authorize]
    public class IndexBackupModel : Microsoft.AspNetCore.Mvc.RazorPages.PageModel
    {
        private readonly ILogger<IndexBackupModel> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public IndexBackupModel(SignInManager<ApplicationUser> signInManager,
            ILogger<IndexBackupModel> logger,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _signInManager = signInManager;            
        }

        public void OnGet()
        {
            
        }
    }
}
