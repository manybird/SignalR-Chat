using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Chat.Web.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : Microsoft.AspNetCore.Mvc.RazorPages.PageModel
    {
        public string RequestId { get; set; }

        
        public string AdminId { get; set; }
        public int? Id2 { get; set; }

        public ICollection<string> Items { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

              

        public void OnGet(string adminId,int? id2)
        {
            this.AdminId = adminId;
            Id2 = id2;
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.Log(LogLevel.Trace,"RequestId: {0}", RequestId);

            Items = new List<string>();

            for(var i=1;i < 10; i++)
            {
                Items.Add(String.Format("Items: {0}", i));
            }

        }
    }
}
