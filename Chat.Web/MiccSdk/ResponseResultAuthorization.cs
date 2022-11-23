using AutoMapper.Configuration.Conventions;
using System;

namespace Chat.Web.MiccSdk
{
    public class ResponseResultServerStatus : ResponseResult
    {
        
        public override ResponseLinks _links { get; set; }
        public TimeSpan ServerUpTime { get; set; }
        public DateTime ServerTime { get; set; }
        public TimeSpan ApplicationUpTime { get; set; }

        public bool IsReady { get; set; }
        public bool IsRealtimeConnected { get; set; }
        public bool IsDataAccessConnected { get; set; }
                
        public override bool IsSuccess
        {
            get
            {
                return IsSuccessStatusCode && IsReady;
            }
        }

        
    }
}
