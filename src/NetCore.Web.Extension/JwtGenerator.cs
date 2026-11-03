using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public static string GenerateSecurityKey()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        public JwtResult Generate(TimeSpan expire, Dictionary<string, string> claims = null, string issuer = null,
            string audience = null)
        {
            return GenerateToken(Options, expire, claims?.Select(p => new Claim(p.Key, p.Value)), issuer, audience);
        }

        internal static JwtResult GenerateToken(JwtOptions options, TimeSpan expire, IEnumerable<Claim> claims,
            string issuer = null, string audience = null)
        {
            if (expire <= TimeSpan.Zero || expire.TotalSeconds > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(expire));
            var now = DateTime.UtcNow;
            var payload = new JwtPayload(issuer ?? options.ValidIssuer, audience ?? options.ValidAudience,
                claims, now, now.Add(expire), now);
            var header = new JwtHeader(new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecurityKey)), options.SecurityAlgorithm),
                JwtSecurityTokenHandler.DefaultOutboundAlgorithmMap);
            return new JwtResult
            {
                Token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(header, payload)),
                ExpiresIn = (int)expire.TotalSeconds
            };
        }

        internal static TokenValidationParameters CreateValidationParameters(JwtOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.SecurityKey))
                throw new ArgumentNullException(nameof(options.SecurityKey));
            var algorithm = JwtSecurityTokenHandler.DefaultOutboundAlgorithmMap.TryGetValue(options.SecurityAlgorithm,
                out var mapped) ? mapped : options.SecurityAlgorithm;
            var minimumBytes = algorithm switch
            {
                SecurityAlgorithms.HmacSha256 => 32,
                SecurityAlgorithms.HmacSha384 => 48,
                SecurityAlgorithms.HmacSha512 => 64,
                _ => throw new ArgumentException("Only HS256, HS384 and HS512 are supported.", nameof(options.SecurityAlgorithm))
            };
            var key = Encoding.UTF8.GetBytes(options.SecurityKey);
            if (key.Length < minimumBytes)
                throw new ArgumentOutOfRangeException(nameof(options.SecurityKey),
                    $"{algorithm} requires a SecurityKey of at least {minimumBytes} UTF-8 bytes.");
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidAlgorithms = new[] { algorithm, options.SecurityAlgorithm },
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuer = !string.IsNullOrWhiteSpace(options.ValidIssuer) || options.ValidIssuers?.Any() == true,
                ValidIssuer = options.ValidIssuer,
                ValidIssuers = options.ValidIssuers,
                ValidateAudience = !string.IsNullOrWhiteSpace(options.ValidAudience) || options.ValidAudiences?.Any() == true,
                ValidAudience = options.ValidAudience,
                ValidAudiences = options.ValidAudiences
            };
        }
    }
}
