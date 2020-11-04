using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using NetCore.Web.AutoGenerateHtmlControl.Attributes;

namespace NetCore.Web.AutoGenerateHtmlControl
{
    public class FormProperties
    {
        public FormProperties(Type type)
        {
            var prop = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead).ToList();
            foreach (var p in prop)
            {
                var controlAttrs = p.GetCustomAttributes<FormControlsAttribute>().ToList();
                if (!controlAttrs.Any())
                    continue;
                var display = p.GetCustomAttribute<DisplayNameAttribute>();
                var displayName = display == null ? p.Name : display.DisplayName;

                var order = p.GetCustomAttribute<DisplayOrderAttribute>();
                var orderNumber = order?.OrderNumber ?? 0;
                Properties.Add(new FormPropertyInfo
                {
                    PropertyInfo = p,
                    Controls = controlAttrs,
                    DataListColumnConvert = p.GetCustomAttribute<DataListColumnConvertAttribute>(),
                    DisplayName = displayName,
                    OrderNumber = orderNumber,
                    Hide = p.GetCustomAttribute<HideAttribute>() != null
                });
            }

            Properties = Properties.OrderBy(p => p.OrderNumber).ToList();
        }

        public List<FormPropertyInfo> Properties { get; set; } = new List<FormPropertyInfo>();
    }

    public class FormPropertyInfo
    {
        public PropertyInfo PropertyInfo { get; set; }

        public List<FormControlsAttribute> Controls { get; set; }

        public DataListColumnConvertAttribute DataListColumnConvert { get; set; }

        public string DisplayName { get; set; }

        public int OrderNumber { get; set; }

        public bool Hide { get; set; }
    }
}
