using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace NetCore.Web.Extension
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddGlobalExceptionFilter(this IServiceCollection services)
        {
            return services.Configure<MvcOptions>(options => { options.Filters.Add<GlobalExceptionFilter>(); });
        }

        public static IServiceCollection AddGlobalExceptionFilter(this IServiceCollection services,
            Action<ExceptionContext, ILogger> builder)
        {
            return services.AddGlobalExceptionFilter().AddSingleton(new GlobalExceptionOptions { Action = builder });
        }

        public static IServiceCollection AddGlobalModelStateFilter(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(options => { options.Filters.Add<GlobalModelStateFilter>(); });
            return services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
        }

        public static IServiceCollection AddGlobalModelStateFilter(this IServiceCollection services,
            Action<ActionExecutingContext> builder)
        {
            return services.AddGlobalModelStateFilter().AddSingleton(new GlobalModelStateOptions { Action = builder });
        }

        public static IServiceCollection AddJwtBearerAuthentication(this IServiceCollection services,
            Action<JwtOptions> builder)
        {
            var options = new JwtOptions();
            builder?.Invoke(options);
            if (string.IsNullOrWhiteSpace(options.SecurityKey))
                throw new ArgumentNullException(nameof(options.SecurityKey));
            if (options.SecurityKey.Length < 16)
                throw new ArgumentOutOfRangeException(nameof(options.SecurityKey),
                    "SecurityKey length cannot be less than 16.");
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(options.SecurityKey));
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            if (!string.IsNullOrWhiteSpace(options.ValidIssuer) ||
                options.ValidIssuers != null && options.ValidIssuers.Any())
            {
                validationParameters.ValidateIssuer = true;
                validationParameters.ValidIssuer = options.ValidIssuer;
                validationParameters.ValidIssuers = options.ValidIssuers;
            }

            if (!string.IsNullOrWhiteSpace(options.ValidAudience) ||
                options.ValidAudiences != null && options.ValidAudiences.Any())
            {
                validationParameters.ValidateAudience = true;
                validationParameters.ValidAudience = options.ValidAudience;
                validationParameters.ValidAudiences = options.ValidAudiences;
            }

            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option =>
            {
                option.TokenValidationParameters = validationParameters;
                if (options.Events != null)
                    option.Events = options.Events;
            });

            return services.AddSingleton<IJwtGenerator, JwtGenerator>().AddSingleton(options);
        }

        public static IServiceCollection AddJwtCookieAuthentication(this IServiceCollection services,
            Action<JwtCookieOptions> builder)
        {
            var options = new JwtCookieOptions();
            builder?.Invoke(options);
            if (string.IsNullOrWhiteSpace(options.SecurityKey))
                throw new ArgumentNullException(nameof(options.SecurityKey));
            if (options.SecurityKey.Length < 16)
                throw new ArgumentOutOfRangeException(nameof(options.SecurityKey),
                    "SecurityKey length cannot be less than 16.");
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(options.SecurityKey));
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            if (!string.IsNullOrWhiteSpace(options.ValidIssuer) ||
                options.ValidIssuers != null && options.ValidIssuers.Any())
            {
                validationParameters.ValidateIssuer = true;
                validationParameters.ValidIssuer = options.ValidIssuer;
                validationParameters.ValidIssuers = options.ValidIssuers;
            }

            if (!string.IsNullOrWhiteSpace(options.ValidAudience) ||
                options.ValidAudiences != null && options.ValidAudiences.Any())
            {
                validationParameters.ValidateAudience = true;
                validationParameters.ValidAudience = options.ValidAudience;
                validationParameters.ValidAudiences = options.ValidAudiences;
            }

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option =>
            {
                option.Cookie = options.Cookie;
                option.LoginPath = options.LoginPath;
                option.AccessDeniedPath = options.AccessDeniedPath;
                option.TicketDataFormat = new JwtCookieDataFormat(validationParameters, options);
                option.SlidingExpiration = options.SlidingExpiration;
                option.ExpireTimeSpan = options.ExpireTimeSpan;
                if (options.Events != null)
                    option.Events = options.Events;
            });

            return services;
        }

        public static IServiceCollection AddMultiSchemePolicy(this IServiceCollection services,
            Action<JwtCookieOptions> cookieBuilder,
            Action<JwtOptions> jwtBuilder, bool shareToken = true)
        {
            var options = new JwtCookieOptions();
            cookieBuilder?.Invoke(options);
            if (string.IsNullOrWhiteSpace(options.SecurityKey))
                throw new ArgumentNullException(nameof(options.SecurityKey));
            if (options.SecurityKey.Length < 16)
                throw new ArgumentOutOfRangeException(nameof(options.SecurityKey),
                    "SecurityKey length cannot be less than 16.");
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(options.SecurityKey));
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            if (!string.IsNullOrWhiteSpace(options.ValidIssuer) ||
                options.ValidIssuers != null && options.ValidIssuers.Any())
            {
                validationParameters.ValidateIssuer = true;
                validationParameters.ValidIssuer = options.ValidIssuer;
                validationParameters.ValidIssuers = options.ValidIssuers;
            }

            if (!string.IsNullOrWhiteSpace(options.ValidAudience) ||
                options.ValidAudiences != null && options.ValidAudiences.Any())
            {
                validationParameters.ValidateAudience = true;
                validationParameters.ValidAudience = options.ValidAudience;
                validationParameters.ValidAudiences = options.ValidAudiences;
            }


            var jwtOptions = new JwtOptions();
            jwtBuilder?.Invoke(jwtOptions);
            if (string.IsNullOrWhiteSpace(jwtOptions.SecurityKey))
                throw new ArgumentNullException(nameof(jwtOptions.SecurityKey));
            if (jwtOptions.SecurityKey.Length < 16)
                throw new ArgumentOutOfRangeException(nameof(jwtOptions.SecurityKey),
                    "SecurityKey length cannot be less than 16.");
            var jwtSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions.SecurityKey));
            var jwtValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = jwtSigningKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            if (!string.IsNullOrWhiteSpace(jwtOptions.ValidIssuer) ||
                jwtOptions.ValidIssuers != null && jwtOptions.ValidIssuers.Any())
            {
                jwtValidationParameters.ValidateIssuer = true;
                jwtValidationParameters.ValidIssuer = jwtOptions.ValidIssuer;
                jwtValidationParameters.ValidIssuers = jwtOptions.ValidIssuers;
            }

            if (!string.IsNullOrWhiteSpace(jwtOptions.ValidAudience) ||
                jwtOptions.ValidAudiences != null && jwtOptions.ValidAudiences.Any())
            {
                jwtValidationParameters.ValidateAudience = true;
                jwtValidationParameters.ValidAudience = jwtOptions.ValidAudience;
                jwtValidationParameters.ValidAudiences = jwtOptions.ValidAudiences;
            }


            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                option.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, option =>
            {
                option.Cookie = options.Cookie;
                option.LoginPath = options.LoginPath;
                option.AccessDeniedPath = options.AccessDeniedPath;
                if (shareToken)
                    option.TicketDataFormat = new JwtCookieDataFormat(validationParameters, options);
                option.SlidingExpiration = options.SlidingExpiration;
                option.ExpireTimeSpan = options.ExpireTimeSpan;
                if (options.Events != null)
                    option.Events = options.Events;
            }).AddJwtBearer(option =>
            {
                option.TokenValidationParameters = jwtValidationParameters;
                if (jwtOptions.Events != null)
                    option.Events = jwtOptions.Events;
            });

            return services.AddSingleton<IJwtGenerator, JwtGenerator>().AddSingleton(jwtOptions);
        }
    }
}