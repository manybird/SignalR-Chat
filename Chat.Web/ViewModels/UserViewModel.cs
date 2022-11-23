using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Web.ViewModels
{
    public class UserViewModel
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public string CurrentRoom { get; set; }
        public string Device { get; set; }

        public int IsUserVisiting { get; set; }
                
        internal static UserViewModel ByUserName(string identityName)
        {
            return new UserViewModel()
            {
                Username = identityName,
                FullName = identityName + "@",
            };
        }


    }
    public class UserViewModelExt:UserViewModel
    {
        public string ConnectionId { get; set; }
    }
}
