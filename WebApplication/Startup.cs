using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCore.Web.Extension;
using System;

namespace WebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new DateOnlyConverter());
                options.SerializerSettings.Converters.Add(new TimeOnlyConverter());
                options.SerializerSettings.DateFormatString = "yyyy/MM/dd HH:mm:ss";
            });
            services.AddGlobalExceptionFilter();
            services.AddGlobalModelStateFilter();

            // 演示环境允许每次启动生成密钥；实际部署需从安全配置读取固定密钥。
            var securityKey = Configuration["Authentication:SecurityKey"] ?? JwtGenerator.GenerateSecurityKey();
            services.AddMultiSchemePolicy(cookie =>
            {
                cookie.LoginPath = "/login";
                cookie.ExpireTimeSpan = TimeSpan.FromHours(2);
                cookie.SlidingExpiration = true;
            }, jwt =>
            {
                jwt.SecurityKey = securityKey;
                jwt.ValidIssuer = "test1";
                jwt.ValidAudience = "test2";
            }, shareToken: false);
            services.AddJwtSlidingExpiration();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseJwtSlidingExpiration();
            app.UseEndpoints(endpoints => endpoints.MapControllerRoute(
                name: "default", pattern: "{controller=Home}/{action=Index}/{id?}"));
        }
    }
}
