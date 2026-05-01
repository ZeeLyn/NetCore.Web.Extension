namespace NetCore.Web.Extension
{
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;

    /// <summary>
    /// Defines the <see cref="IJwtGenerator" />
    /// </summary>
    public interface IJwtGenerator
    {
        /// <summary>
        /// The Generate
        /// </summary>
        /// <param name="expire">The expire<see cref="TimeSpan"/></param>
        /// <param name="claims">The claims<see cref="Dictionary{string, string}"/></param>
        /// <param name="issuer">The issuer<see cref="string"/></param>
        /// <param name="audience">The audience<see cref="string"/></param>
        /// <returns>The <see cref="JwtResult"/></returns>
        JwtResult Generate(TimeSpan expire, Dictionary<string, string> claims = null, string issuer = null,
            string audience = null);

    }

    /// <summary>
    /// Defines the <see cref="JwtGenerator" />
    /// </summary>
    public class JwtGenerator : IJwtGenerator
    {
        /// <summary>
        /// Gets the Options
        /// </summary>
        private JwtOptions Options { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtGenerator"/> class.
        /// </summary>
        /// <param name="options">The options<see cref="JwtOptions"/></param>
        public JwtGenerator(JwtOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// The GenerateSecurityKey
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public static string GenerateSecurityKey()
        {
            return new StringBuilder().AppendFormat("{0:N}{1:N}{2:N}{3:N}", Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid()).ToString().ToUpper();
        }

        /// <summary>
        /// The Generate
        /// </summary>
        /// <param name="expire">The expire<see cref="TimeSpan"/></param>
        /// <param name="claims">The claims<see cref="Dictionary{string, string}"/></param>
        /// <param name="issuer">The issuer<see cref="string"/></param>
        /// <param name="audience">The audience<see cref="string"/></param>
        /// <returns>The <see cref="JwtResult"/></returns>
        public JwtResult Generate(TimeSpan expire, Dictionary<string, string> claims = null, string issuer = null,
            string audience = null)
        {
            var jwtHeader = new JwtHeader(new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Options.SecurityKey)), Options.SecurityAlgorithm));
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
