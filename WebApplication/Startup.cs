﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCore.Web.AutoGenerateHtmlControl;
using NetCore.Web.Extension;
using System;
using System.IO;
using UploadMiddleware.Core;
using UploadMiddleware.LocalStorage;

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
            services.AddControllersWithViews();
            //services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
            //{
            //    options.FileProviders.Add(new EmbeddedFileProvider(typeof(UploaderContext).GetTypeInfo().Assembly));
            //});

            //services.AddGlobalExceptionFilter();
            //services.AddGlobalModelStateFilter();
            //services.AddJwtBearerAuthentication(options =>
            //{
            //    options.SecurityKey = "8A94FD8A94F122B0";
            //    options.ValidIssuer = "Issuer";
            //    options.ValidAudience = "Audience";
            //});
            services.AddJwtCookieAuthentication(options =>
            {
                options.SecurityKey = "8A94FDA4354414A0320A72292571DF8BDF3B215B44EC523C5862628A4F77C77E29131382D8937A81A1A0E737406A06C4DE24AB0539375EEE779783F5D4E7FE67";
                options.ValidIssuer = "test1";
                options.ValidAudience = "test2";
                options.Cookie.Name = "access_token";
                options.LoginPath = "/login";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(1);
                options.SlidingExpiration = true;

            });

            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            //{
            //    options.ExpireTimeSpan = TimeSpan.FromMinutes(2);
            //    options.SlidingExpiration = true;
            //});


            services.AddCors(option => option.AddPolicy("Cors", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
            services.AddAutoGenerateHtmlControl(builder =>
            {
                builder.UploaderOptions.FormData = new { summary = "this is summary" };
                builder.UploaderOptions.FileSingleSizeLimit = 1024 * 1024 * 20;
                builder.UploaderOptions.AutoUpload = true;
                builder.UploaderOptions.FileBaseUrl = "/upload";
                builder.UploaderOptions.Accept.Extensions = "jpg,gif,png,bmp,jpeg,pdf,doc,docx,xls,xlsx,ppt,pptx,mp3";
                //builder.UploaderOptions.StoreProvider = StoreProvider.OSS;
                builder.RichEditorOptions.Options = new
                {
                    ckfinder = new
                    {
                        uploadUrl = "/api/upload?target=editor"
                    },
                    language = "zh-cn",
                    toolbar = new[] { "heading", "|", "bold", "italic", "link", "bulletedList", "numberedList", "blockQuote", "insertTable", "mediaEmbed", "imageUpload", "undo", "redo", "fontsize", "fontcolor", "highlight" }
                };
            });
            //services.AddChunkedUploadLocalStorage();
            services.AddUploadLocalStorage(builder =>
            {
                builder.RootDirectory = Path.Combine("wwwroot", "upload");
                builder.AddUploadCompletedHandler<EditorUploadCompletedHandler>();
                builder.AddAllowFileExtension(".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".mp3");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseUpload("/api/upload");
            app.UseCors("Cors");

            app.UseStaticFiles();
            app.UseAutoGenerateHtmlControl();
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
