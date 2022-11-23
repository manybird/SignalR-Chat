using Chat.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Web.Data
{
    public class AppDbContextSeedData
    {
        //private ApplicationDbContext _context;
        //private readonly UserManager<ApplicationUser> _userManager;

        //public AppDbContextSeedData(ApplicationDbContext context,UserManager<ApplicationUser> userManager)
        //{
        //    _context = context;
        //    _userManager = userManager;
        //}

        public async static Task SeedData(IServiceProvider services)
        {
            using (var scope = services.CreateScope())
            {
                UserManager<ApplicationUser> _userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                ApplicationDbContext _context = scope.ServiceProvider.GetService<ApplicationDbContext>();

                //Auto Create db?
                await _context.Database.MigrateAsync();

                RoleManager<ApplicationRole> _roleManager = scope.ServiceProvider.GetService<RoleManager<ApplicationRole>>();

                string[] urAll = ApplicationRole.RoleAll;

                //var roleStore = new RoleStore<ApplicationRole>(_context);
                //var roles = await _context.Roles.ToListAsync();

                foreach (var ur in urAll)
                {
                    //if (_context.Roles.Any(r => r.Name == ur)) continue;

                    if (await _roleManager.RoleExistsAsync(ur)) continue;
                    await _roleManager.CreateAsync(new ApplicationRole(ur)
                    {
                        Id = Guid.NewGuid().ToString()
                    });
                    //await _context.Roles.AddAsync();
                    //await _context.SaveChangesAsync();
                    //await roleStore.CreateAsync(new ApplicationRole(ur));
                }

                await AddUser(_userManager, _context, "admin", "superA", urAll[0]);
                await AddUser(_userManager, _context, "user01", "password", urAll[1]);

                await _context.SaveChangesAsync();
            }

           
        }

        private async static Task AddUser(UserManager<ApplicationUser> _userManager, ApplicationDbContext _context, 
            string userName, string up, string role)
        {
            if (_context.Users.Any(u => u.UserName == userName)) return;

            var um = userName + "@default.p";
            var user = new ApplicationUser()
            {
                Id = Guid.NewGuid().ToString(),
                UserName = userName,
                FullName = um,
                NormalizedUserName = userName.ToUpper(),
                Email = um,
                NormalizedEmail = um.ToUpper(),
                EmailConfirmed = true,
                LockoutEnabled = false,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            //var password = new PasswordHasher<ApplicationUser>();
            //var hashed = password.HashPassword(user, up);
            //user.PasswordHash = hashed;
            //var userStore = new UserStore<ApplicationUser>(_context);
            //await userStore.CreateAsync(user);
            //await userStore.AddToRoleAsync(user, urAll[0]);
            //await _context.Users.AddAsync(user);
            //var user = new ApplicationUser { UserName = Input.UserName, Email = Input.Email, FullName = Input.FullName };
            var result = await _userManager.CreateAsync(user, up);
           // var result = await _context.Users.AddAsync(user);
            if (result.Succeeded &&!string.IsNullOrEmpty(role))
                await _userManager.AddToRoleAsync(user, role);

        }
    }
}
