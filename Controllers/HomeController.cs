using BlogAspNet_Performance.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace BlogAspNet_Performance.Controllers
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        // Método comum apenas para testar se a API está online.
        [HttpGet("")]
        //[ApiKey]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
