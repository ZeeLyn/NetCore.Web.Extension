namespace NetCore.Web.Extension
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="JwtRefreshMiddleware" />
    /// </summary>
    public class JwtRefreshMiddleware
    {
        /// <summary>
        /// Defines the _next
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// Defines the JwtGenerator
        /// </summary>
        private readonly IJwtGenerator JwtGenerator;

        /// <summary>
        /// Defines the options
        /// </summary>
        private readonly JwtOptions options;

        /// <summary>
        /// Defines the _slidingExpirationOptions
        /// </summary>
        private readonly JwtSlidingExpirationOptions _slidingExpirationOptions;

        /// <summary>
        /// Defines the excludeClaims
        /// </summary>
        private readonly List<string> excludeClaims = new List<string> { "exp", "nbf", "iat", "iss", "aud" };

        /// <summary>
        /// Gets the Logger
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtRefreshMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next<see cref="RequestDelegate"/></param>
        /// <param name="jwtGenerator">The jwtGenerator<see cref="IJwtGenerator"/></param>
        /// <param name="jwtOptions">The jwtOptions<see cref="JwtOptions"/></param>
        /// <param name="slidingExpirationOptions">The slidingExpirationOptions<see cref="JwtSlidingExpirationOptions"/></param>
        /// <param name="logger">The logger<see cref="ILogger{JwtRefreshMiddleware}"/></param>
        public JwtRefreshMiddleware(RequestDelegate next, IJwtGenerator jwtGenerator, JwtOptions jwtOptions, JwtSlidingExpirationOptions slidingExpirationOptions, ILogger<JwtRefreshMiddleware> logger)
        {
            _next = next;
            JwtGenerator = jwtGenerator;
            options=jwtOptions;
            _slidingExpirationOptions=slidingExpirationOptions;
            Logger= logger;
        }

        /// <summary>
        /// The InvokeAsync
        /// </summary>
        /// <param name="context">The context<see cref="HttpContext"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    var principal = JwtGenerator.GetPrincipalFromToken(token, out _);
                    var exp = principal.FindFirstValue("exp");
                    var expTime = long.Parse(exp);
                    var e = DateTimeOffset.Now.Add(_slidingExpirationOptions.SlidingExpiration).ToUnixTimeSeconds();
                    if (expTime<=e)
                    {
                        var claims = principal.Claims.Where(x => !excludeClaims.Contains(x.Type)).ToDictionary(x => x.Type, x => x.Value);
                        var jwt = JwtGenerator.Generate(_slidingExpirationOptions.ExpireTimeSpan, claims, options.ValidIssuer, options.ValidAudience);
                        context.Response.Headers[_slidingExpirationOptions.NewTokenHeaderName]= jwt.Token;
                    }
                }
            }
            catch (Exception err)
            {
                Logger.LogError(err, "JWT滑动过期中间件出现异常");
            }
            await _next(context);
        }
    }
}
