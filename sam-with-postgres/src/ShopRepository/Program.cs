using System.Text.Json;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using ShopRepository.Data;

var builder = WebApplication.CreateBuilder(args);

// Logger
builder.Logging
    .ClearProviders()
    .AddJsonConsole();

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });

//TODO configure AWS keys properly. (APP WONT DEPLOY WITHOUT THIS WORKING!)
var accessKeyId = Environment.GetEnvironmentVariable("ACCESS_KEY");
var secretKeyId = Environment.GetEnvironmentVariable("SECRET_KEY");
var dynamoConfig = Environment.GetEnvironmentVariable("AWS_SAM_LOCAL") == "true" ? new AmazonDynamoDBConfig { ServiceURL = Environment.GetEnvironmentVariable("LOCAL_DB"), UseHttp = true } : new AmazonDynamoDBConfig();


var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? RegionEndpoint.APSoutheast2.SystemName;
dynamoConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(region);
builder.Services
    .AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(dynamoConfig))
    .AddScoped<IDynamoDBContext, DynamoDBContext>()
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