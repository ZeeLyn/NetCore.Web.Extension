using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;

namespace NetCore.Web.Extension
{
    public class JwtCookieDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private const string PropertiesClaim = "urn:netcore.web.extension:ticket-properties";
        private TokenValidationParameters ValidationParameters { get; }
        private JwtCookieOptions Options { get; }
        private JwtSecurityTokenHandler Handler { get; } = new JwtSecurityTokenHandler();

        public JwtCookieDataFormat(TokenValidationParameters validationParameters, JwtCookieOptions options)
        {
            if (validationParameters == null)
                throw new ArgumentNullException(nameof(validationParameters));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            ValidationParameters = validationParameters.Clone();
            ValidationParameters.ValidAlgorithms = JwtGenerator.CreateValidationParameters(options).ValidAlgorithms;
        }

        public AuthenticationTicket Unprotect(string protectedText) => Unprotect(protectedText, null);

        public AuthenticationTicket Unprotect(string protectedText, string purpose)
        {
            if (string.IsNullOrWhiteSpace(protectedText))
                return null;
            try
            {
                var principal = Handler.ValidateToken(protectedText, ValidationParameters, out var token);
                if (!(token is JwtSecurityToken jwt))
                    return null;
                var storedProperties = principal.FindFirst(PropertiesClaim);
                var properties = storedProperties == null ? new AuthenticationProperties() :
                    PropertiesSerializer.Default.Deserialize(Base64UrlEncoder.DecodeBytes(storedProperties.Value));
                if (properties == null)
                    return null;
                foreach (var identity in principal.Identities)
                    foreach (var claim in identity.FindAll(PropertiesClaim).ToArray())
                        identity.RemoveClaim(claim);
                properties.IssuedUtc = new DateTimeOffset(jwt.ValidFrom);
                properties.ExpiresUtc = new DateTimeOffset(jwt.ValidTo);
                properties.Items["issuer"] = jwt.Issuer;
                properties.Items["audience"] = jwt.Audiences.FirstOrDefault();
                return new AuthenticationTicket(principal, properties, CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (SecurityTokenException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (FormatException)
            {
                return null;
            }
            catch (System.IO.IOException)
            {
                return null;
            }
        }

        public string Protect(AuthenticationTicket data) => Protect(data, null);

        public string Protect(AuthenticationTicket data, string purpose)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            data.Properties.Items.TryGetValue("issuer", out var issuer);
            data.Properties.Items.TryGetValue("audience", out var audience);
            data.Properties.Parameters.TryGetValue("expire", out var expire);
            var issued = data.Properties.IssuedUtc ?? DateTimeOffset.UtcNow;
            var expires = data.Properties.ExpiresUtc ?? issued.Add(expire is TimeSpan duration ? duration : Options.ExpireTimeSpan);
            // 不把旧 JWT 的时间及签发信息带入新票据，否则续期后仍可能沿用旧 exp。
            var claims = data.Principal.Claims.Where(claim => claim.Type != "exp" && claim.Type != "nbf" &&
                claim.Type != "iat" && claim.Type != "iss" && claim.Type != PropertiesClaim)
                .Append(new Claim(PropertiesClaim, Base64UrlEncoder.Encode(PropertiesSerializer.Default.Serialize(data.Properties))))
                .ToArray();
            var payload = new JwtPayload(issuer ?? Options.ValidIssuer,
                claims.Any(claim => claim.Type == "aud") ? null : audience ?? Options.ValidAudience,
                claims, issued.UtcDateTime, expires.UtcDateTime, issued.UtcDateTime);
            var header = new JwtHeader(new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Options.SecurityKey)), Options.SecurityAlgorithm),
                JwtSecurityTokenHandler.DefaultOutboundAlgorithmMap);
            return Handler.WriteToken(new JwtSecurityToken(header, payload));
        }
    }
}
