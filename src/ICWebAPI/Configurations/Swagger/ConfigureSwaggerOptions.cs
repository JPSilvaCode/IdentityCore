using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace ICWebAPI.Configurations.Swagger
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }
        }

        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo
            {
                Version = description.ApiVersion.ToString(),
                Title = "EF Core",
                Description = "EF Core API Swagger",
                Contact = new OpenApiContact { Name = "João Paulo", Email = "contato@joaopaulo.com.br", Url = new Uri("http://www.joaopaulo.com.br") },
                License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://github.com/JPSilvaCode/EFCore") }
            };

            if (description.IsDeprecated) info.Description += " Esta versão da API está depreciada.";

            return info;
        }
    }
}