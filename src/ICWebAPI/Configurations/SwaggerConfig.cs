using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;

namespace ICWebAPI.Configurations
{
    public static class SwaggerConfig
    {
        public static void AddSwaggerConfiguration(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            //services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            services.AddSwaggerGen(s =>
            {
                var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var description in provider.ApiVersionDescriptions)
                {
                    s.SwaggerDoc(
                        description.GroupName,
                        new OpenApiInfo
                        {
                            Version = description.ApiVersion.ToString(),
                            Title = "EF Core",
                            Description = "EF Core API Swagger",
                            Contact = new OpenApiContact { Name = "João Paulo", Email = "contato@joaopaulo.com.br", Url = new Uri("http://www.joaopaulo.com.br") },
                            License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://github.com/JPSilvaCode/EFCore") }
                        });
                }

                //s.OperationFilter<SwaggerDefaultValues>();

                s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Insira o token JWT desta maneira: Bearer {seu token}",
                    Name = "Authorization",
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                s.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        public static void UseSwaggerSetup(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }

                options.DocExpansion(DocExpansion.List);
            });
        }
    }
}