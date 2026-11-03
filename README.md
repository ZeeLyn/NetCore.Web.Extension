# NetCore.Web.Extension

ASP.NET Core 的 JWT / Cookie 认证、MVC 全局异常和 ModelState 验证扩展。NuGet 包名为 `DotNetCore.Web.Extension`，命名空间仍为 `NetCore.Web.Extension`，保留 .NET 7 / 8 / 9 / 10 目标框架。

## 认证注册

以下三种方式选择一种。JWT 配置中的 `SecurityKey` 应从安全配置读取并在重启及多实例间保持一致，可以用 `JwtGenerator.GenerateSecurityKey()` 生成后保存。

### 只使用 JWT

```csharp
services.AddJwtBearerAuthentication(options =>
{
    options.SecurityKey = configuration["Authentication:SecurityKey"];
    options.ValidIssuer = "my-app";
    options.ValidAudience = "my-api";
});
```

通过 `IJwtGenerator` 签发令牌，省略 issuer / audience 时使用配置中的 `ValidIssuer` / `ValidAudience`：

```csharp
var result = generator.Generate(TimeSpan.FromHours(2),
    new Dictionary<string, string> { ["nick"] = "owen" });
```

客户端使用 `Authorization: Bearer <token>`。配置了 issuer / audience 时会严格校验；未配置时不校验对应项。只配置 `ValidIssuers` / `ValidAudiences` 列表时，签发令牌仍需明确传入对应值。

### 只使用普通 Cookie

```csharp
services.AddCookieAuthentication(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/denied";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
});
```

普通 Cookie 使用 ASP.NET Core Data Protection 加密认证票据，不需要 JWT 密钥。多实例部署需共享 Data Protection 密钥环和应用名称。Cookie 的重定向及 API 401 / 403 行为沿用目标 ASP.NET Core 版本的默认行为，可通过 `Events` 自定义。

控制器登录及退出沿用现有方法：

```csharp
await this.SignInAsync(new Dictionary<string, string> { ["nick"] = "owen" },
    TimeSpan.FromHours(2));
await this.SignOutAsync();
```

`SignInAsync` 的 `expire` 同时控制票据和持久化 Cookie 的有效期。Cookie 登录和其他修改数据的请求应按 ASP.NET Core 的方式验证防伪令牌；示例登录表单已启用。

### 同时使用 JWT 和 Cookie

继续使用现有 `AddMultiSchemePolicy`，不要重复调用单方案注册方法：

```csharp
services.AddMultiSchemePolicy(cookie =>
{
    cookie.LoginPath = "/login";
    cookie.ExpireTimeSpan = TimeSpan.FromHours(2);
    cookie.SlidingExpiration = true;
}, jwt =>
{
    jwt.SecurityKey = configuration["Authentication:SecurityKey"];
    jwt.ValidIssuer = "my-app";
    jwt.ValidAudience = "my-api";
}, shareToken: false);
```

`shareToken: false` 使用普通 Cookie，cookie 配置中的 JWT 密钥及 issuer / audience 不参与验证。带 Bearer 头的请求使用 JWT，否则使用 Cookie；同时携带两者时 Bearer 优先。无效 Bearer 返回 401，不回退到 Cookie。`[Authorize]` 因而可以用于两种登录方式。

需要限制某个端点的认证方式时，继续显式指定已有方案名：

```csharp
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Bearer
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)] // Cookies
```

转发使用 ASP.NET Core 的 [Policy schemes](https://learn.microsoft.com/aspnet/core/security/authentication/policyschemes) 实现。

### 保留 JWT Cookie 兼容模式

`AddJwtCookieAuthentication(Action<JwtCookieOptions>)` 仍然可用；`AddMultiSchemePolicy` 的 `shareToken` 默认值也仍为 `true`。在这两种情况下，Cookie 存放签名 JWT，需要给 cookie 配置 `SecurityKey`。若要让同一个令牌在 Cookie / Bearer 间通用，两端的密钥、算法、issuer / audience 配置需要一致。

JWT Cookie 只有签名，没有加密，claims 和票据属性可被读取，不应存放秘密数据；一般网页登录可使用上述普通 Cookie 模式。JWT Cookie 现在保留签发时间、过期时间、持久化及 `AllowRefresh` 等票据属性，以便 Cookie 自动续期正确工作。`JwtCookieOptions.SlidingExpiration` 仍默认关闭，`ExpireTimeSpan` 默认 14 天，Cookie 默认 HttpOnly、SameSite=Lax。

## 中间件与 JWT 续期

```csharp
services.AddJwtSlidingExpiration(options =>
{
    options.SlidingExpiration = TimeSpan.FromMinutes(10);
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.NewTokenHeaderName = "x-token";
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseJwtSlidingExpiration();
// 然后映射控制器端点
```

JWT 续期只用于注册了 Bearer 的应用。剩余有效期达到阈值时，通过响应头返回新令牌，客户端需保存并替换旧令牌；Cookie 身份不会触发 JWT 续期。内置生成器续期时保留原始 claim 名称、重复角色和多个受众。自定义 `IJwtGenerator` 仍沿用原有字典接口，仅支持每种类型一个 claim、一个 audience。跨域客户端读取新令牌时还需在自己的 CORS 策略中暴露该响应头。

## 全局过滤器

```csharp
services.AddControllers(); // MVC 应用可使用 AddControllersWithViews()
services.AddGlobalExceptionFilter();
services.AddGlobalModelStateFilter();
```

异常过滤器记录异常日志，默认返回不含异常详情的 HTTP 500 ProblemDetails。ModelState 验证失败返回 HTTP 400 和错误文本，只有绑定异常而没有可展示消息时返回通用错误。

原有自定义回调注册方式保留，并修复为通过 `IOptions` 实际生效：

```csharp
services.AddGlobalExceptionFilter((context, logger) =>
{
    logger.LogError(context.Exception, "Request failed");
    context.Result = new ObjectResult("自定义错误") { StatusCode = 500 };
});
services.AddGlobalModelStateFilter(context =>
{
    context.Result = new BadRequestObjectResult("请检查输入");
});
```

MVC 异常过滤器覆盖 MVC 控制器执行阶段，不覆盖任意中间件、Minimal API 或响应序列化阶段。ModelState 过滤器也只适用于 MVC。需要覆盖整个请求管线时，应用应另外配置 ASP.NET Core 异常处理中间件。

## 兼容性变化

- 保留现有注册方法、登录方法、选项类型和 `Bearer` / `Cookies` 方案名；新增 `AddCookieAuthentication`。
- 默认算法仍为 HS512；密钥按 UTF-8 字节长度检查，HS256 / HS384 / HS512 分别至少需要 32 / 48 / 64 字节。弱密钥在注册时直接报错。生成和验证统一使用 UTF-8。
- 新令牌使用标准 JOSE 算法短名称，同时接受原配置算法的旧 URI 名称；不会放行其他签名算法。
- 默认异常响应从包含异常消息的 HTTP 400 改为通用 HTTP 500；可用现有回调定制。
- 删除 AutoGenerateHtmlControl、分页扩展及其演示、依赖和旧备份项目。依赖这些公开类型的应用需要迁移；发布时应使用新的主版本。当前包版本未自动调整。

## 本地验证

```powershell
dotnet build NetCore.Web.Extension.sln -c Release
foreach ($framework in 'net7.0', 'net8.0', 'net9.0', 'net10.0') {
    dotnet run --project tests/NetCore.Web.Extension.Tests -c Release --framework $framework --no-build
    if ($LASTEXITCODE -ne 0) { throw "Failed: $framework" }
}
```

回归程序通过本地 Kestrel 的真实 HTTP 请求及断言验证单 JWT、普通 Cookie、JWT Cookie、两种双方案、自定义过滤器、无 issuer 配置、错误密钥 / 算法 / 受众、有效期、续期及方案隔离，不额外引入测试依赖。运行对应目标框架需要安装相应 ASP.NET Core 运行时。.NET 7 已停止支持，此处保留目标框架用于兼容现有用户。

`WebApplication` 仅演示认证流程，示例中的昵称登录和公开 token 接口不能替代生产用户验证。未设置 `Authentication:SecurityKey` 时示例会在启动时生成临时密钥，重启后旧 JWT 失效。
