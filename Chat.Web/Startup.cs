using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Chat.Web.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Chat.Web.Hubs;
using Chat.Web.Models;
using AutoMapper;
using Chat.Web.Helpers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Chat.Web.MiccSdk;
using Microsoft.Extensions.Options;
using Chat.Web.Services;
using Chat.Web.Services.QueuedBackgroundTask;
using Chat.Web.Services.Scoped;

namespace Chat.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton(Configuration.GetSection(typeof(Micc).Name).Get<Micc>());
            var appSettings = Configuration.GetSection(typeof(AppSettings).Name).Get<AppSettings>();
            services.AddSingleton(appSettings);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //services.AddDataProtection().PersistKeysToDbContext<ApplicationDbContext>();

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                var signIn = options.SignIn;
                signIn.RequireConfirmedAccount = false;

                var oPassword = options.Password;
                oPassword.RequireNonAlphanumeric = false;
                oPassword.RequiredLength = 4;
                oPassword.RequireDigit = false;
                oPassword.RequireLowercase = false;
                oPassword.RequireUppercase = false;

                var user = options.User;                
                user.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
                user.RequireUniqueEmail = true;
            }).AddRoles<ApplicationRole>().AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAutoMapper(typeof(Startup));
            services.AddTransient<IFileValidator, FileValidator>();

            //services.AddTransient<AppDbContextSeedData>();            

            services.AddRazorPages();
            services.AddControllers();
            services.AddSignalR();

            services.AddAntiforgery(options =>
            {
                //options.Cookie.Expiration = TimeSpan.Zero;
                options.SuppressXFrameOptionsHeader = true;
            });

            services.ConfigureApplicationCookie(options =>
            {                
            });

            //services.AddHostedService<TimedHostedService>();

            //services.AddSingleton<ScopedProcessingService>();
            services.AddHostedService<ConsumeScopedServiceHostedService>();
            services.AddScoped<IScopedProcessingService, ScopedProcessingService>();

            //services.AddSingleton<MonitorWorker>();
            //services.AddHostedService<QueuedHostedService>();
            //services.AddSingleton<IBackgroundTaskQueue>(ctx => new BackgroundTaskQueue(appSettings.QueueCapacity));
            
            //services.AddScoped<IScopedMonitorService, ScopedMonitorService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env
            ,ILoggerFactory logger, AppSettings settings,ApplicationDbContext applicationDbContext)
        {
            //AppDbContextSeedData.SeedData(app.ApplicationServices).Wait();
            

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors( x =>
            {
                x.WithOrigins(settings.AllowedOrigins);
            });
            
            //app.UsePathBase("/chat");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/chatHub");
            });

            AppDbContextSeedData.SeedData(app.ApplicationServices).Wait();

            //MonitorWorker.Run(app.ApplicationServices);

        }
    }
}
