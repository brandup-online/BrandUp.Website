using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BrandUp.Website.Api.Controllers
{
    [ApiController, Route("api/[controller]/[action]")]
    public class AppController : ControllerBase
    {
        [HttpPost]
        public IActionResult ChangeCity([FromQuery, BindRequired]string id)
        {
            return Ok();
        }
    }
}