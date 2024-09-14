using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetCore.Web.Extension;

namespace WebApplication.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IJwtGenerator Generator { get; }

        public AccountController(IJwtGenerator generator)
        {
            Generator = generator;
        }

        [HttpGet("token")]
        public IActionResult Token()
        {
            return Ok(Generator.Generate(TimeSpan.FromMinutes(10),
                new Dictionary<string, string> { { "nick", "owen" } }, "test1", "test2"));
        }

        [HttpGet("nickname")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> NickName()
        {
            var u = User;
            return Ok(new
                { User.Identity.IsAuthenticated, c = User.Claims.FirstOrDefault(p => p.Type == "nick")?.Value });
        }
    }
}