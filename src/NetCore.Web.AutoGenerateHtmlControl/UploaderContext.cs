using System;
using System.Collections.Generic;
using System.Text;

namespace NetCore.Web.AutoGenerateHtmlControl
{
    public class UploaderContext
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public string ServerUrl { get; set; }

        public IDictionary<string, object> HtmlAttributes { get; set; }
    }
}
