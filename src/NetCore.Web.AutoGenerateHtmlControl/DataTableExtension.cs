using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetCore.Web.AutoGenerateHtmlControl.Attributes;

namespace NetCore.Web.AutoGenerateHtmlControl
{
    public static class DataTableExtension
    {
        /// <summary>
        /// 生成table
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="html"></param>
        /// <param name="dataList"></param>
        /// <returns></returns>
        public static async Task<IHtmlContent> DataTableAsync<TModel>(this IHtmlHelper html, IEnumerable<TModel> dataList)
        {
            var type = typeof(TModel);
            var meta = DataTableHelper.GetTableMeta(type);
            var builder = new StringBuilder();
            builder.Append("<table class=\"table table-hover\">");
            builder.Append("<tr>");
            foreach (var column in meta)
            {
                builder.AppendFormat("<th scope=\"col\">{0}</th>", column.DisplayName);
            }
            builder.Append("</tr>");

            foreach (var item in dataList)
            {
                builder.Append("<tr>");
                foreach (var column in meta)
                {
                    builder.AppendFormat("<td>{0}</td>", column.GetValue(type, item));
                }
                builder.Append("</tr>");
            }

            builder.Append("</table>");
            return html.Raw(builder.ToString());
        }
    }

    internal class DataTableHelper
    {
        private static readonly Regex Regex = new Regex(@"{\w+?}", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromSeconds(1));

        private static readonly ConcurrentDictionary<(Type, string), HashSet<string>> Placeholder = new ConcurrentDictionary<(Type, string), HashSet<string>>();

        private static readonly ConcurrentDictionary<Type, List<DataTableMeta>> Meta = new ConcurrentDictionary<Type, List<DataTableMeta>>();

        internal static List<DataTableMeta> GetTableMeta(Type type)
        {
            return Meta.GetOrAdd(type, t =>
            {
                var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                return props.Select(p => new DataTableMeta(p, type)).ToList();
            });
        }
        internal static HashSet<string> GetPlaceholder(Type type, string name, DataTableAttribute attribute)
        {
            return Placeholder.GetOrAdd((type, name), t =>
            {
                if (string.IsNullOrWhiteSpace(attribute.Format)) return new HashSet<string>();
                var hashSet = new HashSet<string>();
                foreach (Match mc in Regex.Matches(attribute.Format))
                {
                    if (!mc.Success)
                        continue;
                    var v = mc.Value.TrimStart('{').TrimEnd('}');
                    if (!hashSet.Contains(v))
                        hashSet.Add(v);
                }
                return hashSet;
            });

        }
    }

    public class DataTableMeta
    {
        public DataTableMeta(PropertyInfo property, Type type)
        {
            PropertyInfo = property;
            Attribute = property.GetCustomAttribute<DataTableAttribute>();
            var display = property.GetCustomAttribute<DisplayNameAttribute>();
            DisplayName = display == null ? property.Name : display.DisplayName;
            Name = property.Name;
            Placeholder = DataTableHelper.GetPlaceholder(type, property.Name, Attribute);
        }
        public string DisplayName { get; set; }

        public string Name { get; set; }

        public PropertyInfo PropertyInfo { get; set; }

        public DataTableAttribute Attribute { get; set; }

        public HashSet<string> Placeholder { get; set; }


        public string GetValue(Type type, object obj)
        {
            if (string.IsNullOrWhiteSpace(Attribute.Format))
                return PropertyInfo.GetValue(obj).ToString();
            var displayText = Attribute.Format;
            var metas = DataTableHelper.GetTableMeta(type);
            foreach (var ph in Placeholder)
            {
                displayText = displayText.Replace("{" + ph + "}", metas.First(p => p.Name == ph).PropertyInfo.GetValue(obj).ToString());
            }

            return displayText;
        }
    }
}
