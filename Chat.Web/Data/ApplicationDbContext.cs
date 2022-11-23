using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Chat.Web.Models;
using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

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

        public DbSet<Case> Cases { get; set; }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ApplicationUser> AppUsers { get; set; }
        public DbSet<ApplicationRole> AppRoles { get; set; }
        public DbSet<ApplicationUserRole> AppUserRoles { get; set; }
        //public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Case>(b =>
            {
                b.Property(s => s.CaseDate).HasDefaultValueSql("getdate()");
            });

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

        public async Task< Case> GetCaseByIdASync(string caseId)
        {
            return await Cases.FirstOrDefaultAsync(r => r.Id == caseId);
        }
        public async Task<Case> GetCaseByAdminAndCaseIdAsync(string adminId, string caseId)
        {
            return await Cases.FirstOrDefaultAsync(r => r.Id == caseId && r.AdminId == adminId);
        }

        public async Task<Case> GetNotCompletedCaseByAdminIdAsync(string adminId)
        {
            return await Cases.FirstOrDefaultAsync(r => r.AdminId == adminId && r.CaseCompletionDate == null);
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
