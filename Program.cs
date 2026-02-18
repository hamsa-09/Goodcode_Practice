using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Assignment_Example_HU.Configurations.Validation;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// Explicit validator registrations (clear for beginners)
builder.Services.AddTransient<IValidator<RegisterRequestDto>, RegisterRequestValidator>();
builder.Services.AddTransient<IValidator<LoginRequestDto>, LoginRequestValidator>();
builder.Services.AddTransient<IValidator<CreateVenueDto>, CreateVenueDtoValidator>();
builder.Services.AddTransient<IValidator<CreateCourtDto>, CreateCourtDtoValidator>();
builder.Services.AddTransient<IValidator<CreateDiscountDto>, CreateDiscountDtoValidator>();
builder.Services.AddTransient<IValidator<CreateGameDto>, CreateGameDtoValidator>();

// Custom layers
builder.Services.AddPersistence(configuration);
builder.Services.AddApplicationServices(configuration);
builder.Services.AddJwtAuthentication(configuration);
builder.Services.AddSwaggerWithJwt();

builder.Services.AddAuthorization();
// Configure Kestrel explicitly to avoid implicit/duplicate bindings (ASPNETCORE_URLS / launchSettings)
// This prevents "address already in use" errors caused by multiple bindings to the same port.
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTPS
    options.ListenLocalhost(58229, listenOptions => listenOptions.UseHttps());

    // HTTP
    options.ListenLocalhost(58230);
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Assignment Example HU API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();

// Initialize Database (Auto-create schema if not exists - handy for offline/dev)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<Assignment_Example_HU.Data.AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
