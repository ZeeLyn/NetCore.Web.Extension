using System.ComponentModel.DataAnnotations;

namespace NetCore.Web.Extension
{
    public class UserNameAttribute : RegularExpressionAttribute
    {
        public UserNameAttribute() : base(@"^[a-zA-Z]\w{5,17}$")
        {
            ErrorMessage = "用户名只能由字母，数字和下划线组成，并且必须以字母开头,长度在6-18";
        }
    }
}
