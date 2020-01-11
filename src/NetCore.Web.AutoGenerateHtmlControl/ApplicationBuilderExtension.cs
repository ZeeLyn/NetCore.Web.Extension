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
                RequestPath = "/auto_generate_html_control/resources",
                FileProvider = new EmbeddedFileProvider(typeof(UploaderContext).GetTypeInfo().Assembly)
            });
            return app;
        }
    }
}
