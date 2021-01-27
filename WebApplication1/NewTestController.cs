using Microsoft.AspNetCore.Mvc;

namespace WebApplication1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}")]
    public class NewTestController : ControllerBase
    {
        [HttpPost("do-something")]
        public string DoSometing()
        {
            return "Hello";
        }
    }
}
