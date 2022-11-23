using AutoMapper;
using Chat.Web.Helpers;
using Chat.Web.Models;
using Chat.Web.ViewModels;
using System.Security.Cryptography;

namespace Chat.Web.Mappings
{
    public class UserActionProfile:Profile
    {
        public UserActionProfile()
        {
            CreateMap<MessageUserAction, MessageUserActionViewModel>()
                .ForMember(dst => dst.From, opt => opt.MapFrom(x => x.FromUser.FullName))
                .ForMember(dst => dst.Room, opt => opt.MapFrom(x => x.ToRoom.Name))
                .ForMember(dst => dst.Avatar, opt => opt.MapFrom(x => x.FromUser.Avatar))
                .ForMember(dst => dst.Content, opt => opt.MapFrom(x => BasicEmojis.ParseEmojis(x.Content)));

            CreateMap<MessageUserActionViewModel, MessageUserAction>();
        }
    }
}
