using Microsoft.AspNetCore.Builder;
using Assignment_Example_HU.Middleware;

namespace Assignment_Example_HU.Extensions
{
    public static class ExceptionHandlingExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}

