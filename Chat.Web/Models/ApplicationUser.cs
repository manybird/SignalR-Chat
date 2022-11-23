using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;


namespace Chat.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        //[TempData] public string TempAdminId { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public ICollection<Room> Rooms { get; set; }
        public ICollection<Message> Messages { get; set; }
        public int IsUserVisiting;
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

        public static ApplicationUser FromUserName(string name)
        {
            
            return new ApplicationUser()
            {
                UserName = name,
                FullName = name + "@Staff",
                Email = name + "@sample.cc",
            };
        }

        
    }

    public class ApplicationRole:IdentityRole
    {
        public ApplicationRole():base()
        {
            
        }
        public ApplicationRole(string name):base()
        {
            Name = name;
            NormalizedName = name.ToUpper();
        }

        public static readonly string AdminKey = "admin";
        public static readonly string UserKey = "user";
        public static readonly string AgentKey = "agent";
        

        public static readonly string[] RoleAll = { AdminKey, UserKey, AgentKey };
        //public string RoleName { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    }

    public class ApplicationUserRole : IdentityUserRole<string>
    {
        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationRole Role { get; set; }
    }
}
