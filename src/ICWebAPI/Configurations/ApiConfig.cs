using Microsoft.Extensions.DependencyInjection;
using System;
using ICWebAPI.Authorization;
using ICWebAPI.Service;

namespace ICWebAPI.Configurations
{
    public static class ApiConfig
    {
        public static void AddApiConfiguration(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddControllers();

            services.AddScoped<AuthenticationService>();
            services.AddScoped<IAspNetUser, AspNetUser>();
        }
    }
}