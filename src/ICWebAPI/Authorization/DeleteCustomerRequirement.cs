using Microsoft.AspNetCore.Authorization;

namespace ICWebAPI.Authorization
{
    public class DeleteCustomerRequirement : IAuthorizationRequirement
    {
        public string RequiredPermission { get; }

        public DeleteCustomerRequirement(string requiredPermission)
        {
            RequiredPermission = requiredPermission;
        }
    }
}