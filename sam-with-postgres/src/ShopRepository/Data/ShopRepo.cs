using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Identity;
using ShopRepository.Dtos;
using ShopRepository.Models;

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
    public async Task<Order?> GetOrder(Guid orderId)
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
    // TODO
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
    // TODO
    public async Task<IEnumerable<Order>?> GetAllOrders(int limit = 20)
    {
        try
        {
            if (limit <= 0)
                return new List<Order>();

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
    // TODO
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
    // TODO
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
    
    // TODO normalise how ID's are handled. {make application as independent from stripe as possible}
    public async Task<bool> DeleteOrder(string orderId)
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
    public async Task<IEnumerable<Customer>> GetAllCustomers(int limit = 20)
    {
        var result = new List<Customer>();
        
        try
        {
            if (limit <= 0)
                return result;

            var filter = new ScanFilter();
            filter.AddCondition("Id", ScanOperator.IsNotNull);
            var scanConfig = new ScanOperationConfig()
            {
                Limit = limit,
                Filter = filter
            };
            var queryResult = _dbContext.FromScanAsync<Customer>(scanConfig);

            do
                result.AddRange(await queryResult.GetNextSetAsync());
            while (!queryResult.IsDone && result.Count < limit);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Query Failed");
            return new List<Customer>();
        }

        return result;
    }
    public async Task<Customer?> GetCustomer(Guid id)
    {
        try
        {
            return await _dbContext.LoadAsync<Customer>(id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to find customer id={id} in database.");
            return null;
        }
    }
    public async Task<Customer?> GetCustomerFromStripe(string stripeId)
    {
        try
        {
            var customerIdSearch = _dbContext.FromQueryAsync<Customer>(
                new QueryOperationConfig
                {
                    IndexName = "CustomerStripeIndex",
                    Select = SelectValues.AllProjectedAttributes,
                    KeyExpression = new Expression
                    {
                        ExpressionStatement = "#stripe = :v_stripe",
                        ExpressionAttributeNames = new Dictionary<string, string> { {"#stripe", "StripeId"} },
                        ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry> { {":v_stripe", new Primitive {Value = stripeId} } }
                    }
                });
            var projectedCustomer = await customerIdSearch.GetRemainingAsync();
            return await _dbContext.LoadAsync<Customer>(projectedCustomer.FirstOrDefault()!.Id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to find customer StripeId={stripeId} in database.");
            return null;
        }
    }
    public async Task<Customer?> GetCustomerFromEmail(string email)
    {
        try
        {
            var customerIdSearch = _dbContext.FromQueryAsync<Customer>(
                // Run Query on GSI table containing customer emails as partition key.
                new QueryOperationConfig
                {
                    IndexName = "CustomerEmailIndex",
                    Select = SelectValues.AllProjectedAttributes,
                    KeyExpression = new Expression
                    {
                        ExpressionStatement = "#email = :v_email",
                        ExpressionAttributeNames = new Dictionary<string, string> { { "#email", "Email" } },
                        ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry> { { ":v_email", new Primitive {Value = email} } }
                    }
                });
            var projectedCustomer = await customerIdSearch.GetRemainingAsync(); // Customer object that contains only ID & Email
            return await _dbContext.LoadAsync<Customer>(projectedCustomer.FirstOrDefault()!.Id); // Find full customer via Id of projected customer
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to find customer with email={email} in database.");
            return null;
        }
    }
    public async Task<IEnumerable<Order>?> GetCustomerOrders(Guid id)
    {
        try
        {
            // GSI's are not always consistent all the time. We should be careful how we use results returned from GSI queries.
            var orderSearch = _dbContext.FromQueryAsync<Order>(
                new QueryOperationConfig
                {
                    IndexName = "OrderCustomerIndex",
                    Select = SelectValues.AllProjectedAttributes,
                    KeyExpression = new Expression
                    {
                        ExpressionStatement = "#customer = :v_customer",
                        ExpressionAttributeNames = new Dictionary<string, string>{ { "#customer", "CustomerId" } },
                        ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry> { { ":v_customer", new Primitive{Value = id.ToString()} } }
                    }
                });
            
            var projectedOrders = new List<Order>();
            do
                projectedOrders.AddRange(await orderSearch.GetNextSetAsync());
            while (!orderSearch.IsDone);
            
            var orders = new List<Order>();
            foreach (var o in projectedOrders)
                orders.Add(await _dbContext.LoadAsync<Order>(o.Id));
            
            return orders;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Order lookup by CustomerId failed");
            return null;
        }
    }
    public async Task<bool> AddCustomer(CustomerInput cInput)
    {
        try
        {
            if (await GetCustomerFromEmail(cInput.Email) != null)
                throw new Exception($"An account with email={cInput.Email} already exists.");
            
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = cInput.Fname,
                LastName = cInput.Lname,
                Email = cInput.Email,
            };
            customer.Password = new PasswordHasher<Customer>().HashPassword(customer, cInput.Pass);
        
            await _dbContext.SaveAsync(customer);
            _logger.LogInformation($"Customer has been added");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to add customer to the database");
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
            _logger.LogInformation("Customer was updated");
        }
        catch (Exception e){
            _logger.LogError(e, "Failed to update customer");
            return false;
        }

        return true;
    }
    public async Task<bool> DeleteCustomer(Guid customerId)
    {
        bool result;
        
        try
        {
            await _dbContext.DeleteAsync<Customer>(customerId);
            // Check for delete success
            var ghost = await _dbContext.LoadAsync<Customer>(customerId, new DynamoDBOperationConfig { ConsistentRead = true });
            result = ghost == null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete customer");
            result = false;
        }
        
        if (result) _logger.LogInformation("Book successfully deleted");

        return result;
    }

    // STOCK METHODS
    // TODO
    public async Task<Stock?> GetStockFromStripe(string stockId)
    {
        try
        {
            return await _dbContext.LoadAsync<Stock>(stockId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to find stock in database.");
            return null;
        }
    }
    
    // TODO
    public async Task<Stock?> GetStock(Guid id)
    {
        throw new NotImplementedException();
    }
    // TODO
    public async Task<IEnumerable<Stock>?> GetAllStock(int limit)
    {
        try
        {
            if (limit <= 0)
                return new List<Stock>();

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
    
    // TODO update this to work with a DTO.
    public async Task<bool> AddStock(Stock stock)
    {
        try
        {
            if (stock.Id == Guid.Empty) throw new ArgumentException(); 
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
    
    // TODO this should probably work off the standard id and not the stripe id.
    public async Task<bool> DeleteStock(string stockId)
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
    
    // StockRequest METHODS
    // TODO Actually create these.
}