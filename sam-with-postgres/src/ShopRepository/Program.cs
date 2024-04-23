using System.Text.Json;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using ShopRepository.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });

//TODO configure AWS keys properly. (APP WONT DEPLOY WITHOUT THIS WORKING!)
var clientConfig = new AmazonDynamoDBConfig();

// Configure local endpoint if we're running in SAM CLI
if (Environment.GetEnvironmentVariable("AWS_SAM_LOCAL") == "true")
    clientConfig.ServiceURL = "http://localhost:8000";
else
    clientConfig.RegionEndpoint = RegionEndpoint.APSoutheast2; //Sydney


builder.Services
    .AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(clientConfig))
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