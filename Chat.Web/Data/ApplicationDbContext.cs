using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using Chat.Web.Models;
using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Chat.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser,ApplicationRole,string, 
        IdentityUserClaim<string>, ApplicationUserRole, IdentityUserLogin<string>,
        IdentityRoleClaim<string>, IdentityUserToken<string>>
    //,IDataProtectionKeyContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ApplicationUser> AppUsers { get; set; }
        public DbSet<ApplicationRole> AppRoles { get; set; }
        public DbSet<ApplicationUserRole> AppUserRoles { get; set; }

        //public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUser>(b =>
            {
                // Each User can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });
            modelBuilder.Entity<ApplicationRole>(b =>
            {
                // Each Role can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
            });
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            //InitDefaultData(modelBuilder);
            //builder.HasDefaultSchema("admin");            
            //builder.Entity("Chat.Web.Models.Room", b =>  b.ToTable("Rooms"));
        }

        /// <summary>
        /// Not working?
        /// </summary>
        /// <param name="model"></param>
        private void InitDefaultData(ModelBuilder model)
        {
            model.Entity<ApplicationRole>(b =>
            {
                b.HasData(new ApplicationRole()
                {
                    Id = "b6dfb977-3a56-434a-a9fd-d2f9ed9f3f98",
                    Name = ApplicationRole.UserKey,
                    NormalizedName = ApplicationRole.UserKey.ToUpper(),
                });

                b.HasData(new ApplicationRole()
                {
                    Id = "673ccfda-56b9-439a-8299-40952a66ff5d",
                    Name = ApplicationRole.AgentKey,
                    NormalizedName = ApplicationRole.AgentKey.ToUpper(),
                });
            });
        }
    }
}
