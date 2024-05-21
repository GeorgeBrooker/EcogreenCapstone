using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Identity;
using ShopRepository.Dtos;
using ShopRepository.Models;

namespace ShopRepository.Data;

public class ShopRepo(IDynamoDBContext dbContext, ILogger<ShopRepo> logger) : IShopRepo
{
//
// ORDER METHODS
//
    public async Task<IEnumerable<Order>> GetAllOrders(int limit = 20)
    {
        var result = new List<Order>();

        try
        {
            if (limit <= 0)
                return result;

            var filter = new ScanFilter();
            filter.AddCondition("Id", ScanOperator.IsNotNull);
            var scanConfig = new ScanOperationConfig
            {
                Limit = limit,
                Filter = filter
            };
            var queryResult = dbContext.FromScanAsync<Order>(scanConfig);

            do
            {
                result.AddRange(await queryResult.GetNextSetAsync());
            } while (!queryResult.IsDone && result.Count < limit);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to find any orders");
            return new List<Order>();
        }

        return result;
    }

    public async Task<IEnumerable<StockRequest>> GetOrderStock(Guid orderId)
    {
        var orderStock = new List<StockRequest>();
        try
        {
            var stockQuery = dbContext.QueryAsync<StockRequest>(orderId);
            do
            {
                orderStock.AddRange(await stockQuery.GetNextSetAsync());
            } while (!stockQuery.IsDone);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get component stock of order.");
            return new List<StockRequest>();
        }

        return orderStock;
    }

    public async Task<Order?> GetOrder(Guid orderId)
    {
        try
        {
            return await dbContext.LoadAsync<Order>(orderId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to find order in database.");
            return null;
        }
    }

    public async Task<Order?> GetOrderFromPaymentId(string paymentIntentId)
    {
        try
        {
            var paymentIdSearch = dbContext.FromQueryAsync<Order>(
                new QueryOperationConfig
                {
                    IndexName = "OrderPaymentIndex",
                    Select = SelectValues.AllProjectedAttributes,
                    KeyExpression = new Expression
                    {
                        ExpressionStatement = "#stripe = :v_stripe",
                        ExpressionAttributeNames = new Dictionary<string, string> { { "#stripe", "PaymentIntentId" } },
                        ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                            { { ":v_stripe", new Primitive { Value = paymentIntentId } } }
                    }
                });

            var projectedOrder = await paymentIdSearch.GetRemainingAsync();
            return await dbContext.LoadAsync<Order>(projectedOrder.FirstOrDefault()!.Id);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to find order from paymentId.");
            return null;
        }
    }

    public async Task<Guid?> AddOrder(OrderInput nOrder)
    {
        try
        {
            if (await GetOrderFromPaymentId(nOrder.PaymentId) != null)
                throw new Exception($"An order with paymentId={nOrder.PaymentId} already exists.");

            var order = new Order
            {
                Id = Guid.NewGuid(),
                PaymentIntentId = nOrder.PaymentId,
                CustomerId = nOrder.CustomerId,
                DeliveryLabelUid = nOrder.DeliveryLabel,
                TrackingNumber = nOrder.Tracking,
                PackageReference = nOrder.PackageRef,
                CustomerAddress = nOrder.CustomerAddress,
                OrderStatus = nOrder.OrderStatus,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.SaveAsync(order);
            logger.LogInformation("Order added");
            return order.Id;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to add order to database");
            return null;
        }
    }

    public async Task<bool> UpdateOrder(Order? order)
    {
        if (order == null) return false;

        try
        {
            await dbContext.SaveAsync(order);
            logger.LogInformation("Order was updated.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update order.");
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteOrder(Guid orderId)
    {
        bool result;

        try
        {
            // Delete
            await dbContext.DeleteAsync<Order>(orderId);
            // Check for delete success
            var ghost = await dbContext.LoadAsync<Order>(orderId,
                new DynamoDBOperationConfig { ConsistentRead = true });
            result = ghost == null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to delete order");
            result = false;
        }

        if (result) logger.LogInformation("Order successfully deleted");
        return result;
    }

//
// CUSTOMER METHODS
// 
    public async Task<IEnumerable<Customer>> GetAllCustomers(int limit = 20)
    {
        var result = new List<Customer>();

        try
        {
            if (limit <= 0)
                return result;

            var filter = new ScanFilter();
            filter.AddCondition("Id", ScanOperator.IsNotNull);
            var scanConfig = new ScanOperationConfig
            {
                Limit = limit,
                Filter = filter
            };
            var queryResult = dbContext.FromScanAsync<Customer>(scanConfig);

            do
            {
                result.AddRange(await queryResult.GetNextSetAsync());
            } while (!queryResult.IsDone && result.Count < limit);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to find any Customers");
            return new List<Customer>();
        }

        return result;
    }

    public async Task<Customer?> GetCustomer(Guid id)
    {
        try
        {
            return await dbContext.LoadAsync<Customer>(id);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to find customer in database.");
            return null;
        }
    }

    public async Task<Customer?> GetCustomerFromStripe(string stripeId)
    {
        try
        {
            var customerIdSearch = dbContext.FromQueryAsync<Customer>(
                new QueryOperationConfig
                {
                    IndexName = "CustomerStripeIndex",
                    Select = SelectValues.AllProjectedAttributes,
                    KeyExpression = new Expression
                    {
                        ExpressionStatement = "#stripe = :v_stripe",
                        ExpressionAttributeNames = new Dictionary<string, string> { { "#stripe", "StripeId" } },
                        ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                            { { ":v_stripe", new Primitive { Value = stripeId } } }
                    }
                });

            var projectedCustomer = await customerIdSearch.GetRemainingAsync();
            return await dbContext.LoadAsync<Customer>(projectedCustomer.FirstOrDefault()!.Id);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to find customer from stripeId.");
            return null;
        }
    }

    public async Task<Customer?> GetCustomerFromEmail(string email)
    {
        try
        {
            var customerIdSearch = dbContext.FromQueryAsync<Customer>(
                // Run Query on GSI table containing customer emails as partition key.
                new QueryOperationConfig
                {
                    IndexName = "CustomerEmailIndex",
                    Select = SelectValues.AllProjectedAttributes,
                    KeyExpression = new Expression
                    {
                        ExpressionStatement = "#email = :v_email",
                        ExpressionAttributeNames = new Dictionary<string, string> { { "#email", "Email" } },
                        ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                            { { ":v_email", new Primitive { Value = email } } }
                    }
                });
            var projectedCustomer =
                await customerIdSearch.GetRemainingAsync(); // Customer object that contains only ID & Email
            return await dbContext.LoadAsync<Customer>(projectedCustomer.FirstOrDefault()!
                .Id); // Find full customer via Id of projected customer
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to find customer with email={email} in database.");
            return null;
        }
    }

    public async Task<Customer?> ValidLogin(string email, string password)
    {
        var customer = await GetCustomerFromEmail(email);
        var pwHasher = new PasswordHasher<Customer>();

        if (customer == null || pwHasher.VerifyHashedPassword(customer, customer.Password, password) ==
            PasswordVerificationResult.Failed)
            return null;

        return customer;
    }

    public async Task<IEnumerable<Order>?> GetCustomerOrders(Guid id)
    {
        try
        {
            // GSI's are not always consistent all the time. We should be careful how we use results returned from GSI queries.
            var orderSearch = dbContext.FromQueryAsync<Order>(
                new QueryOperationConfig
                {
                    IndexName = "OrderCustomerIndex",
                    Select = SelectValues.AllProjectedAttributes,
                    KeyExpression = new Expression
                    {
                        ExpressionStatement = "#customer = :v_customer",
                        ExpressionAttributeNames = new Dictionary<string, string> { { "#customer", "CustomerId" } },
                        ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                            { { ":v_customer", new Primitive { Value = id.ToString() } } }
                    }
                });

            var projectedOrders = new List<Order>();
            do
            {
                projectedOrders.AddRange(await orderSearch.GetNextSetAsync());
            } while (!orderSearch.IsDone);

            var orders = new List<Order>();
            foreach (var o in projectedOrders)
                orders.Add(await dbContext.LoadAsync<Order>(o.Id));

            return orders;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Order lookup by CustomerId failed");
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
                Password = ""
            };
            customer.Password = new PasswordHasher<Customer>().HashPassword(customer, cInput.Pass);

            await dbContext.SaveAsync(customer);
            logger.LogInformation("Customer has been added");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to add customer to the database");
            return false;
        }

        return true;
    }

    public async Task<bool> UpdateCustomer(Customer? customer)
    {
        if (customer == null) return false;

        try
        {
            await dbContext.SaveAsync(customer);
            logger.LogInformation("Customer was updated");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update customer");
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteCustomer(Guid customerId)
    {
        bool result;

        try
        {
            await dbContext.DeleteAsync<Customer>(customerId);
            // Check for delete success
            var ghost = await dbContext.LoadAsync<Customer>(customerId,
                new DynamoDBOperationConfig { ConsistentRead = true });
            result = ghost == null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to delete customer");
            result = false;
        }

        if (result) logger.LogInformation("Customer successfully deleted");
        return result;
    }

//
// CUSTOMER ADDRESS METHODS
//
    public async Task<Address?> GetCustomerAddress(Guid customerId, string addressName)
    {
        try
        {
            return await dbContext.LoadAsync<Address>(customerId, addressName);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to find address in database.");
            return null;
        }
    }

    public async Task<IEnumerable<Address>> GetCustomerAddresses(Guid customerId)
    {
        var addresses = new List<Address>();
        try
        {
            var addressQuery = dbContext.QueryAsync<Address>(customerId);
            do
            {
                addresses.AddRange(await addressQuery.GetNextSetAsync());
            } while (!addressQuery.IsDone);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get customer addresses.");
            return new List<Address>();
        }

        return addresses;
    }

    public async Task<bool> AddCustomerAddress(Address address)
    {
        try
        {
            await dbContext.SaveAsync(address);
            logger.LogInformation("Address added to database.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to add address to database.");
            return false;
        }

        return true;
    }

    public async Task<bool> UpdateCustomerAddress(Address? address)
    {
        if (address == null) return false;

        try
        {
            await dbContext.SaveAsync(address);
            logger.LogInformation("Address was updated");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update address");
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteCustomerAddress(Address address)
    {
        bool result;

        try
        {
            await dbContext.DeleteAsync(address);
            // Check for delete success
            var ghost = await dbContext.LoadAsync(address, new DynamoDBOperationConfig { ConsistentRead = true });
            result = ghost == null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to delete address");
            result = false;
        }

        return result;
    }

//
// *STOCK METHODS*
//
    public async Task<IEnumerable<Stock>> GetAllStock(int limit)
    {
        var result = new List<Stock>();

        try
        {
            if (limit <= 0)
                return result;

            var filter = new ScanFilter();
            filter.AddCondition("Id", ScanOperator.IsNotNull);
            var scanConfig = new ScanOperationConfig
            {
                Limit = limit,
                Filter = filter
            };
            var stockSearch = dbContext.FromScanAsync<Stock>(scanConfig);

            do
            {
                result.AddRange(await stockSearch.GetNextSetAsync());
            } while (!stockSearch.IsDone && result.Count < limit);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to find any stock");
            return new List<Stock>();
        }

        return result;
    }

    public async Task<Stock?> GetStock(Guid id)
    {
        try
        {
            return await dbContext.LoadAsync<Stock>(id);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to find stock in the database.");
            return null;
        }
    }

    public async Task<Stock?> GetStockFromStripe(string stripeId)
    {
        try
        {
            var stockIdSearch = dbContext.FromQueryAsync<Stock>(
                new QueryOperationConfig
                {
                    IndexName = "StockStripeIndex",
                    Select = SelectValues.AllProjectedAttributes,
                    KeyExpression = new Expression
                    {
                        ExpressionStatement = "#stripe = :v_stripe",
                        ExpressionAttributeNames = new Dictionary<string, string> { { "#stripe", "StripeId" } },
                        ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                            { { ":v_stripe", new Primitive { Value = stripeId } } }
                    }
                });

            var projectedStock = await stockIdSearch.GetRemainingAsync();
            return await dbContext.LoadAsync<Stock>(projectedStock.FirstOrDefault()!.Id);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to find stock from stripeId");
            return null;
        }
    }

    public async Task<Guid?> AddStock(StockInput nStock)
    {
        try
        {
            switch (nStock.CreateWithoutStripeLink)
            {
                case true when nStock.StripeId != null:
                    throw new Exception("Stock set to create without stripe link, but a stripe link was provided");
                case false when await GetStockFromStripe(nStock.StripeId!) != null:
                    throw new Exception($"Stock with stripeId={nStock.StripeId} already exists.");
            }

            var stock = new Stock
            {
                Id = Guid.NewGuid(),
                StripeId = nStock.StripeId,
                Name = nStock.Name,
                TotalStock = nStock.TotalStock,
                PhotoUri = nStock.PhotoUri,
                Description = nStock.Description,
                Price = nStock.Price,
                DiscountPercentage = nStock.DiscountPercentage
            };

            await dbContext.SaveAsync(stock);
            logger.LogInformation("Stock has been added");
            return stock.Id;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to add stock to database");
            return null;
        }
    }

    public async Task<bool> UpdateStock(Stock? stock)
    {
        if (stock == null) return false;

        try
        {
            await dbContext.SaveAsync(stock);
            logger.LogInformation("Stock was updated");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update stock");
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteStock(Guid stockId)
    {
        bool result;

        try
        {
            await dbContext.DeleteAsync<Stock>(stockId);
            // Check for delete success
            var ghost = await dbContext.LoadAsync<Stock>(stockId, new DynamoDBContextConfig { ConsistentRead = true });
            result = ghost == null;
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to delete stock: id={stockId}");
            result = false;
        }

        if (result) logger.LogInformation("Order successfully deleted");
        return result;
    }

//
// StockRequest METHODS
//
    public async Task<StockRequest?> GetStockRequest(Guid orderId, Guid stockId)
    {
        try
        {
            return await dbContext.LoadAsync<StockRequest>(orderId, stockId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to find stock type in order.");
            return null;
        }
    }

    public async Task<bool> AddStockRequest(StockRequest stock)
    {
        try
        {
            await dbContext.SaveAsync(stock);
            logger.LogInformation("Stock request stored in database.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to add stock request to database.");
            return false;
        }

        return true;
    }

    public async Task<bool> UpdateStockRequest(StockRequest? stock)
    {
        if (stock == null) return false;

        try
        {
            await dbContext.SaveAsync(stock);
            logger.LogInformation("StockRequest was updated");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update StockRequest");
            return false;
        }

        return true;
    }

    // Stock requests should only be deleted when the parent order is deleted.
    public async Task<bool> DeleteStockRequest(StockRequest stock)
    {
        bool result;

        try
        {
            await dbContext.DeleteAsync(stock);
            // Check for delete success
            var ghost = await dbContext.LoadAsync(stock, new DynamoDBOperationConfig { ConsistentRead = true });
            result = ghost == null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to delete stockRequest");
            result = false;
        }

        return result;
    }
}