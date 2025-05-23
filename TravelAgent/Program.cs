using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Scalar.AspNetCore;
using System.Text;
using TravelAgent.Data;
using TravelAgent.ServiceClients;
using TravelAgent.ServiceClients.Interfaces;
using TravelAgent.Services;
using TravelAgent.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register health checks
builder.Services.AddHealthChecks();

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddCors(options =>
{

    options.AddPolicy("AllowUIAndLocalhost", policy =>
    {
        policy.WithOrigins(
                "https://brave-bay-05bdb560f.6.azurestaticapps.net",
                "http://localhost:5050"
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });

    // options.AddPolicy("AllowAll", policy =>
    // {
    //     // policy.AllowAnyOrigin()
    //     //       .AllowAnyMethod()
    //     //       .AllowAnyHeader();
    // });
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
builder.Services.AddSingleton<ISharePointAuthProvider, SharePointAuthProvider>();
builder.Services.AddScoped<ISharepointGraphServiceClinet, SharepointGraphServiceClinet>();
builder.Services.AddScoped<ISharepointGraphApiClient, SharepointGraphApiClient>();
//builder.Services.AddScoped<ISharePointRestApiClient, SharePointRestApiClient>();

builder.Services.AddOpenApi();

// Adds Jitters for Retry.
var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(5), retryCount: 3);

// Define the retry policy with exponential backoff
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(delay);

builder.Services.AddHttpClient("AIClient")
    .AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient("SPGraphAPIClient", (serviceProvider, client) =>
{
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddPolicyHandler(retryPolicy);


var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(); // Maps the Scalar UI endpoint

app.MapGet("/", () => "Welcome to Travel Agent AI API");

// Map health check endpoint
app.MapHealthChecks("/health");

app.UseHttpsRedirection();

//app.UseCors("AllowAll");
app.UseCors("AllowUIAndLocalhost");

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


