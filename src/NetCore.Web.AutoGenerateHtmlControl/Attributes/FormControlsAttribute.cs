using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public abstract class FormControlsAttribute : Attribute
    {
        public string HtmlAttributes { get; set; }

        protected readonly Dictionary<string, object> _attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, object> GetAttributes()
        {
            if (string.IsNullOrWhiteSpace(HtmlAttributes)) return _attributes;
            try
            {
                var attrs = JsonConvert.DeserializeObject<Dictionary<string, object>>(HtmlAttributes);
                if (attrs == null) return _attributes;
                foreach (var (key, o) in attrs)
                    _attributes[key] = o;
            }
            catch
            {
                throw new FormatException(HtmlAttributes);
            }
            return _attributes;
        }
        protected void AddAttribute(string name, object value)
        {
            _attributes[name.ToLower()] = value;
        }

        public HtmlControlType ControlType { get; protected internal set; }
    }


}
