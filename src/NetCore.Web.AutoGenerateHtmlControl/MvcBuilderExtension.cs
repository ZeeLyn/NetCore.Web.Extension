using System;
using Microsoft.Extensions.DependencyInjection;

namespace NetCore.Web.AutoGenerateHtmlControl
{
    public static class MvcBuilderExtension
    {
        public static IMvcBuilder AddAutoGenerateForm(this IMvcBuilder mvcBuilder, Action<AutoGenerateFormBuilder> builder = null)
        {
            var options = new AutoGenerateFormBuilder(mvcBuilder.Services);
            builder?.Invoke(options);

            return mvcBuilder;
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
