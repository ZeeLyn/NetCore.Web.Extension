using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCore.Web.AutoGenerateHtmlControl;
using NetCore.Web.Extension;

namespace WebApplication
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
            services.AddControllersWithViews().AddAutoGenerateForm(builder =>
            {
                builder.AddDataSource<ProfessionDataSource>();
                builder.AddDataSource<HobbyDataSource>();
            });
            //services.AddGlobalExceptionFilter();
            //services.AddGlobalModelStateFilter();
            services.AddJwtBearerAuthentication(options =>
            {
                options.SecurityKey = "8A94FD8A94F122B0";
                options.ValidIssuer = "Issuer";
                options.ValidAudience = "Audience";
            });
            services.AddJwtCookieAuthentication(options =>
            {
                options.SecurityKey = "8A94FDA4354414A0320A72292571DF8BDF3B215B44EC523C5862628A4F77C77E29131382D8937A81A1A0E737406A06C4DE24AB0539375EEE779783F5D4E7FE67";
                options.ValidIssuer = "test1";
                options.ValidAudience = "test2";
                options.Cookie.Name = "access_token";
                options.LoginPath = "/login";
            });

            //services.Configure<MvcRazorRuntimeCompilationOptions>(options => { options.FileProviders.Add(new EmbeddedFileProvider(typeof(FormGeneratorExtension).GetTypeInfo().Assembly)); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
