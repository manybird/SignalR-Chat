using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Chat.Web.Models;
using Microsoft.AspNetCore.Server.HttpSys;
using System.Web;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Mvc.Filters;
using Chat.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Chat.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    
    public class LoginModel : PageModel
    {
        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string UserName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        private readonly ApplicationDbContext _context;

        public LoginModel(SignInManager<ApplicationUser> signInManager, 
            ILogger<LoginModel> logger, ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context; 
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        private static NameValueCollection GetParameters(string returnUrl)
        {
            var s = returnUrl;
            if (s == null) return null;
            if (s.StartsWith(@"/?"))
            {
                s = s[1..] ;
            }

            return HttpUtility.ParseQueryString(s);
        }
        
        private static string GetUserNameInUrl(string returnUrl)
        {            
            var qs = GetParameters(returnUrl);

            if (qs == null) return null;            
            return qs["na1ta"];
                        

            //var arr = returnUrl.Split("na1ta=");
            //if (arr.Length < 2) return null;

            //var decodedUrl = System.Net.WebUtility.UrlDecode(arr[1]);
            //if (decodedUrl == null) return null;
            //var arr2 = decodedUrl.Split("nata=");
            //if (arr2.Length < 2) return null;

            //return arr[1];
        }


       

        public async Task OnGetAsync(string returnUrl = null, string na1ta = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }
            string conectionString = _context.Database.GetConnectionString();
            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            //var t = await _signInManager.GetExternalLoginInfoAsync();

            var qs = GetParameters(returnUrl);
            string adminId="";
            if (qs != null)
            {
                 adminId= qs["adminId"];

                TempData["adminId"] = adminId;

                if (na1ta == null)
                    na1ta = qs["na1ta"];
            }            

            if (!string.IsNullOrEmpty(na1ta))
            {
                var appUser = await _userManager.FindByNameAsync(na1ta);

                if (appUser == null)
                {
                    appUser = ApplicationUser.FromUserName(na1ta);
                    var result = await _userManager.CreateAsync(appUser);
                    if (result.Succeeded)
                    {

                    }
                    else if (result.Errors != null)
                    {

                    }
                }

                //var tmp = await _userManager.create(appUser);
                //var b = await _signInManager.CanSignInAsync(appUser);
                await _signInManager.SignInAsync(appUser,false);
            }
            
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            // If we got this far, something failed, redisplay form
            if (!ModelState.IsValid) return Page();

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(Input.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();

        }
    }
}
