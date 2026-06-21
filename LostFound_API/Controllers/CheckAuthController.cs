using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FBLA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckAuthController : ControllerBase
    {
        // GET: api/<CheckAuthController>
        [HttpGet("check-auth")]
        public IActionResult CheckAuth()
        {
            var user = HttpContext.User.Identity;
            if (user != null && user.IsAuthenticated)
            {
                return Ok(new { isAuthenticated = true, username = user.Name });
            }

            return Unauthorized(new { isAuthenticated = false });
        }

        // GET api/<CheckAuthController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<CheckAuthController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<CheckAuthController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CheckAuthController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
