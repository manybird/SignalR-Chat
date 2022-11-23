using System;
using System.Collections.Generic;

namespace Chat.Web
{
    public class AppSettings
    {
        public string[] AllowedOrigins { get; set; }
        public int QueueCapacity { get; set; }
        public int? ScopedServiceWaitTimeInSecond { get; set; }
        
        //internal string[] AllowedOriginsArray()
        //{
        //    if (AllowedOrigins == null) return new string[] { };

        //    return AllowedOrigins.Split(";", StringSplitOptions.RemoveEmptyEntries);
        //}
    }
}
