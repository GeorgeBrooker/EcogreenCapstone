using System;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Microsoft.EntityFrameworkCore;
using ShopRepository.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ShopRepository.Data;

public class ShopRepo : IShopRepo
{
    private readonly IDynamoDBContext _dbContext;
    public ShopRepo(IDynamoDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    
    // ORDER METHODS
    public async Task<Order> GetOrderAsync(int orderId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        throw new NotImplementedException();
    }

    public async Task AddOrderAsync(Order order)
    {
        throw new NotImplementedException();
    }

    public Task UpdateOrderAsync(Order order)
    {
        throw new NotImplementedException();
    }

    public Task DeleteOrderAsync(int orderId)
    {
        throw new NotImplementedException();
    }
    
    // CUSTOEMR METHODS
    public Task<Customer> GetCustomerAsync(int customerId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        throw new NotImplementedException();
    }

    public Task AddCustomerAsync(Customer customer)
    {
        throw new NotImplementedException();
    }

    public Task UpdateCustomerAsync(Customer customer)
    {
        throw new NotImplementedException();
    }

    public Task DeleteCustomerAsync(int customerId)
    {
        throw new NotImplementedException();
    }

    // PHOTO METHODS
    public Task<Photo> GetPhotoAsync(int photoId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Photo>> GetAllPhotosAsync()
    {
        throw new NotImplementedException();
    }

    public Task AddPhotoAsync(Photo photo)
    {
        throw new NotImplementedException();
    }

    public Task UpdatePhotoAsync(Photo photo)
    {
        throw new NotImplementedException();
    }

    public Task DeletePhotoAsync(int photoId)
    {
        throw new NotImplementedException();
    }

    
    // STOCK METHODS
    public Task<Stock> GetStockAsync(int stockId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Stock>> GetAllStockAsync()
    {
        throw new NotImplementedException();
    }

    public Task AddStockAsync(Stock stock)
    {
        throw new NotImplementedException();
    }

    public Task UpdateStockAsync(Stock stock)
    {
        throw new NotImplementedException();
    }

    public Task DeleteStockAsync(int stockId)
    {
        throw new NotImplementedException();
    }
}
