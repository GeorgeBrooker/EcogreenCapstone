using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using ShopRepository.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace ShopRepository.Data;

public class ShopRepo : IShopRepo
{
    private readonly IDynamoDBContext _dbContext;
    private readonly ILogger<ShopRepo> _logger;
    public ShopRepo(IDynamoDBContext dbContext, ILogger<ShopRepo> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }


    // ORDER METHODS
    public async Task<Order?> GetOrder(int orderId)
    {
        try
        {
            return await _dbContext.LoadAsync<Order>(orderId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to find order {orderId} in database.");
            return null;
        }
    }

    public async Task<Order?> GetOrderFromPaymentId(string paymentIntentId)
    {
        try
        {
            var orderSearch = _dbContext.QueryAsync<Order>("PaymentIntentID", QueryOperator.Equal, [paymentIntentId]);
            var order = await orderSearch.GetRemainingAsync();
            return order[0];
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to find order with payment id {paymentIntentId}");
            return null;
        }
    }

    public async Task<IEnumerable<Order>?> GetAllOrders(int limit = 20)
    {
        try
        {
            if (limit <= 0)
                return new List<Order>();;

            var filter = new ScanFilter();
            filter.AddCondition("Id", ScanOperator.IsNotNull);
            var scanConfig = new ScanOperationConfig()
            {
                Limit = limit,
                Filter = filter
            };

            return await _dbContext.FromScanAsync<Order>(scanConfig).GetRemainingAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Query Failed");
            return null;
        }
    }

    public async Task<bool> AddOrder(Order order)
    {
        try
        {
            order.Id = Guid.NewGuid();
            await _dbContext.SaveAsync(order);
            _logger.LogInformation($"Order {order} has been added");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to add {order} to database");
            return false;
        }

        return true;
    }

    public async Task<bool> UpdateOrder(Order? order)
    {
        if (order == null) return false;

        try
        {
            await _dbContext.SaveAsync(order);
            _logger.LogInformation($"Order {order} was updated");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "failed to update order");
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteOrder(int orderId)
    {
        bool result;
        
        try
        {
            // Delete
            await _dbContext.DeleteAsync<Order>(orderId);
            // Check for delete success
            var config = new DynamoDBContextConfig { ConsistentRead = true };
            Order ghost = await _dbContext.LoadAsync<Order>(orderId, config);
            result = ghost == null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to delete order id={orderId}");
            result = false;
        }
        
        if (result) _logger.LogInformation("Order successfully deleted");

        return result;
    }
    
    // CUSTOMER METHODS
    public async Task<Customer?> GetCustomer(string customerId)
    {
        try
        {
            return await _dbContext.LoadAsync<Customer>(customerId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to find customer {customerId} in database.");
            return null;
        }
    }

    public async Task<IEnumerable<Order>?> GetCustomerOrders(string customerId)
    {
        try
        {
            var orderSearch = _dbContext.QueryAsync<Order>("CustomerID", QueryOperator.Equal, [customerId]);
            var order = await orderSearch.GetRemainingAsync();
            return order;
        }
        catch (Exception e)
        {
            _logger.LogError($"Order lookup by customerId={customerId} failed");
            return null;
        }
    }

    public async Task<IEnumerable<Customer>?> GetAllCustomers(int limit = 20)
    {
        try
        {
            if (limit <= 0)
                return new List<Customer>();;

            var filter = new ScanFilter();
            filter.AddCondition("Id", ScanOperator.IsNotNull);
            var scanConfig = new ScanOperationConfig()
            {
                Limit = limit,
                Filter = filter
            };

            return await _dbContext.FromScanAsync<Customer>(scanConfig).GetRemainingAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Query Failed");
            return null;
        }
    }

    public async Task<bool> AddCustomer(Customer customer)
    {
        try
        {
            if (customer.Id != null)
            {
                await _dbContext.SaveAsync(customer);
                _logger.LogInformation($"Customer {customer} has been added");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to add {customer} to the database");
            return false;
        }

        return true;
    }

    public async Task<bool> UpdateCustomer(Customer? customer)
    {
        if (customer == null) return false;

        try
        {
            await _dbContext.SaveAsync(customer);
            _logger.LogInformation($"Order {customer} was updated");
        }
        catch (Exception e){
            _logger.LogError(e, "failed to update customer");
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteCustomer(int customerId)
    {
        bool result;
        
        try
        {
            // Delete
            await _dbContext.DeleteAsync<Customer>(customerId);
            // Check for delete success
            var config = new DynamoDBContextConfig { ConsistentRead = true };
            Customer ghost = await _dbContext.LoadAsync<Customer>(customerId, config);
            result = ghost == null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to delete customer id={customerId}");
            result = false;
        }
        
        if (result) _logger.LogInformation("Book successfully deleted");

        return result;
    }

    // STOCK METHODS
    public async Task<Stock?> GetStock(string stockId)
    {
        try
        {
            return await _dbContext.LoadAsync<Stock>(stockId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to find stock: id={stockId} in database.");
            return null;
        }
    }

    public async Task<IEnumerable<Stock>?> GetAllStock(int limit)
    {
        try
        {
            if (limit <= 0)
                return new List<Stock>();;

            var filter = new ScanFilter();
            filter.AddCondition("Id", ScanOperator.IsNotNull);
            var scanConfig = new ScanOperationConfig()
            {
                Limit = limit,
                Filter = filter
            };

            return await _dbContext.FromScanAsync<Stock>(scanConfig).GetRemainingAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Query Failed");
            return null;
        }
    }

    public async Task<bool> AddStock(Stock stock)
    {
        try
        {
            if (stock.Id == null) throw new ArgumentException(); 
            await _dbContext.SaveAsync(stock);
            _logger.LogInformation($"Stock: {stock} has been added");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to add stock: {stock} to database");
            return false;
        }

        return true;
    }

    public async Task<bool> UpdateStock(Stock? stock)
    {
        if (stock == null) return false;

        try
        {
            await _dbContext.SaveAsync(stock);
            _logger.LogInformation($"Stock: {stock} was updated");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "failed to update order");
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteStock(int stockId)
    {
        bool result;
        
        try
        {
            // Delete
            await _dbContext.DeleteAsync<Stock>(stockId);
            // Check for delete success
            var config = new DynamoDBContextConfig { ConsistentRead = true };
            Stock ghost = await _dbContext.LoadAsync<Stock>(stockId, config);
            result = ghost == null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to delete stock: id={stockId}");
            result = false;
        }
        
        if (result) _logger.LogInformation("Order successfully deleted");

        return result;
    }
}