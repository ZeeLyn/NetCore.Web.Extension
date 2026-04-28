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

        /// <summary>
        /// The GetPrincipalFromToken
        /// </summary>
        /// <param name="token">The token<see cref="string"/></param>
        /// <returns>The <see cref="ClaimsPrincipal"/></returns>
        ClaimsPrincipal GetPrincipalFromToken(string token, out SecurityToken securityToken);
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

        /// <summary>
        /// The GetPrincipalFromToken
        /// </summary>
        /// <param name="token">The token<see cref="string"/></param>
        /// <param name="issuer">The issuer<see cref="string"/></param>
        /// <param name="audience">The audience<see cref="string"/></param>
        /// <returns>The <see cref="ClaimsPrincipal"/></returns>
        public ClaimsPrincipal GetPrincipalFromToken(string token, out SecurityToken securityToken)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Options.SecurityKey)),

                //ValidateLifetime = false // 允许过期Token解析
            };
            if (Options.ValidIssuers is not null)
                tokenValidationParameters.ValidIssuers=Options.ValidIssuers;
            if (Options.ValidAudiences is not null)
                tokenValidationParameters.ValidAudiences=Options.ValidAudiences;

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken _securityToken);
            securityToken=_securityToken;
            return principal;
        }
    }
}
