using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Assignment_Example_HU.Exceptions;

namespace Assignment_Example_HU.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred.";
            var type = "Error";

            if (exception is BaseException baseEx)
            {
                statusCode = baseEx.StatusCode;
                message = baseEx.Message;
                type = baseEx.GetType().Name;
            }
            else
            {
                switch (exception)
                {
                    case UnauthorizedAccessException:
                        statusCode = HttpStatusCode.Unauthorized;
                        message = exception.Message;
                        type = "UnauthorizedError";
                        break;
                    case InvalidOperationException:
                        statusCode = HttpStatusCode.BadRequest;
                        message = exception.Message;
                        type = "BadRequestError";
                        break;
                    case ArgumentException:
                        statusCode = HttpStatusCode.BadRequest;
                        message = exception.Message;
                        type = "ArgumentError";
                        break;
                }
            }

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var response = new Assignment_Example_HU.DTOs.ErrorResponseDto
            {
                Success = false,
                Type = type,
                Message = message,
                StatusCode = (int)statusCode,
                Timestamp = DateTime.UtcNow
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }
}
