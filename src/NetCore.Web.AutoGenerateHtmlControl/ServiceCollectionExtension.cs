using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace NetCore.Web.AutoGenerateHtmlControl
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddAutoGenerateHtmlControl(this IServiceCollection services, Action<AutoGenerateFormBuilder> builder = null)
        {
            var options = new AutoGenerateFormBuilder();
            builder?.Invoke(options);
            services.AddSingleton(options);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic && !p.FullName.StartsWith("System") && !p.FullName.StartsWith("Microsoft"));
            var dataSourceType = typeof(IDataSource);
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetExportedTypes().Where(p => !p.IsAbstract && !p.IsInterface))
                {
                    if (dataSourceType.IsAssignableFrom(type))
                    {
                        services.AddScoped(type);
                    }
                }
            }

            return services;
        }
    }

    public class AutoGenerateFormBuilder
    {
        public string DefaultRichEditorPartialName { get; set; }

        public string DefaultUploaderPartialName { get; set; }

        public string UploadServerUrl { get; set; } = "/api/upload";
    }
}
