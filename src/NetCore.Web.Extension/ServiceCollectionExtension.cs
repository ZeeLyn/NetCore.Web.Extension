using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace NetCore.Web.Extension
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddGlobalExceptionFilter(this IServiceCollection services)
        {
            services.AddOptions<GlobalExceptionOptions>();
            return services.Configure<MvcOptions>(options => options.Filters.Add<GlobalExceptionFilter>());
        }

        public static IServiceCollection AddGlobalExceptionFilter(this IServiceCollection services,
            Action<ExceptionContext, ILogger> builder)
        {
            return services.AddGlobalExceptionFilter().Configure<GlobalExceptionOptions>(options => options.Action = builder);
        }

        public static IServiceCollection AddGlobalModelStateFilter(this IServiceCollection services)
        {
            services.AddOptions<GlobalModelStateOptions>();
            services.Configure<MvcOptions>(options => options.Filters.Add<GlobalModelStateFilter>());
            return services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
        }

        public static IServiceCollection AddGlobalModelStateFilter(this IServiceCollection services,
            Action<ActionExecutingContext> builder)
        {
            return services.AddGlobalModelStateFilter().Configure<GlobalModelStateOptions>(options => options.Action = builder);
        }

        public static IServiceCollection AddJwtSlidingExpiration(this IServiceCollection services,
            Action<JwtSlidingExpirationOptions> builder = null)
        {
            var options = new JwtSlidingExpirationOptions();
            builder?.Invoke(options);
            if (options.SlidingExpiration < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(options.SlidingExpiration));
            if (options.ExpireTimeSpan <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(options.ExpireTimeSpan));
            if (string.IsNullOrWhiteSpace(options.NewTokenHeaderName))
                throw new ArgumentException("NewTokenHeaderName is required.", nameof(options.NewTokenHeaderName));
            return services.AddSingleton(options);
        }

        /// <summary>放在 UseAuthentication、UseAuthorization 之后，控制器端点之前。</summary>
        public static void UseJwtSlidingExpiration(this IApplicationBuilder app)
        {
            app.UseMiddleware<JwtRefreshMiddleware>();
        }

        public static IServiceCollection AddJwtBearerAuthentication(this IServiceCollection services,
            Action<JwtOptions> builder)
        {
            var options = new JwtOptions();
            builder?.Invoke(options);
            var validationParameters = JwtGenerator.CreateValidationParameters(options);
            services.AddAuthentication(authentication =>
            {
                authentication.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option =>
            {
                option.TokenValidationParameters = validationParameters;
                if (options.Events != null)
                    option.Events = options.Events;
            });
            return services.AddSingleton<IJwtGenerator, JwtGenerator>().AddSingleton(options);
        }

        /// <summary>使用 ASP.NET Core 数据保护票据的普通 Cookie 登录，不需要 JWT 密钥。</summary>
        public static IServiceCollection AddCookieAuthentication(this IServiceCollection services,
            Action<CookieAuthenticationOptions> builder = null)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => builder?.Invoke(options));
            return services;
        }

        /// <summary>保留兼容入口：Cookie 的内容是已签名的 JWT。</summary>
        public static IServiceCollection AddJwtCookieAuthentication(this IServiceCollection services,
            Action<JwtCookieOptions> builder)
        {
            var options = new JwtCookieOptions();
            builder?.Invoke(options);
            var validationParameters = JwtGenerator.CreateValidationParameters(options);
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option =>
            {
                ConfigureCookie(option, options);
                option.TicketDataFormat = new JwtCookieDataFormat(validationParameters, options);
            });
            return services;
        }

        /// <summary>同时支持 Bearer 和 Cookie；shareToken=false 时 Cookie 使用普通加密票据。</summary>
        public static IServiceCollection AddMultiSchemePolicy(this IServiceCollection services,
            Action<JwtCookieOptions> cookieBuilder, Action<JwtOptions> jwtBuilder, bool shareToken = true)
        {
            if (shareToken)
                services.AddJwtCookieAuthentication(cookieBuilder);
            else
            {
                var options = new JwtCookieOptions();
                cookieBuilder?.Invoke(options);
                services.AddCookieAuthentication(cookie => ConfigureCookie(cookie, options));
            }
            services.AddJwtBearerAuthentication(jwtBuilder);
            const string scheme = "JwtOrCookie";
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = scheme;
                options.DefaultAuthenticateScheme = scheme;
                options.DefaultChallengeScheme = scheme;
                options.DefaultForbidScheme = scheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddPolicyScheme(scheme, scheme, options =>
            {
                // 显式传入 Bearer 时验证失败就返回 401，不能回退到已登录的 Cookie。
                options.ForwardDefaultSelector = context =>
                {
                    var authorization = context.Request.Headers.Authorization.ToString().TrimStart();
                    return authorization.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase) &&
                           (authorization.Length == 6 || char.IsWhiteSpace(authorization[6]))
                        ? JwtBearerDefaults.AuthenticationScheme
                        : CookieAuthenticationDefaults.AuthenticationScheme;
                };
                options.ForwardSignIn = CookieAuthenticationDefaults.AuthenticationScheme;
                options.ForwardSignOut = CookieAuthenticationDefaults.AuthenticationScheme;
            });
            return services;
        }

        private static void ConfigureCookie(CookieAuthenticationOptions target, JwtCookieOptions source)
        {
            target.Cookie = source.Cookie;
            if (source.LoginPath != null)
                target.LoginPath = source.LoginPath;
            if (source.AccessDeniedPath != null)
                target.AccessDeniedPath = source.AccessDeniedPath;
            target.SlidingExpiration = source.SlidingExpiration;
            target.ExpireTimeSpan = source.ExpireTimeSpan;
            if (source.Events != null)
                target.Events = source.Events;
        }
    }
}
