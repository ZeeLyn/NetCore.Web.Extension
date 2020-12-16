using System.ComponentModel.DataAnnotations;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    public class MobileAttribute : RegularExpressionAttribute
    {
        public MobileAttribute() : base(@"^[1]+[3,4,5,6,7,8,9]+\d{9}")
        {
            ErrorMessage = "手机号格式不正确";
        }
    }
}
