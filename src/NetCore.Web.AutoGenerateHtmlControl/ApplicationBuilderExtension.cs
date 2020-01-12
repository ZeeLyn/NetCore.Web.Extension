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
                RequestPath = "/auto-generate-html-control/resources",
                FileProvider = new EmbeddedFileProvider(typeof(UploaderContext).GetTypeInfo().Assembly)
            });
            return app;
        }
    }
}
