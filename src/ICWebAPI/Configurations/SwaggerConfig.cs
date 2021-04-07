using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;

namespace ICWebAPI.Configurations
{
    public static class SwaggerConfig
    {
        public static void AddSwaggerConfiguration(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "EF Core",
                    Description = "EF Core API Swagger",
                    Contact = new OpenApiContact { Name = "João Paulo", Email = "contato@joaopaulo.com.br", Url = new Uri("http://www.joaopaulo.com.br") },
                    License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://github.com/JPSilvaCode/EFCore") }
                });
            });
        }

        public static void UseSwaggerSetup(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });
        }
    }
}