using System;
using System.Collections.Generic;
using System.Text;

namespace NetCore.Web.AutoGenerateHtmlControl
{
    public class RichEditorContext
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public IDictionary<string, object> HtmlAttributes { get; set; }
    }
}
