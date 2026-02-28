using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace NetCore.Web.Extension
{
    public interface IJwtGenerator
    {
        JwtResult Generate(TimeSpan expire, Dictionary<string, string> claims = null, string issuer = null,
            string audience = null);
    }

    public class JwtGenerator : IJwtGenerator
    {
        private JwtOptions Options { get; }

        public JwtGenerator(JwtOptions options)
        {
            Options = options;
        }

        public static string GenerateSecurityKey()
        {
            return new StringBuilder().AppendFormat("{0:N}{1:N}{2:N}{3:N}", Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid()).ToString().ToUpper();
        }

        public JwtResult Generate(TimeSpan expire, Dictionary<string, string> claims = null, string issuer = null,
            string audience = null)
        {
            var jwtHeader = new JwtHeader(new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Options.SecurityKey)), Options.SecurityAlgorithm));
            var now = DateTime.UtcNow;
            var jwtPayload = new JwtPayload(
                issuer,
                audience,
                claims?.Select(p => new Claim(p.Key, p.Value)),
                now,
                now.Add(expire),
                now);
            var jwt = new JwtSecurityToken(jwtHeader, jwtPayload);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return new JwtResult
            {
                Token = encodedJwt,
                ExpiresIn = (int)expire.TotalSeconds
            };
        }
    }
}