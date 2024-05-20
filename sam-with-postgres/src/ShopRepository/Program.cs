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

    // List of SAFE local secret examples. These are not sensitive and can be stored in the code.
    var localSecrets = new Dictionary<string, string>
    {
        { "Cognito:UserPoolId", "ap-southeast-2_RXmB1ATp1" },
        { "Cognito:ClientId", "d06f48kk6k9kcp491mlkskkta" },
        { "Cognito:Region", "ap-southeast-2" },
        { "Cognito:ClientSecret", "189anlvjvs2hrjeantnv5dbiol5upt456n3toes8mv7lcu00andv" },

        { "Stripe:SecretKey", "your-local-stripe-key" },
        { "Stripe:PublishableKey", "your-local-stripe-pub-key" },

        { "StockUploadBucket", "kashish-web-asset-bucket" },

        { "Environment", "local" }
    };
    foreach (var kvp in localSecrets) builder.Configuration[kvp.Key] = kvp.Value;
}
else
{
    client = new AmazonDynamoDBClient(dynamoConfig);
    // Use secrets manager to get the API keys and other sensitive config data.
    // We will have one secret containing all keys as KVPs for this app.
    var secrets = new AmazonSecretsManagerClient(RegionEndpoint.APSoutheast2);
    var response =
        await secrets.GetSecretValueAsync(new GetSecretValueRequest { SecretId = "KashishWebbAppSecretARN" });
    var secretValue = response.SecretString;
    if (secretValue != null)
    {
        // Parse the secret value as JSON and add it to the configuration
        var secretJson = JsonSerializer.Deserialize<Dictionary<string, string>>(secretValue);
        foreach (var kvp in secretJson!) builder.Configuration[kvp.Key] = kvp.Value;
    }
}

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

                return groups != null && (groups.Contains("Customers") || groups.Contains("Administrators"));
            }
        ));

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });


// Add DynamoDB Context and Repositories
builder.Services
    .AddSingleton<IAmazonDynamoDB>(client)
    .AddScoped<IDynamoDBContext, DynamoDBContext>()
    .AddScoped<IShopRepo, ShopRepo>()
    .AddScoped<CognitoService>()
    .AddScoped<AuthHandler>()
    .AddScoped<StockUploadHelper>();

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