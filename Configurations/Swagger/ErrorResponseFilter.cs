using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Configurations.Swagger
{
    public class ErrorResponseFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Responses == null)
                operation.Responses = new OpenApiResponses();

            // Add standard error responses
            if (!operation.Responses.ContainsKey("400"))
                operation.Responses.Add("400", new OpenApiResponse { Description = "Bad Request" });

            if (!operation.Responses.ContainsKey("401"))
                operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });

            if (!operation.Responses.ContainsKey("403"))
                operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

            if (!operation.Responses.ContainsKey("404"))
                operation.Responses.Add("404", new OpenApiResponse { Description = "Not Found" });

            if (!operation.Responses.ContainsKey("409"))
                operation.Responses.Add("409", new OpenApiResponse { Description = "Conflict" });

            if (!operation.Responses.ContainsKey("500"))
                operation.Responses.Add("500", new OpenApiResponse { Description = "Internal Server Error" });
        }
    }
}
