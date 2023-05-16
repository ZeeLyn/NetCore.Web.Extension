using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
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
        public static IHtmlContent TableView<TModel>(this IHtmlHelper html, IEnumerable<TModel> dataList,
            Func<TModel, IHtmlContent> operatingBuilder = default)
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
            foreach (var column in columnMeta.FindAll(p => p.Attribute != null))
            {
                var th = new TagBuilder("th");
                th.MergeAttribute("scope", "col");
                if (column.HeaderHtmlAttribute != null)
                    th.MergeAttributes(column.HeaderHtmlAttribute, true);
                th.InnerHtml.AppendHtml(column.DisplayName);
                header.InnerHtml.AppendHtml(th);
            }

            if (operatingBuilder != null)
            {
                var action = new TagBuilder("th");
                header.InnerHtml.AppendHtml(action);
            }

            table.InnerHtml.AppendHtml(header);
            if (dataList != null)
            {
                foreach (var item in dataList)
                {
                    var tr = new TagBuilder("tr");
                    foreach (var column in columnMeta.FindAll(p => p.Attribute != null))
                    {
                        var td = new TagBuilder("td");
                        if (column.ContentHtmlAttribute != null)
                            td.MergeAttributes(column.ContentHtmlAttribute, true);
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
            }

            tableContainer.InnerHtml.AppendHtml(table);
            return tableContainer;
        }

        public static IHtmlContent CardView<TModel>(this IHtmlHelper html, IEnumerable<TModel> dataList,
            Func<TModel, IHtmlContent> operatingBuilder = default)
        {
            var type = typeof(TModel);
            var columnMeta = DataListHelper.GetDataColumnMeta(type);
            var groupMeta = DataListHelper.GetDataMeta(type);
            var group = new TagBuilder("div");
            group.AddCssClass("card-deck");
            if (groupMeta.HtmlAttribute != null)
                group.MergeAttributes(groupMeta.HtmlAttribute, true);

            if (dataList != null)
            {
                foreach (var item in dataList)
                {
                    var card = new TagBuilder("div");
                    card.AddCssClass("card");
                    TagBuilder header = null;
                    TagBuilder body = null;
                    TagBuilder listGroup = null;
                    TagBuilder footer = null;
                    IHtmlContentBuilder root = null;
                    foreach (var column in columnMeta.FindAll(p => p.Attribute != null))
                    {
                        switch (column.CardContentContainer)
                        {
                            case CardContentContainer.Root:
                                root ??= new HtmlContentBuilder();
                                root.AppendHtml(column.GetValue(type, item));
                                break;
                            case CardContentContainer.Header:
                                header ??= new TagBuilder("div");
                                var headerItem = new TagBuilder("div");
                                headerItem.AddCssClass("card-text");
                                if (column.ContentHtmlAttribute != null)
                                    headerItem.MergeAttributes(column.ContentHtmlAttribute, true);
                                headerItem.InnerHtml.AppendHtml(column.GetValue(type, item));
                                header.InnerHtml.AppendHtml(headerItem);
                                break;
                            case CardContentContainer.Body:
                                body ??= new TagBuilder("div");
                                var bodyItem = new TagBuilder("div");
                                bodyItem.AddCssClass("card-text");
                                if (column.ContentHtmlAttribute != null)
                                    bodyItem.MergeAttributes(column.ContentHtmlAttribute, true);
                                bodyItem.InnerHtml.AppendHtml(column.GetValue(type, item));
                                body.InnerHtml.AppendHtml(bodyItem);
                                break;
                            case CardContentContainer.ListGroup:
                                listGroup ??= new TagBuilder("ul");
                                var listItem = new TagBuilder("li");
                                listItem.AddCssClass("list-group-item");
                                if (column.ContentHtmlAttribute != null)
                                    listItem.MergeAttributes(column.ContentHtmlAttribute, true);
                                listItem.InnerHtml.AppendHtml(column.GetValue(type, item));
                                listGroup.InnerHtml.AppendHtml(listItem);
                                break;
                            case CardContentContainer.Footer:
                                footer ??= new TagBuilder("div");
                                var footerItem = new TagBuilder("div");
                                footerItem.AddCssClass("text-muted");
                                if (column.ContentHtmlAttribute != null)
                                    footerItem.MergeAttributes(column.ContentHtmlAttribute, true);
                                footerItem.InnerHtml.AppendHtml(column.GetValue(type, item));
                                footer.InnerHtml.AppendHtml(footerItem);
                                break;
                        }
                    }

                    if (header != null)
                    {
                        header.AddCssClass("card-header");
                        card.InnerHtml.AppendHtml(header);
                    }

                    if (root != null)
                    {
                        card.InnerHtml.AppendHtml(root);
                    }

                    if (body != null)
                    {
                        body.AddCssClass("card-body");
                        card.InnerHtml.AppendHtml(body);
                    }

                    if (listGroup != null)
                    {
                        listGroup.AddCssClass("list-group list-group-flush");
                        card.InnerHtml.AppendHtml(listGroup);
                    }

                    if (operatingBuilder != null)
                    {
                        card.InnerHtml.AppendHtml(operatingBuilder(item));
                    }

                    if (footer != null)
                    {
                        footer.AddCssClass("card-footer");
                        card.InnerHtml.AppendHtml(footer);
                    }

                    group.InnerHtml.AppendHtml(card);
                }
            }

            return group;
        }
    }

    internal class DataListHelper
    {
        private static readonly Regex Regex = new Regex(@"{\w+?}", RegexOptions.IgnoreCase | RegexOptions.Multiline,
            TimeSpan.FromSeconds(1));

        private static readonly ConcurrentDictionary<Type, List<DataColumnMeta>> DataColumnMeta =
            new ConcurrentDictionary<Type, List<DataColumnMeta>>();

        private static readonly ConcurrentDictionary<Type, DataMeta> DataMeta =
            new ConcurrentDictionary<Type, DataMeta>();

        internal static List<DataColumnMeta> GetDataColumnMeta(Type type)
        {
            return DataColumnMeta.GetOrAdd(type, t =>
            {
                var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                return (from p in props
                    let display = p.GetCustomAttribute<DisplayNameAttribute>()
                    let attr = p.GetCustomAttribute<DataListColumnAttribute>()
                    let converter = p.GetCustomAttribute<DataListColumnConvertAttribute>()
                    let order = p.GetCustomAttribute<DisplayOrderAttribute>()
                    //where attr != null
                    select new DataColumnMeta
                    {
                        PropertyInfo = p,
                        Attribute = attr,
                        DataConverter = converter,
                        DisplayName = display == null ? p.Name : display.DisplayName,
                        Name = p.Name,
                        FormatPlaceholder = attr != null ? GetFormatPlaceholder(attr) : null,
                        ValueMapPlaceholder = attr != null ? GetValueMapPlaceholder(attr) : null,
                        ValueMap = attr != null ? ParsingKeyValue(attr.ValueMap) : null,
                        ContentHtmlAttribute = attr != null ? ParsingKeyValue(attr.ContentHtmlAttribute) : null,
                        HeaderHtmlAttribute = attr != null ? ParsingKeyValue(attr.HeaderHtmlAttribute) : null,
                        CardContentContainer = attr?.CardContentContainer ?? default,
                        OrderNumber = order?.OrderNumber ?? 0
                    }).OrderBy(p => p.OrderNumber).ToList();
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

        public DataListColumnConvertAttribute DataConverter { get; internal set; }

        public HashSet<string> FormatPlaceholder { get; internal set; }

        public HashSet<string> ValueMapPlaceholder { get; internal set; }

        public Dictionary<string, string> ValueMap { get; internal set; }

        public Dictionary<string, string> HeaderHtmlAttribute { get; internal set; }

        public Dictionary<string, string> ContentHtmlAttribute { get; internal set; }

        public CardContentContainer CardContentContainer { get; internal set; } = CardContentContainer.Body;

        public int OrderNumber { get; set; }

        private object GetPlainValue(object obj)
        {
            var value = PropertyInfo.GetValue(obj);
            return DataConverter == null ? value : DataConverter.Convert(value);
        }

        public string GetValue(Type type, object obj)
        {
            if (!string.IsNullOrWhiteSpace(Attribute?.Format))
            {
                var displayText = Attribute.Format;
                var metas = DataListHelper.GetDataColumnMeta(type);

                return FormatPlaceholder.Aggregate(displayText,
                    (current, ph) => current.Replace("{" + ph + "}", HttpUtility.HtmlEncode(
                        metas.First(p => p.Name == ph).GetPlainValue(obj)?.ToString())));
            }

            var value = GetPlainValue(obj)?.ToString();
            if (string.IsNullOrWhiteSpace(value))
                return value;
            if (ValueMap != null && ValueMap.ContainsKey(value))
            {
                var displayText = ValueMap[value];
                var metas = DataListHelper.GetDataColumnMeta(type);

                return ValueMapPlaceholder.Aggregate(displayText,
                    (current, ph) => current.Replace("{" + ph + "}",
                        HttpUtility.HtmlEncode(metas.First(p => p.Name == ph).GetPlainValue(obj)?.ToString())));
            }

            return HttpUtility.HtmlEncode(value);
        }
    }

    public class DataMeta
    {
        public Dictionary<string, string> HtmlAttribute { get; internal set; }
    }

    public enum CardContentContainer
    {
        Root,
        Header,
        Body,
        ListGroup,
        Footer
    }
}