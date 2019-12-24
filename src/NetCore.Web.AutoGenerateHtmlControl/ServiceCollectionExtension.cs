using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace NetCore.Web.AutoGenerateHtmlControl
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddAutoGenerateHtmlControl(this IServiceCollection services, Action<AutoGenerateFormBuilder> builder = null)
        {
            var options = new AutoGenerateFormBuilder(services);
            builder?.Invoke(options);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.FullName.StartsWith("System") && !p.FullName.StartsWith("Microsoft"));
            var dataSourceType = typeof(ISelectDataSource);
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetExportedTypes())
                {
                    if (type != dataSourceType && dataSourceType.IsAssignableFrom(type))
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
        protected IServiceCollection Services { get; }
        protected internal AutoGenerateFormBuilder(IServiceCollection services)
        {
            Services = services;
        }
        public IServiceCollection AddDataSource<TSelectDataSource>() where TSelectDataSource : ISelectDataSource
        {
            return Services.AddScoped(typeof(TSelectDataSource));
        }
    }
}
