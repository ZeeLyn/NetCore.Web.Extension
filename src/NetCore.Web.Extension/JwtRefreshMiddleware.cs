using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore.Web.Extension
{
    public class JwtRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly JwtOptions _options;
        private readonly JwtSlidingExpirationOptions _slidingExpirationOptions;
        private readonly ILogger<JwtRefreshMiddleware> _logger;

        // 保留构造签名，续期使用原始 JWT claims，避免映射及重复角色在 Dictionary 转换时丢失。
        public JwtRefreshMiddleware(RequestDelegate next, IJwtGenerator jwtGenerator, JwtOptions jwtOptions,
            JwtSlidingExpirationOptions slidingExpirationOptions, ILogger<JwtRefreshMiddleware> logger)
        {
            _next = next;
            _jwtGenerator = jwtGenerator;
            _options = jwtOptions;
            _slidingExpirationOptions = slidingExpirationOptions;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Cookie 登录不能触发 Bearer 续期，也不能把 Cookie 身份签成访问令牌。
                var authentication = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
                if (authentication.Succeeded)
                {
                    var token = authentication.Properties.GetTokenValue("access_token");
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                        var now = DateTime.UtcNow;
                        if (jwt.ValidTo > now && jwt.ValidTo <= now.Add(_slidingExpirationOptions.SlidingExpiration))
                        {
                            var claims = jwt.Claims.Where(claim => claim.Type != "exp" && claim.Type != "nbf" &&
                                claim.Type != "iat" && claim.Type != "iss").ToArray();
                            // aud 保留在原始 claims 中，确保多个受众不会在续期时丢失。
                            var renewed = _jwtGenerator.GetType() == typeof(JwtGenerator)
                                ? JwtGenerator.GenerateToken(_options, _slidingExpirationOptions.ExpireTimeSpan,
                                    claims, jwt.Issuer, string.Empty)
                                : _jwtGenerator.Generate(_slidingExpirationOptions.ExpireTimeSpan,
                                    claims.Where(claim => claim.Type != "aud").ToDictionary(claim => claim.Type, claim => claim.Value),
                                    jwt.Issuer, jwt.Audiences.FirstOrDefault());
                            context.Response.Headers[_slidingExpirationOptions.NewTokenHeaderName] = renewed.Token;
                        }
                    }
                }
            }
            catch (Exception error)
            {
                _logger.LogError(error, "JWT滑动过期中间件出现异常");
            }
            await _next(context);
        }
    }
}
