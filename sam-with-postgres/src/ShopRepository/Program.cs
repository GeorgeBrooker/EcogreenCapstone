using System.Text.Json;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.Authentication;
using ShopRepository.Data;
using ShopRepository.Handler;
using ShopRepository.Helper;
using ShopRepository.Services;
using JsonSerializer = System.Text.Json.JsonSerializer;

var builder = WebApplication.CreateBuilder(args);

// Logger
builder.Logging
    .ClearProviders()
    .AddJsonConsole();

// Load config from AWS Secrets Manager
var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? RegionEndpoint.APSoutheast2.SystemName;
var secretClient = new AmazonSecretsManagerClient(RegionEndpoint.APSoutheast2);
var secretValue = secretClient.GetSecretValueAsync(new GetSecretValueRequest { SecretId = "KashishWebAppConfigSecrets" }).Result.SecretString;
if (secretValue != null)
{
    var secretJson = JsonSerializer.Deserialize<Dictionary<string, string>>(secretValue);
    foreach (var kvp in secretJson!) builder.Configuration[kvp.Key] = kvp.Value;
}

// Check for local environment and set up DynamoDB and Configuration accordingly
AmazonDynamoDBClient client;
var local = Environment.GetEnvironmentVariable("AWS_SAM_LOCAL") == "true";
if (local)
{
    Console.WriteLine("\nRUNNING WITH LOCAL DYNAMODB IN TEST MODE!\n");
    var dynamoConfig = new AmazonDynamoDBConfig
    {
        RegionEndpoint = RegionEndpoint.GetBySystemName(region),
        ServiceURL = Environment.GetEnvironmentVariable("LOCAL_DB"),
        AuthenticationRegion = "ap-southeast-2"
    };
    var creds = new SessionAWSCredentials("fake", "key", "fake");
    client = new AmazonDynamoDBClient(creds, dynamoConfig);
    
    builder.Configuration["Environment"] = "local";
}
else
{
    client = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(region));
    builder.Configuration["Environment"] = "prod";
}

// Configure HttpClient for NZ Post and Register the NZPost service
builder.Services.AddHttpClient("NZPostClient", client =>
{
    client.BaseAddress = new Uri("https://api.uat.nzpost.co.nz/"); //UAT environment
    client.DefaultRequestHeaders.Add("Accept", "application/json"); 
});
builder.Services.AddSingleton<NZPostService>();

// Add Authentication and Authorization policies
builder.Services.AddAuthentication("CustomCognitoAuth")
    .AddScheme<AuthenticationSchemeOptions, AuthHandler>("CustomCognitoAuth", null);

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CustomerOnly", policy =>
        policy.RequireAssertion(context =>
            {
                var groups = context.User.Claims.Where(c => c.Type == "cognito:groups").Select(c => c.Value).ToList();
                Console.WriteLine("\n\nMade it to the policy!\n\n");
                Console.WriteLine(groups.Contains("Customers"));

                return groups.Contains("Customers") || groups.Contains("Administrators");
            }
        ));

// Add cors policies 
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(",") ?? throw new Exception("AllowedOrigins not set in config");
Console.WriteLine("Configured Allowed Origins:");
foreach (var origin in allowedOrigins)
{
    Console.WriteLine(origin);
}
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policyBuilder =>
    {
        policyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Access-Control-Allow-Origin");
    });
    options.AddPolicy("AllowProdRequests", policyBuilder =>
    {
        policyBuilder
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Access-Control-Allow-Origin");
    });
});

// Add controllers to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });

// Add Services
builder.Services
    .AddSingleton<IAmazonDynamoDB>(client)
    .AddScoped<IDynamoDBContext, DynamoDBContext>()
    .AddScoped<IShopRepo, ShopRepo>()
    .AddScoped<CognitoService>()
    .AddScoped<AuthHandler>()
    .AddScoped<StockUploadHelper>()
    .AddScoped<StripeService>()
    .AddDefaultAWSOptions(builder.Configuration.GetAWSOptions())
    .AddAWSService<IAmazonCognitoIdentityProvider>()
    .AddAWSService<IAmazonS3>()
    .AddAWSLambdaHosting(LambdaEventSource.HttpApi); // Add AWS Lambda hosting support


// Build app and register the middleware
var app = builder.Build();
app.UseCors(local ? "AllowAllOrigins" : "AllowProdRequests"); // Set cors policy based on app environment
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "Welcome to running ASP.NET Core Minimal API on AWS Lambda");

app.Run();