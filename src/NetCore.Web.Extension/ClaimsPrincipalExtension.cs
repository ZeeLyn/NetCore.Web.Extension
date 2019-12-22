using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace NetCore.Web.Extension
{
    public static class ClaimsPrincipalExtension
    {
        public static string GetClaimValue(this ClaimsPrincipal principal, string type)
        {
            return principal.Claims.FirstOrDefault(p => p.Type == type)?.Value;
        }

        public static TValue GetClaimValue<TValue>(this ClaimsPrincipal principal, string type)
        {
            var value = principal.GetClaimValue(type);
            if (string.IsNullOrWhiteSpace(value))
                return default;
            return (TValue)Convert.ChangeType(value, typeof(TValue));
        }

        public static TValue GetClaimValueFromJson<TValue>(this ClaimsPrincipal principal, string type)
        {
            var value = principal.GetClaimValue(type);
            return string.IsNullOrWhiteSpace(value) ? default : JsonSerializer.Deserialize<TValue>(value);
        }
    }
}
