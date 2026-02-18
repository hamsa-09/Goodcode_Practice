using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.Middleware;
using Assignment_Example_HU.Exceptions;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Tests.Middleware
{
    public class ExceptionHandlingMiddlewareTests
    {
        private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;
        private readonly DefaultHttpContext _context;

        public ExceptionHandlingMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
            _context = new DefaultHttpContext();
            _context.Response.Body = new MemoryStream();
        }

        [Fact]
        public async Task InvokeAsync_CallsNext_WhenNoException()
        {
            // Arrange
            var nextCalled = false;
            RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };
            var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeAsync_HandlesNotFoundException()
        {
            // Arrange
            RequestDelegate next = (ctx) => throw new NotFoundException("Test Not Found");
            var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            var response = await ReadResponse<ErrorResponseDto>(_context);
            response.Type.Should().Be("NotFoundException");
            response.Message.Should().Be("Test Not Found");
        }

        [Fact]
        public async Task InvokeAsync_HandlesUnauthorizedAccessException()
        {
            // Arrange
            RequestDelegate next = (ctx) => throw new UnauthorizedAccessException("Test Access Denied");
            var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
            var response = await ReadResponse<ErrorResponseDto>(_context);
            response.Type.Should().Be("UnauthorizedError");
        }

        [Fact]
        public async Task InvokeAsync_HandlesGenericException()
        {
            // Arrange
            RequestDelegate next = (ctx) => throw new Exception("Kill me now");
            var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        private async Task<T> ReadResponse<T>(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var json = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
        }
    }
}
