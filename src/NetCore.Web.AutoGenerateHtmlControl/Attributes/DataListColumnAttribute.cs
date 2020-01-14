using System;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    public class DataListColumnAttribute : Attribute
    {
        /// <summary>
        /// 格式化显示，跟ValueMap只能二选一，可使用占位符代替对应的字段值，例如：{Title}
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// 值映射，KV键值对，跟Format只能二选一
        /// </summary>
        public string ValueMap { get; set; }

        /// <summary>
        /// 内容显示区的Html属性
        /// </summary>
        public string ContentHtmlAttribute { get; set; }

        /// <summary>
        /// 标题栏的Html属性
        /// </summary>
        public string HeaderHtmlAttribute { get; set; }

        public CardContentContainer CardContentContainer { get; set; } = CardContentContainer.Body;
    }
}
