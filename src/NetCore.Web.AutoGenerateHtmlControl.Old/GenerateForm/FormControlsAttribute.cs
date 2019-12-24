using System;
using System.Collections.Generic;

namespace NetCore.Web.AutoGenerateHtmlControl.GenerateForm
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public abstract class FormControlsAttribute : Attribute
    {
        public string Format { get; set; }

        public object HtmlAttributes { get; set; }


        protected readonly Dictionary<string, object> _attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "class", "form-control" } };

        public Dictionary<string, object> GetAttributes()
        {
            var attrs = HtmlAttributes.Object2Dictionary();
            if (attrs == null) return _attributes;
            foreach (var (key, o) in attrs)
                _attributes[key] = o;
            return _attributes;
        }
        protected void AddAttribute(string name, object value)
        {
            _attributes[name.ToLower()] = value;
        }


        public HtmlControl ControlType { get; protected internal set; }
    }

    public enum HtmlControl
    {
        Label,
        Hidden,
        TextBox,
        Password,
        TextArea,
        Button,
        DropDownList,
        ListBox
    }
}
