using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;

namespace NetCore.Web.Extension
{
    public class JwtCookieDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private TokenValidationParameters ValidationParameters { get; }

        private JwtCookieOptions Options { get; }

        private JwtSecurityTokenHandler Handler { get; }

        public JwtCookieDataFormat(TokenValidationParameters validationParameters, JwtCookieOptions options)
        {
            ValidationParameters = validationParameters;
            Options = options;
            Handler = new JwtSecurityTokenHandler();
        }

        public AuthenticationTicket Unprotect(string protectedText)
          => Unprotect(protectedText, null);

        public AuthenticationTicket Unprotect(string protectedText, string purpose)
        {
            var handler = new JwtSecurityTokenHandler();
            try
            {
                var principal = handler.ValidateToken(protectedText, ValidationParameters, out var validToken);
                if (!(validToken is JwtSecurityToken validJwt))
                {
                    throw new ArgumentException("Invalid JWT");
                }
                if (!validJwt.Header.Alg.Equals(Options.SecurityAlgorithm, StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Algorithm must be '{Options.SecurityAlgorithm}'");
                }
                // Validation passed. Return a valid AuthenticationTicket:
                return new AuthenticationTicket(principal, new AuthenticationProperties(), CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // This ISecureDataFormat implementation is decode-only
        public string Protect(AuthenticationTicket data)
        {
            return Protect(data, null);
        }

        public string Protect(AuthenticationTicket data, string purpose)
        {
            data.Properties.Items.TryGetValue("issuer", out var issuer);
            data.Properties.Items.TryGetValue("audience", out var audience);
            data.Properties.Parameters.TryGetValue("expire", out var expire);
            return Handler.WriteToken(new JwtSecurityToken(issuer, audience, data.Principal.Claims, DateTime.UtcNow, expire == null ? default : DateTime.UtcNow.AddSeconds(((TimeSpan)expire).TotalSeconds), new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Options.SecurityKey)), Options.SecurityAlgorithm)));
        }
    }
}
