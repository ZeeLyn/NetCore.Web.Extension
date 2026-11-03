using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetCore.Web.Extension;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

var key = JwtGenerator.GenerateSecurityKey();
var checks = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
    checks++;
}

void Jwt(JwtOptions options, bool issuer = true)
{
    options.SecurityKey = key;
    if (issuer)
    {
        options.ValidIssuer = "issuer";
        options.ValidAudience = "audience";
    }
}

string Token(TimeSpan lifetime, string issuer = "issuer", string audience = "audience",
    string algorithm = SecurityAlgorithms.HmacSha512, string signingKey = null, bool multiple = false)
{
    var claims = new List<Claim> { new("nick", "bearer"), new("role", "reader") };
    if (multiple)
    {
        claims.Add(new Claim("role", "writer"));
        claims.Add(new Claim("aud", "other-audience"));
    }
    var now = DateTime.UtcNow;
    return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(issuer, audience, claims,
        now.AddHours(-1), now.Add(lifetime),
        new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey ?? key)), algorithm)));
}

foreach (var mode in new[] { "bearer", "cookie", "jwt-cookie", "multi", "multi-jwt", "no-issuer", "custom" })
{
    var builder = WebApplication.CreateBuilder();
    builder.Logging.ClearProviders();
    builder.WebHost.UseUrls("http://127.0.0.1:0");
    builder.Services.AddControllers().AddApplicationPart(typeof(CheckController).Assembly);
    var bearer = mode is "bearer" or "multi" or "multi-jwt" or "no-issuer" or "custom";
    var cookie = mode is "cookie" or "jwt-cookie" or "multi" or "multi-jwt";
    var hasIssuer = mode != "no-issuer";
    if (mode == "custom")
    {
        builder.Services.AddGlobalExceptionFilter((context, logger) =>
            context.Result = new ObjectResult("custom exception") { StatusCode = 409 });
        builder.Services.AddGlobalModelStateFilter(context =>
            context.Result = new ObjectResult("custom model") { StatusCode = 422 });
    }
    else
    {
        builder.Services.AddGlobalExceptionFilter();
        builder.Services.AddGlobalModelStateFilter();
    }
    void Cookie(JwtCookieOptions options)
    {
        if (mode != "multi") Jwt(options);
        options.Cookie.Name = "test-auth";
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    }
    if (mode is "multi" or "multi-jwt")
        builder.Services.AddMultiSchemePolicy(Cookie, options => Jwt(options), shareToken: mode == "multi-jwt");
    else if (mode == "cookie")
        builder.Services.AddCookieAuthentication(options => options.Cookie.Name = "test-auth");
    else if (mode == "jwt-cookie")
        builder.Services.AddJwtCookieAuthentication(Cookie);
    else
        builder.Services.AddJwtBearerAuthentication(options => Jwt(options, hasIssuer));
    if (bearer) builder.Services.AddJwtSlidingExpiration();

    await using var app = builder.Build();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    if (bearer) app.UseJwtSlidingExpiration();
    app.MapControllers();
    await app.StartAsync();
    using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false })
    { BaseAddress = new Uri(app.Urls.Single()) };

    async Task<HttpResponseMessage> Get(string path, string token = null, string cookieValue = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (token != null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (cookieValue != null) request.Headers.Add("Cookie", cookieValue);
        return await client.SendAsync(request);
    }
    bool Rejected(HttpResponseMessage response) => response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Redirect;

    Check(Rejected(await Get("/check/any")), $"{mode}: anonymous access");
    var error = await Get("/check/error");
    Check((int)error.StatusCode == (mode == "custom" ? 409 : 500), $"{mode}: exception status");
    Check(!(await error.Content.ReadAsStringAsync()).Contains("private-secret"), $"{mode}: exception disclosure");
    var invalid = await client.PostAsJsonAsync("/check/model", new { });
    Check((int)invalid.StatusCode == (mode == "custom" ? 422 : 400), $"{mode}: invalid model callback");
    Check(!string.IsNullOrWhiteSpace(await invalid.Content.ReadAsStringAsync()), $"{mode}: empty model error");
    Check((await client.PostAsJsonAsync("/check/model", new { name = "ok" })).IsSuccessStatusCode, $"{mode}: valid model");

    string cookieHeader = null;
    if (cookie)
    {
        var login = await client.PostAsync("/check/login", null);
        Check(login.IsSuccessStatusCode, $"{mode}: login");
        var setCookie = login.Headers.GetValues("Set-Cookie").Single(value => value.StartsWith("test-auth="));
        cookieHeader = setCookie.Split(';')[0];
        Check(setCookie.Contains("httponly", StringComparison.OrdinalIgnoreCase), $"{mode}: HttpOnly");
        var cookieOptions = app.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get("Cookies");
        var ticket = cookieOptions.TicketDataFormat.Unprotect(Uri.UnescapeDataString(cookieHeader["test-auth=".Length..]));
        Check(ticket != null && ticket.Properties.ExpiresUtc - ticket.Properties.IssuedUtc == TimeSpan.FromMinutes(5),
            $"{mode}: login expiry argument");
        Check(ticket.Properties.IsPersistent && ticket.Properties.AllowRefresh == true, $"{mode}: ticket properties");
        var authenticated = await Get("/check/any", cookieValue: cookieHeader);
        Check(authenticated.IsSuccessStatusCode && (await authenticated.Content.ReadAsStringAsync()).Contains("cookie"),
            $"{mode}: default Cookie authentication");
        Check(!authenticated.Headers.Contains("x-token"), $"{mode}: Cookie must not refresh Bearer");
        Check((await Get("/check/cookie", cookieValue: cookieHeader)).IsSuccessStatusCode, $"{mode}: explicit Cookie scheme");
        var logout = new HttpRequestMessage(HttpMethod.Post, "/check/logout");
        logout.Headers.Add("Cookie", cookieHeader);
        var signedOut = await client.SendAsync(logout);
        Check(signedOut.IsSuccessStatusCode && signedOut.Headers.Contains("Set-Cookie"), $"{mode}: logout");

        // 构造已过半期的真实票据，验证 Cookie 中间件能自动续期。
        var oldProperties = new AuthenticationProperties
        {
            IssuedUtc = DateTimeOffset.UtcNow.AddMinutes(-4),
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(1),
            IsPersistent = true,
            AllowRefresh = true
        };
        var oldTicket = new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("nick", "cookie") }, "Cookies")), oldProperties, "Cookies");
        var oldCookie = "test-auth=" + Uri.EscapeDataString(cookieOptions.TicketDataFormat.Protect(oldTicket));
        var refreshed = await Get("/check/any", cookieValue: oldCookie);
        Check(refreshed.IsSuccessStatusCode && refreshed.Headers.Contains("Set-Cookie"), $"{mode}: Cookie sliding expiry");
        oldProperties.AllowRefresh = false;
        var noRefreshCookie = "test-auth=" + Uri.EscapeDataString(cookieOptions.TicketDataFormat.Protect(oldTicket));
        Check(!(await Get("/check/any", cookieValue: noRefreshCookie)).Headers.Contains("Set-Cookie"), $"{mode}: AllowRefresh=false");
        oldProperties.IsPersistent = false;
        var sessionTicket = cookieOptions.TicketDataFormat.Unprotect(cookieOptions.TicketDataFormat.Protect(oldTicket));
        Check(sessionTicket != null && !sessionTicket.Properties.IsPersistent, $"{mode}: session Cookie properties");
        oldProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(-1);
        var expiredCookie = "test-auth=" + Uri.EscapeDataString(cookieOptions.TicketDataFormat.Protect(oldTicket));
        Check(Rejected(await Get("/check/any", cookieValue: expiredCookie)), $"{mode}: expired Cookie");
        Check(Rejected(await Get("/check/any", cookieValue: "test-auth=invalid")), $"{mode}: malformed Cookie");
        if (mode is "jwt-cookie" or "multi-jwt")
        {
            var sharedToken = Token(TimeSpan.FromMinutes(1), multiple: true);
            var sharedCookie = await Get("/check/any", cookieValue: "test-auth=" + sharedToken);
            Check(sharedCookie.IsSuccessStatusCode, $"{mode}: Bearer Token as JWT Cookie");
            var renewedCookieValue = sharedCookie.Headers.GetValues("Set-Cookie").Single(value => value.StartsWith("test-auth="))
                .Split(';')[0]["test-auth=".Length..];
            var renewedCookieToken = new JwtSecurityTokenHandler().ReadJwtToken(Uri.UnescapeDataString(renewedCookieValue));
            Check(renewedCookieToken.Audiences.Count() == 2, $"{mode}: JWT Cookie preserves multiple audiences");
            Check(cookieOptions.TicketDataFormat.Unprotect(Token(TimeSpan.FromMinutes(5), algorithm: SecurityAlgorithms.HmacSha256)) == null,
                $"{mode}: JWT Cookie wrong algorithm");
            Check(cookieOptions.TicketDataFormat.Unprotect(Token(TimeSpan.FromMinutes(5), issuer: "wrong")) == null,
                $"{mode}: JWT Cookie wrong issuer");
            Check(cookieOptions.TicketDataFormat.Unprotect(Token(TimeSpan.FromMinutes(5), audience: "wrong")) == null,
                $"{mode}: JWT Cookie wrong audience");
        }
    }
    if (bearer)
    {
        var generator = app.Services.GetRequiredService<IJwtGenerator>();
        var generated = generator.Generate(TimeSpan.FromMinutes(30), new Dictionary<string, string> { ["nick"] = "bearer" });
        Check(new JwtSecurityTokenHandler().ReadJwtToken(generated.Token).Header.Alg == "HS512", $"{mode}: JOSE algorithm name");
        var generatedResponse = await Get("/check/any", generated.Token);
        Check(generatedResponse.IsSuccessStatusCode, $"{mode}: generator defaults ({generatedResponse.StatusCode}, {await generatedResponse.Content.ReadAsStringAsync()})");
        Check(!(await Get("/check/any", generated.Token)).Headers.Contains("x-token"), $"{mode}: early refresh");
        Check((await Get("/check/bearer", generated.Token)).IsSuccessStatusCode, $"{mode}: explicit Bearer scheme");
        Check((await Get("/check/admin", generated.Token)).StatusCode == HttpStatusCode.Forbidden, $"{mode}: forbidden");
        Check((await Get("/check/any", "broken", cookieHeader)).StatusCode == HttpStatusCode.Unauthorized, $"{mode}: invalid Bearer fallback");
        Check((await Get("/check/any", Token(TimeSpan.FromMinutes(-1)))).StatusCode == HttpStatusCode.Unauthorized, $"{mode}: expired Bearer");
        Check((await Get("/check/any", Token(TimeSpan.FromMinutes(5), signingKey: JwtGenerator.GenerateSecurityKey()))).StatusCode == HttpStatusCode.Unauthorized, $"{mode}: wrong key");
        Check((await Get("/check/any", Token(TimeSpan.FromMinutes(5), algorithm: SecurityAlgorithms.HmacSha256))).StatusCode == HttpStatusCode.Unauthorized, $"{mode}: wrong algorithm");
        if (hasIssuer)
        {
            Check((await Get("/check/any", Token(TimeSpan.FromMinutes(5), issuer: "wrong"))).StatusCode == HttpStatusCode.Unauthorized, $"{mode}: wrong issuer");
            Check((await Get("/check/any", Token(TimeSpan.FromMinutes(5), audience: "wrong"))).StatusCode == HttpStatusCode.Unauthorized, $"{mode}: wrong audience");
        }
        var renewal = await Get("/check/any", Token(TimeSpan.FromMinutes(1), multiple: true));
        Check(renewal.IsSuccessStatusCode && renewal.Headers.Contains("x-token"), $"{mode}: Bearer renewal");
        var renewedToken = renewal.Headers.GetValues("x-token").Single();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(renewedToken);
        Check(jwt.Claims.Count(claim => claim.Type == "role") == 2, $"{mode}: repeated roles");
        Check(jwt.Audiences.OrderBy(value => value).SequenceEqual(new[] { "audience", "other-audience" }), $"{mode}: multiple audiences");
        Check((await Get("/check/any", renewedToken)).IsSuccessStatusCode, $"{mode}: renewed Token validates");
        if (cookie)
        {
            var cookieToken = Uri.UnescapeDataString(cookieHeader["test-auth=".Length..]);
            var sharedBearer = await Get("/check/any", cookieToken);
            Check(mode == "multi-jwt" ? sharedBearer.IsSuccessStatusCode : sharedBearer.StatusCode == HttpStatusCode.Unauthorized,
                $"{mode}: shareToken setting");
            Check((await Get("/check/bearer", cookieValue: cookieHeader)).StatusCode == HttpStatusCode.Unauthorized, $"{mode}: Cookie cannot access Bearer-only endpoint");
            Check(Rejected(await Get("/check/cookie", generated.Token)), $"{mode}: Bearer cannot access Cookie-only endpoint");
            var both = await Get("/check/any", generated.Token, cookieHeader);
            Check(both.IsSuccessStatusCode && (await both.Content.ReadAsStringAsync()).Contains("bearer"), $"{mode}: Bearer priority");
        }
    }
    await app.StopAsync();
    Console.WriteLine($"PASS {mode}");
}

// UTF-8 密钥在生成与验证两端必须一致。
var unicodeOptions = new JwtOptions { SecurityKey = new string('密', 32) };
var unicodeServices = new ServiceCollection();
unicodeServices.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
unicodeServices.AddLogging();
unicodeServices.AddJwtBearerAuthentication(options => options.SecurityKey = unicodeOptions.SecurityKey);
using (var provider = unicodeServices.BuildServiceProvider())
{
    var validation = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get("Bearer").TokenValidationParameters;
    var token = provider.GetRequiredService<IJwtGenerator>().Generate(TimeSpan.FromMinutes(5)).Token;
    Check(new JwtSecurityTokenHandler().ValidateToken(token, validation, out _).Identity.IsAuthenticated, "UTF-8 key");
}
try
{
    new ServiceCollection().AddJwtBearerAuthentication(options => options.SecurityKey = "1234567890123456");
    throw new InvalidOperationException("Weak key was accepted");
}
catch (ArgumentOutOfRangeException) { checks++; }
var state = new ModelStateDictionary();
state.AddModelError("field", new InvalidOperationException("private-secret"), new EmptyModelMetadataProvider().GetMetadataForType(typeof(string)));
var actionContext = new ActionExecutingContext(new ActionContext(new DefaultHttpContext(), new(), new(), state), new List<IFilterMetadata>(), new Dictionary<string, object>(), new object());
new GlobalModelStateFilter(Options.Create(new GlobalModelStateOptions())).OnActionExecuting(actionContext);
Check(actionContext.Result is BadRequestObjectResult result && result.Value is string value && value.Length > 0 && !value.Contains("private-secret"), "exception-only model error");
Console.WriteLine($"PASS: {checks} assertions ({System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription})");

[ApiController]
[Route("check")]
public class CheckController : ControllerBase
{
    [HttpGet("any"), Authorize]
    public IActionResult Any() => Ok(User.GetClaimValue("nick"));
    [HttpGet("bearer"), Authorize(AuthenticationSchemes = "Bearer")]
    public IActionResult Bearer() => Any();
    [HttpGet("cookie"), Authorize(AuthenticationSchemes = "Cookies")]
    public IActionResult Cookie() => Any();
    [HttpGet("admin"), Authorize(Roles = "admin")]
    public IActionResult Admin() => Any();
    [HttpGet("error")]
    public IActionResult Error() => throw new InvalidOperationException("private-secret");
    [HttpPost("model")]
    public IActionResult Model(CheckModel model) => Ok(model);
    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        await this.SignInAsync(new Dictionary<string, string> { ["nick"] = "cookie" }, TimeSpan.FromMinutes(5));
        return Ok();
    }
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await this.SignOutAsync();
        return Ok();
    }
}

public class CheckModel
{
    [Required]
    public string Name { get; set; }
}
