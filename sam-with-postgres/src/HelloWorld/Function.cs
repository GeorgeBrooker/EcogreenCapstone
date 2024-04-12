using System;
using Npgsql;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.EntityFrameworkCore.Query;
using JsonSerializer = System.Text.Json.JsonSerializer;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HelloWorld;

public class Function
{

    NpgsqlConnection conn = null;
    
    string userName = Environment.GetEnvironmentVariable("USER_NAME");
    string password = Environment.GetEnvironmentVariable("PASSWORD");
    string rdsProxyHost = Environment.GetEnvironmentVariable("RDS_PROXY_HOST");
    string dbName = Environment.GetEnvironmentVariable("DB_NAME");
    
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {
        string connString = $"Server={rdsProxyHost};Username={userName};Password={password}";
        Console.WriteLine(connString);
        var conn = new NpgsqlConnection(connString);
        conn.Open();

        return new APIGatewayProxyResponse
        {
            Body = JsonSerializer.Serialize("Jobs done"),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}

