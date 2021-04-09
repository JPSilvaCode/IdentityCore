using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ICWebAPI.Authorization
{
    public class DeleteCustomerRequirementHandler : AuthorizationHandler<DeleteCustomerRequirement>
    {
        private const string AdministratorRoleName = "Admin";

        private AuthorizationHandlerContext _context;

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DeleteCustomerRequirement requirement)
        {
            _context = context;

            var isAdministrator = IsAdministrator();
            var canDeleteUser = HasRequirements(requirement);

            if (isAdministrator && canDeleteUser) context.Succeed(requirement);

            return Task.CompletedTask;
        }

        private bool IsAdministrator() =>
            GetClaim(ClaimTypes.Role, AdministratorRoleName);

        private bool HasRequirements(DeleteCustomerRequirement requirement) =>
            GetClaim("Customer", requirement.RequiredPermission);

        private bool GetClaim(string type, string value) => _context.User.Claims.Any(c => c.Type == type &&
            c.Value.Split(',').Contains(value));
    }
}