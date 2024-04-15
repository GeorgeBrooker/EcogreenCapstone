using System;
using Amazon.Lambda.Core;
using Npgsql;
using System.Collections.Generic;
using ShopRepository.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ShopRepository.Data;

public class ShopRepo
{
    // Connection string is injected through env variables when using AWS lambda pattern.
    static string userName = Environment.GetEnvironmentVariable("USER_NAME");
    static string password = Environment.GetEnvironmentVariable("PASSWORD");
    static string rdsProxyHost = Environment.GetEnvironmentVariable("RDS_PROXY_HOST");
    static string dbName = Environment.GetEnvironmentVariable("DB_NAME");
    private static readonly string connectionString = $"Server={rdsProxyHost};Username={userName};Password={password}";
    
    // I think having the connection as static makes sure there's only one floating around?
    private static NpgsqlConnection _conn;

    static ShopRepo()
    {
        // Initialize the database connection using connection pooling
        var connStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            // Specify additional connection pooling options here if needed.
            Pooling = true,
            MinPoolSize = 0,
            MaxPoolSize = 100,
            ConnectionIdleLifetime = 300,
            ConnectionPruningInterval = 10
        };
        _conn = new NpgsqlConnection(connStringBuilder.ConnectionString);
    }
    
    //todo
    public List<Product> GetAllProducts()
    {
        try
        {
            _conn.OpenAsync();

        }
        finally
        {
            _conn.Close();
        }

        return new List<Product>();
    }
    
    //todo
    public Product GetProductById(int id)
    {

        return new Product();
    }
    
    //todo
    public void AddProduct(Product product)
    {

    }
    
    //todo
    public void UpdateProduct(Product product)
    {

    }
    
    //todo
    public void DeleteProduct(int id)
    {

    }
}
