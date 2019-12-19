using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;

namespace NetCore.Web.Extension
{
    public class JwtCookieDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly string _algorithm;
        private readonly TokenValidationParameters _validationParameters;
        private JwtCookieOptions Options { get; set; }

        public JwtCookieDataFormat(string algorithm, TokenValidationParameters validationParameters, JwtCookieOptions options)
        {
            _algorithm = algorithm;
            _validationParameters = validationParameters;
            Options = options;
        }

        public AuthenticationTicket Unprotect(string protectedText)
          => Unprotect(protectedText, null);

        public AuthenticationTicket Unprotect(string protectedText, string purpose)
        {
            var handler = new JwtSecurityTokenHandler();
            ClaimsPrincipal principal;
            try
            {
                principal = handler.ValidateToken(protectedText, _validationParameters, out var validToken);
                var validJwt = validToken as JwtSecurityToken;
                if (validJwt == null)
                {
                    throw new ArgumentException("Invalid JWT");
                }
                if (!validJwt.Header.Alg.Equals(_algorithm, StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Algorithm must be '{_algorithm}'");
                }
            }
            catch (SecurityTokenValidationException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }

            // Validation passed. Return a valid AuthenticationTicket:
            return new AuthenticationTicket(principal, new AuthenticationProperties(), CookieAuthenticationDefaults.AuthenticationScheme);
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
            return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(issuer, audience, data.Principal.Claims, DateTime.UtcNow, expire == null ? default : DateTime.UtcNow.AddSeconds(((TimeSpan)expire).TotalSeconds), new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Options.SecurityKey)), SecurityAlgorithms.HmacSha512Signature)));
        }
    }
}
