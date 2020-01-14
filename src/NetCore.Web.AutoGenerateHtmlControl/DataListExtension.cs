using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetCore.Web.AutoGenerateHtmlControl.Attributes;
using Newtonsoft.Json;

namespace NetCore.Web.AutoGenerateHtmlControl
{
    public static class DataListExtension
    {
        /// <summary>
        /// 生成table
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="html"></param>
        /// <param name="dataList">数据集合</param>
        /// <param name="operatingBuilder">在table最后一列添加html</param>
        /// <returns></returns>
        public static IHtmlContent TableView<TModel>(this IHtmlHelper html, IEnumerable<TModel> dataList, Func<TModel, IHtmlContent> operatingBuilder = default)
        {
            var type = typeof(TModel);
            var columnMeta = DataListHelper.GetDataColumnMeta(type);
            var tableMeta = DataListHelper.GetDataMeta(type);
            var tableContainer = new TagBuilder("div");
            tableContainer.AddCssClass("table-responsive-md");
            var table = new TagBuilder("table");
            table.MergeAttribute("class", "table table-hover");
            if (tableMeta.HtmlAttribute != null)
                table.MergeAttributes(tableMeta.HtmlAttribute, true);
            var header = new TagBuilder("tr");
            foreach (var column in columnMeta)
            {
                var th = new TagBuilder("th");
                th.MergeAttribute("scope", "col");
                if (column.ThHtmlAttribute != null)
                    th.MergeAttributes(column.ThHtmlAttribute, true);
                th.InnerHtml.AppendHtml(column.DisplayName);
                header.InnerHtml.AppendHtml(th);
            }
            if (operatingBuilder != null)
            {
                var action = new TagBuilder("th");
                header.InnerHtml.AppendHtml(action);
            }
            table.InnerHtml.AppendHtml(header);

            foreach (var item in dataList)
            {
                var tr = new TagBuilder("tr");
                foreach (var column in columnMeta)
                {
                    var td = new TagBuilder("td");
                    if (column.TdHtmlAttribute != null)
                        td.MergeAttributes(column.TdHtmlAttribute, true);
                    td.InnerHtml.AppendHtml(column.GetValue(type, item));
                    tr.InnerHtml.AppendHtml(td);

                }
                if (operatingBuilder != null)
                {
                    var td = new TagBuilder("td");
                    td.InnerHtml.AppendHtml(operatingBuilder(item));
                    tr.InnerHtml.AppendHtml(td);
                }
                table.InnerHtml.AppendHtml(tr);
            }

            tableContainer.InnerHtml.AppendHtml(table);
            return tableContainer;
        }
    }

    internal class DataListHelper
    {
        private static readonly Regex Regex = new Regex(@"{\w+?}", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromSeconds(1));

        private static readonly ConcurrentDictionary<Type, List<DataColumnMeta>> DataColumnMeta = new ConcurrentDictionary<Type, List<DataColumnMeta>>();

        private static readonly ConcurrentDictionary<Type, DataMeta> DataMeta = new ConcurrentDictionary<Type, DataMeta>();

        internal static List<DataColumnMeta> GetDataColumnMeta(Type type)
        {
            return DataColumnMeta.GetOrAdd(type, t =>
            {
                var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                return (from p in props
                        let display = p.GetCustomAttribute<DisplayNameAttribute>()
                        let attr = p.GetCustomAttribute<DataListColumnAttribute>()
                        select new DataColumnMeta
                        {
                            PropertyInfo = p,
                            Attribute = attr,
                            DisplayName = display == null ? p.Name : display.DisplayName,
                            Name = p.Name,
                            FormatPlaceholder = GetFormatPlaceholder(attr),
                            ValueMapPlaceholder = GetValueMapPlaceholder(attr),
                            ValueMap = ParsingKeyValue(attr.ValueMap),
                            TdHtmlAttribute = ParsingKeyValue(attr.ContentHtmlAttribute),
                            ThHtmlAttribute = ParsingKeyValue(attr.HeaderHtmlAttribute)
                        }).ToList();
            });
        }

        internal static DataMeta GetDataMeta(Type type)
        {
            return DataMeta.GetOrAdd(type, t =>
            {
                var attr = t.GetCustomAttribute<DataListAttribute>();
                if (attr == null)
                    return new DataMeta();
                return new DataMeta
                {
                    HtmlAttribute = ParsingKeyValue(attr.HtmlAttribute)
                };
            });
        }

        internal static HashSet<string> GetFormatPlaceholder(DataListColumnAttribute attribute)
        {
            if (string.IsNullOrWhiteSpace(attribute.Format)) return new HashSet<string>();
            var hashSet = new HashSet<string>();
            foreach (Match mc in Regex.Matches(attribute.Format))
            {
                if (!mc.Success)
                    continue;
                var v = mc.Value.TrimStart('{').TrimEnd('}').Trim();
                if (!hashSet.Contains(v))
                    hashSet.Add(v);
            }
            return hashSet;
        }

        internal static HashSet<string> GetValueMapPlaceholder(DataListColumnAttribute attribute)
        {
            if (string.IsNullOrWhiteSpace(attribute.ValueMap)) return new HashSet<string>();
            var hashSet = new HashSet<string>();
            foreach (Match mc in Regex.Matches(attribute.ValueMap))
            {
                if (!mc.Success)
                    continue;
                var v = mc.Value.TrimStart('{').TrimEnd('}').Trim();
                if (!hashSet.Contains(v))
                    hashSet.Add(v);
            }
            return hashSet;
        }


        internal static Dictionary<string, string> ParsingKeyValue(string map)
        {
            if (string.IsNullOrWhiteSpace(map))
                return null;
            var dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var maps = JsonConvert.DeserializeObject<Dictionary<string, string>>(map);
                if (maps == null) return null;
                foreach (var m in maps)
                    dic[m.Key] = m.Value;
            }
            catch
            {
                throw new FormatException(map);
            }
            return dic;
        }
    }

    public class DataColumnMeta
    {
        public string DisplayName { get; internal set; }

        public string Name { get; internal set; }

        public PropertyInfo PropertyInfo { get; internal set; }

        public DataListColumnAttribute Attribute { get; internal set; }

        public HashSet<string> FormatPlaceholder { get; internal set; }

        public HashSet<string> ValueMapPlaceholder { get; internal set; }

        public Dictionary<string, string> ValueMap { get; internal set; }

        public Dictionary<string, string> ThHtmlAttribute { get; internal set; }

        public Dictionary<string, string> TdHtmlAttribute { get; internal set; }


        public string GetValue(Type type, object obj)
        {
            if (!string.IsNullOrWhiteSpace(Attribute.Format))
            {
                var displayText = Attribute.Format;
                var metas = DataListHelper.GetDataColumnMeta(type);

                return FormatPlaceholder.Aggregate(displayText, (current, ph) => current.Replace("{" + ph + "}", metas.First(p => p.Name == ph).PropertyInfo.GetValue(obj).ToString()));
            }

            var value = PropertyInfo.GetValue(obj).ToString();
            if (ValueMap != null && ValueMap.ContainsKey(value))
            {
                var displayText = ValueMap[value].ToString();
                var metas = DataListHelper.GetDataColumnMeta(type);

                return ValueMapPlaceholder.Aggregate(displayText, (current, ph) => current.Replace("{" + ph + "}", metas.First(p => p.Name == ph).PropertyInfo.GetValue(obj).ToString()));
            }
            return value;
        }
    }
    public class DataMeta
    {
        public Dictionary<string, string> HtmlAttribute { get; internal set; }
    }
}
