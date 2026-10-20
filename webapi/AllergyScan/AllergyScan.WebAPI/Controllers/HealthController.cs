using Microsoft.AspNetCore.Mvc;

namespace AllergyScan.WebAPI.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "ok",
                utc = DateTime.UtcNow
            });
        }
    }
}
