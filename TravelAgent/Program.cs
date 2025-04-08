using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using TravelAgent.ServiceClients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ITravelAiClient, TravelAiClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//    app.MapScalarApiReference(); // Maps the Scalar UI endpoint
//}
app.MapOpenApi();
app.MapScalarApiReference(); // Maps the Scalar UI endpoint

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
