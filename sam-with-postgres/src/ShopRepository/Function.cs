using System;
using System.Text.Json;
using Amazon;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;// Add Npgsql package for PostgreSQL
using ShopRepository.Data; 

var builder = WebApplication.CreateBuilder(args);
 
// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

string userName = Environment.GetEnvironmentVariable("USER_NAME");
string password = Environment.GetEnvironmentVariable("PASSWORD");
string rdsProxyHost = Environment.GetEnvironmentVariable("RDS_PROXY_HOST");
string dbName = Environment.GetEnvironmentVariable("DB_NAME");
string connectionString = $"Server={rdsProxyHost};Username={userName};Password={password}";

builder.Services
    // Configure EF to use PostgreSQL
    .AddDbContext<AWSDbContext>(options => { options.UseNpgsql(connectionString); })
    .AddScoped<IShopRepo, ShopRepo>();




// Add AWS Lambda support. When running the application as an AWS Serverless application, Kestrel is replaced
// with a Lambda function contained in the Amazon.Lambda.AspNetCoreServer package, which marshals the request into the ASP.NET Core hosting framework.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);


var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Welcome to running ASP.NET Core Minimal API on AWS Lambda");

app.Run();