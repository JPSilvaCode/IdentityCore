using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace ICWebAPI.Controllers
{
    [Route("[controller]")]
    public class ClaimsController : MainController
    {
        [HttpGet]
        public IActionResult GetClaims()
        {
            if (User.Identity is ClaimsIdentity identity)
            {
                var claims = from c in identity.Claims
                             select new
                             {
                                 subject = c.Subject.Name,
                                 type = c.Type,
                                 value = c.Value
                             };

                return Ok(claims);
            }

            return NotFound();
        }
    }
}
