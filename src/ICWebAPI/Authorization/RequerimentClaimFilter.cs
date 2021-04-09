using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ICWebAPI.Authorization
{
    internal class RequerimentClaimFilter : IAuthorizationFilter
    {
        private readonly Claim _claim;

        public RequerimentClaimFilter(Claim claim) => _claim = claim;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity != null && !context.HttpContext.User.Identity.IsAuthenticated) context.Result = new StatusCodeResult(401);
            else
            {
                if (CustomAuthorizationValidation.UserHasValidClaim(context.HttpContext, _claim.Type, _claim.Value)) return;
                context.Result = new StatusCodeResult(403);
            }
        }
    }
}