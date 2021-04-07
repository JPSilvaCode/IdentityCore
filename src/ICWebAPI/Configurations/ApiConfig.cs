using Microsoft.Extensions.DependencyInjection;
using System;

namespace ICWebAPI.Configurations
{
    public static class ApiConfig
    {
        public static void AddApiConfiguration(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddControllers();
        }
    }
}