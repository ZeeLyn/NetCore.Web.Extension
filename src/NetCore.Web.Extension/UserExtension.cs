using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace NetCore.Web.Extension
{
    public static class UserExtension
    {
        public static string GetClaim(this ControllerBase controller, string type)
        {
            return controller.User.Claims.FirstOrDefault(p => p.Type == type)?.Value;
        }
    }
}
