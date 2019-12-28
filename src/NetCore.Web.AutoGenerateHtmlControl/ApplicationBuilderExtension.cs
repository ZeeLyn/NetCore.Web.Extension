using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;

namespace NetCore.Web.AutoGenerateHtmlControl
{
    public static class ApplicationBuilderExtension
    {
        public static IApplicationBuilder UseAutoGenerateHtmlControl(this IApplicationBuilder app)
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = "/resources/auto_generate_html_control",
                FileProvider = new EmbeddedFileProvider(typeof(UploaderContext).GetTypeInfo().Assembly)
            });
            return app;
        }
    }
}
