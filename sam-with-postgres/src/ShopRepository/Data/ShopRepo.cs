using System;
using Amazon.Lambda.Core;
using Npgsql;
using System.Collections.Generic;
using ShopRepository.Models;
using Dapper;


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
    public async Task<List<Product>> GetAllProducts()
    {
        var sql = "SELECT * FROM stock;"; // Make sure the table name and columns match your schema.
        try
        {
            await _conn.OpenAsync();
            var products = await _conn.QueryAsync<Product>(sql);
            return products.AsList();
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }

    public async Task<Product> GetProductById(int id)
    {
        var sql = "SELECT * FROM stock WHERE id = @Id;"; // Use parameterized queries to prevent SQL injection.
        try
        {
            await _conn.OpenAsync();
            var product = await _conn.QuerySingleOrDefaultAsync<Product>(sql, new { Id = id });
            return product;
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }

    public async Task AddProduct(Product product)
    {
        var sql = "INSERT INTO stock (name, quantity, manufacturer, price, base_cost) VALUES (@Name, @Quantity, @Manufacturer, @Price, @BaseCost);";
        try
        {
            await _conn.OpenAsync();
            await _conn.ExecuteAsync(sql, product);
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }

    public async Task UpdateProduct(Product product)
    {
        var sql = "UPDATE stock SET name = @Name, quantity = @Quantity, manufacturer = @Manufacturer, price = @Price, base_cost = @BaseCost WHERE id = @Id;";
        try
        {
            await _conn.OpenAsync();
            await _conn.ExecuteAsync(sql, product);
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }

    public async Task DeleteProduct(int id)
    {
        var sql = "DELETE FROM stock WHERE id = @Id;";
        try
        {
            await _conn.OpenAsync();
            await _conn.ExecuteAsync(sql, new { Id = id });
        }
        finally
        {
            await _conn.CloseAsync();
        }
    }
}
