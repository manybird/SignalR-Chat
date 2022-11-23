using System;

namespace Chat.Web
{
    public class AppSettings
    {
        public string AllowedOrigins { get; set; }

        internal string[] AllowedOriginsArray()
        {
            if (AllowedOrigins == null) return new string[] { };

            return AllowedOrigins.Split(";", StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
