using ICWebAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICWebAPI.Controllers
{
    [Route("[controller]")]
    public class ClaimsController : MainController
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ClaimsController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetClaims()
        {
            var users = await _userManager.Users.ToListAsync();

            IList<ClaimBinding> claims = null;
            foreach (var user in users)
            {
                var claimsUser = await _userManager.GetClaimsAsync(user);

                if (claimsUser == null) continue;

                claims ??= new List<ClaimBinding>();

                foreach (var claimUser in claimsUser)
                {
                    claims.Add(new ClaimBinding {Type = claimUser.Type, Value = claimUser.Value});
                }
            }

            if (claims == null) return NotFound();

            //var result = claims.GroupBy(c => c.Type, c => c.Value, (key, g) => new { key}).ToList();
            var result = claims.GroupBy(c => new {c.Type, c.Value}, (key, g) => new {key, g}).Select(g => new { g.key.Type, g.key.Value }).ToList();

            return Ok(result);
        }
    }
}
