using System.ComponentModel.DataAnnotations;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    public class EmailAttribute : RegularExpressionAttribute
    {
        public EmailAttribute() : base(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$")
        {
            ErrorMessage = "邮箱格式不正确";
        }
    }
}
