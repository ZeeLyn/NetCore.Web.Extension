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
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.FullName.StartsWith("System") && !p.FullName.StartsWith("Microsoft"));
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
        /// <summary>
        /// 富文本编辑器预置脚本（默认CKEditor5）
        /// 占位符  {Name}:当前字段的名字   {Value}:当前字段的值
        /// </summary>
        public string EditorPresetScript { get; set; } = "window.onload=function(){ClassicEditor.create(document.querySelector(\"#{Name}\")).catch(e=>{console.error(e)});}";

        /// <summary>
        /// 上传组件预置脚本（默认WebUploader）
        /// 占位符  {Name}:当前字段的名字   {Value}:当前字段的值    {ServerUrl}:上传url
        /// </summary>
        public string UploaderPresetScript { get; set; }

        public string UploadServerUrl { get; set; } = "/api/upload";
    }
}
