using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace NetCore.Web.Extension
{
    public static class LoginExtension
    {
        public static async Task SignInAsync(this ControllerBase controller, Dictionary<string, string> claims, TimeSpan expire, string issuer = null, string audience = null)
        {
            var claimsIdentity = new ClaimsIdentity(claims.Select(p => new Claim(p.Key, p.Value)), CookieAuthenticationDefaults.AuthenticationScheme);
            var prop = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.Add(expire),

                IsPersistent = true
            };
            prop.Items.Add("issuer", issuer);
            prop.Items.Add("audience", audience);
            prop.Parameters.Add("expire", expire);

            await controller.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), prop);
        }

        public static async Task SignOutAsync(this ControllerBase controller)
        {
            await controller.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
