using ICWebAPI.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace ICWebAPI.Configurations
{
    public static class AuthConfig
    {
        public static void AddIdentityConfiguration(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandler, DeleteCustomerRequirementHandler>();
        }
    }
}