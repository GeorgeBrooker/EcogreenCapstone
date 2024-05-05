using System.Text.Json;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Microsoft.AspNetCore.Authentication;
using ShopRepository.Data;
using ShopRepository.Handler;

var builder = WebApplication.CreateBuilder(args);

// Logger
builder.Logging
    .ClearProviders()
    .AddJsonConsole();

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });

// Add Authentication
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, AuthHandler>("BasicAuthentication", null);

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CustomerOnly", policy => 
        policy.RequireClaim("customer"));

var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? RegionEndpoint.APSoutheast2.SystemName;

var dynamoConfig = new AmazonDynamoDBConfig { RegionEndpoint = RegionEndpoint.GetBySystemName(region) };
AmazonDynamoDBClient client;

if (Environment.GetEnvironmentVariable("AWS_SAM_LOCAL") == "true")
{
    Console.WriteLine("\nRUNNING WITH LOCAL DYNAMODB IN TEST MODE!\n");
    dynamoConfig.ServiceURL = Environment.GetEnvironmentVariable("LOCAL_DB");
    dynamoConfig.AuthenticationRegion = "ap-southeast-2";
    var creds = new SessionAWSCredentials("fake", "key", "Fake");
    client = new AmazonDynamoDBClient(creds, dynamoConfig);
    
    // CORS CONFIG FOR LOCAL TESTING. IN PROD CORS IS CONFIGURED THROUGH API GATEWAY.
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins", policyBuilder =>
        {
            policyBuilder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });
}
else
{
    client = new AmazonDynamoDBClient(dynamoConfig);
}

builder.Services
    .AddSingleton<IAmazonDynamoDB>(client)
    .AddScoped<IDynamoDBContext, DynamoDBContext>()
    .AddScoped<IShopRepo, ShopRepo>();


// Add AWS Lambda support. When running the application as an AWS Serverless application, Kestrel is replaced
// with a Lambda function contained in the Amazon.Lambda.AspNetCoreServer package, which marshals the request into the ASP.NET Core hosting framework.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);


var app = builder.Build();

app.UseHttpsRedirection();
//app.UseAuthorization();
app.MapControllers();
app.UseCors("AllowAllOrigins");
app.MapGet("/", () => "Welcome to running ASP.NET Core Minimal API on AWS Lambda");

app.Run();