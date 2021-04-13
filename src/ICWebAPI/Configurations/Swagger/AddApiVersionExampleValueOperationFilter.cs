using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace ICWebAPI.Configurations.Swagger
{
    public class AddApiVersionExampleValueOperationFilter : IOperationFilter
    {
        private const string ApiVersionQueryParameter = "api-version";
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiVersionParameter = operation.Parameters.SingleOrDefault(p => p.Name == ApiVersionQueryParameter);
            if (apiVersionParameter == null) return;

            var attribute = context?.MethodInfo?.DeclaringType?
                .GetCustomAttributes(typeof(ApiVersionAttribute), false)
                .Cast<ApiVersionAttribute>()
                .SingleOrDefault();
            
            var version = attribute?.Versions?.SingleOrDefault()?.ToString();

            if (version == null) return;

            apiVersionParameter.Example = new OpenApiString(version);
            apiVersionParameter.Schema.Example = new OpenApiString(version);
        }
    }
}