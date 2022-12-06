using AutoMapper;
using Chat.Web.Models;
using Chat.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Web.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ApplicationUser, UserViewModel>()
                .ForMember(dst => dst.Username, opt => opt.MapFrom(x => x.UserName))
                .ForMember(d=>d.IsUserVisiting,o=>o.MapFrom(x=>x.IsUserVisiting));
            CreateMap<UserViewModel, ApplicationUser>();

            CreateMap<ApplicationUser, UserViewModelExt>()
                .ForMember(dst => dst.Username, opt => opt.MapFrom(x => x.UserName))
                .ForMember(dst => dst.AdminId, s => s.MapFrom(x => x.Id))
                .ForMember(d => d.IsUserVisiting, o => o.MapFrom(x => x.IsUserVisiting));
            CreateMap<UserViewModelExt, ApplicationUser>();
        }
    }
}
