using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Web.Http;

namespace ExampleWebSite.Api.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ApiController
    {
        [HttpPost("signout")]
        public async Task<IActionResult> SingOutAsync()
        {
            await Context.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok();
        }
    }
}