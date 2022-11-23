using AutoMapper;
using Chat.Web.MiccSdk.OpenMedia;
using Chat.Web.Models;
using Chat.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Web.Mappings
{
    public class OpenMediaRequestBodyProfile : Profile
    {
        public OpenMediaRequestBodyProfile()
        {
            CreateMap<OpenMediaRequestBody, OpenMediaRequestBody>();
            //CreateMap<OpenMediaRequestBody, OpenMediaRequestBody>();
        }
    }
}
