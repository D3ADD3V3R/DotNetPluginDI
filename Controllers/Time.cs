using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Anna.Controllers
{
    [Route("api/time")]
    [ApiController]
    public class Time : ControllerBase
    {
        // GET: api/<time>
        [HttpGet]
        public ActionResult<DateTime> Get()
        {
            return Ok(DateTime.Now);
        }

        // GET api/<time>/5
        [HttpGet("utc")]
        public ActionResult<DateTime> GetUtc()
        {
            return Ok(DateTime.UtcNow);
        }

    }
}
