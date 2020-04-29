using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace ExampleWebSite.Api.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ApiController
    {
        readonly static Guid userId = new Guid("1df7d641-2fd4-415a-8530-7b0a2afa88ae");
        readonly static string userName = "test";

        [HttpPost("signin")]
        public async Task<IActionResult> SingInAsync()
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, userName)
            };

            var identity = new ClaimsIdentity(claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                IssuedUtc = DateTime.Now,
                ExpiresUtc = DateTime.Now.AddMonths(1)
            };

            await Context.SignInAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            return Ok();
        }

        [HttpPost("signout")]
        public async Task<IActionResult> SingOutAsync()
        {
            await Context.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok();
        }
    }
}