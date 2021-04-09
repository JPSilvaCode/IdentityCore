using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ICWebAPI.Authorization
{
    public static class CustomAuthorizationValidation
    {
        public static bool UserHasValidClaim(HttpContext context, string claimName, string claimValue)
            => context.User.Identity != null &&
               context.User.Identity.IsAuthenticated &&
               context.User.Claims.Any(c => c.Type == claimName && 
                                            c.Value.Split(',').Contains(claimValue));
    }
}