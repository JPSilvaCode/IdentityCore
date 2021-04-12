using ICWebAPI.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ICWebAPI.Configurations
{
    public static class AuthConfig
    {
        public static void AddIdentityConfiguration(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IAuthorizationHandler, DeleteCustomerRequirementHandler>();
        }
    }
}