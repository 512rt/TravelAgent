using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text;
using TravelAgent.Services;
using TravelAgent.ServiceClients;
using System;
using TravelAgent.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Polly.Extensions.Http;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register health checks
builder.Services.AddHealthChecks();

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();

        //  policy.WithOrigins("https://yourfrontend.com")
        //      .AllowAnyMethod()
        //      .AllowAnyHeader();
    });
});


builder.Services.AddDbContext<TravelAgentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Add authorization
builder.Services.AddAuthorization();

// Add services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITravelAiClient, TravelAiClient>();

builder.Services.AddOpenApi();

// Define the retry policy with exponential backoff
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2, 4, 8 seconds
        onRetry: (outcome, timespan, retryAttempt, context) =>
        {
            Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds due to: {outcome.Exception?.Message}");
        });

builder.Services.AddHttpClient("AIClient")
    .AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(); // Maps the Scalar UI endpoint

app.MapGet("/", () => "Welcome to Travel Agent AI API");

// Map health check endpoint
app.MapHealthChecks("/health");

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
