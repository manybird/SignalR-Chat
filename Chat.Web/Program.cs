using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Web.Services.QueuedBackgroundTask;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Chat.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();                        
            //host.Run();
            host.Start();   

            if (Startup.ex != null)
            {
                Console.WriteLine(Startup.ex.Message);
                Console.WriteLine(Startup.ex.StackTrace);
                Console.ReadLine();
            }
            host.WaitForShutdown();

            //host.Start();                        
            //var monitorLoop = host.Services.GetRequiredService<MonitorLoop>();
            //monitorLoop.StartMonitorLoop();            
            //host.WaitForShutdown();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                return;
                //webBuilder.UseUrls("https://localhost:5051","http://localhost:5050");
                //webBuilder.UseKestrel(opts =>
                //{
                //    opts.ListenAnyIP(5001, opts2 =>
                //    {
                //        opts2.UseHttps();
                //    });
                //});
            });

            return host;
        }
    }
}
