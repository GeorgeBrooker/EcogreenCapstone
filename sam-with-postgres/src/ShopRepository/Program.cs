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
var secretClient = new AmazonSecretsManagerClient(RegionEndpoint.APSoutheast2);
var secretValue = secretClient.GetSecretValueAsync(new GetSecretValueRequest { SecretId = "KashishWebAppConfigSecrets" }).Result.SecretString;
if (secretValue != null)
{
    var secretJson = JsonSerializer.Deserialize<Dictionary<string, string>>(secretValue);
    foreach (var kvp in secretJson!) builder.Configuration[kvp.Key] = kvp.Value;
}

// Local dev env config
var local = Environment.GetEnvironmentVariable("AWS_SAM_LOCAL") == "true";
var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? RegionEndpoint.APSoutheast2.SystemName;
var dynamoConfig = new AmazonDynamoDBConfig { RegionEndpoint = RegionEndpoint.GetBySystemName(region) };
AmazonDynamoDBClient client;
if (local)
{
    Console.WriteLine("\nRUNNING WITH LOCAL DYNAMODB IN TEST MODE!\n");
    dynamoConfig.ServiceURL = Environment.GetEnvironmentVariable("LOCAL_DB");
    dynamoConfig.AuthenticationRegion = "ap-southeast-2";
    var creds = new SessionAWSCredentials("fake", "key", "fake");
    client = new AmazonDynamoDBClient(creds, dynamoConfig);
    
    builder.Configuration["Environment"] = "local";
}
else
    client = new AmazonDynamoDBClient(dynamoConfig);


// Add AWS services to dependency injection
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services
    .AddAWSService<IAmazonCognitoIdentityProvider>()
    .AddAWSService<IAmazonS3>();


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
    .AddScoped<StripeService>();

// Add AWS Lambda support. When running the application as an AWS Serverless application, Kestrel is replaced
// with a Lambda function contained in the Amazon.Lambda.AspNetCoreServer package, which marshals the request into the ASP.NET Core hosting framework.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// CORS POLICY FOR LOCAL TESTING. IN PROD CORS IS CONFIGURED THROUGH API GATEWAY
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

var app = builder.Build();

// Only allow CORS for local testing
if (local)
    app.UseCors("AllowAllOrigins");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "Welcome to running ASP.NET Core Minimal API on AWS Lambda");

app.Run();