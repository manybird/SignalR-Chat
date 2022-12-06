using System;

namespace Chat.Web.Helpers
{
    public class Tools
    {
        public static bool Equal(string a, string b)
        {
            return string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
