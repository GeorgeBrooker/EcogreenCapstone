using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ShopRepository.Data;
using ShopRepository.Dtos;
using ShopRepository.Helper;
using ShopRepository.Models;
using Stripe;
using Stripe.Checkout;

namespace ShopRepository.Services;

public class StripeService
{
    private string _publishableKey;
    private readonly IShopRepo _repo;
    private readonly ILogger<StripeService> _logger;
    
    public StripeService(IConfiguration config, IShopRepo shopRepo, ILogger<StripeService> logger)
    {
        _repo = shopRepo;
        _logger = logger;
        var secretKey = config["stripe:SecretKey"] ?? throw new Exception("Stripe Secret Key not found in config");
        _publishableKey = config["stripe:PublishableKey"] ?? throw new Exception("Stripe Publishable Key not found in config");
        StripeConfiguration.ApiKey = secretKey;
    }
    
    // Creates a checkout session for the order and returns a tuple indicating success and the URL to redirect to for payment.
    // If the session creation fails, the tuple will contain false and the error message in place of the URL.
    // Domain input should be the root domain of the orders page for success or failure redirects.
    // TODO validate stock quality before creating an order
    public async Task<Tuple<bool, string>> CreateCheckoutSession(Guid orderId, string domain) 
    {
        try
        {
            // Configure checkout session options
            var lineItems = await GetLineItems(orderId);
            var sessionOptions = new SessionCreateOptions
            {
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = domain + "?success=true", //Redirect to domain with success=true query param TODO create these pages
                CancelUrl = domain + "?canceled=true" //Redirect to domain with canceled=true query param
            };
            
            var sessionService = new SessionService();
            Session session = await sessionService.CreateAsync(sessionOptions);
            
            //Update order with stripe session ID
            var order = await _repo.GetOrder(orderId) ?? throw new Exception($"Order not found in database for orderID {orderId}");
            order.StripeCheckoutSession = session.Id;
            var updated = await _repo.UpdateOrder(order);
            if (!updated) throw new Exception($"Failed to update order with stripe session ID {order.Id}");

            return new Tuple<bool, string> (true, session.Url);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error in CreateCheckoutSession: {e.Message}");
            return new Tuple<bool, string>(false, e.Message);
        }
    }
    
    // Creates a line items options list (Price, quantity) for a given orderID by querying the stripe API for the Price object of each item in the order.
    private async Task<List<SessionLineItemOptions>> GetLineItems(Guid orderId)
    {
        var lineItems = new List<SessionLineItemOptions>();
        try
        {
            var productService = new ProductService();
            var orderLineItems = await _repo.GetOrderStock(orderId);
            
            foreach (var stockItem in orderLineItems)
            {   
                var quantity = stockItem.Quantity;
                var stock = await _repo.GetStock(stockItem.ProductId);
                if (stock?.StripeId == null) throw new Exception($"Stock either could not be found in DB, or has no StripeID. productID={stockItem.ProductId}");
                
                // Get price of item from stripeAPI
                var stripeProd = await productService.GetAsync(stock.StripeId);
                var stripePrice = stripeProd?.DefaultPriceId;
                if (stripePrice == null) throw new Exception($"Stripe product either could not be found or has no default price, stripeID={stock.StripeId}");
                
                lineItems.Add(new SessionLineItemOptions{ Price = stripePrice, Quantity = quantity });
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Error in GetLineItems: {e.Message}");
        }
        return lineItems;
    }

    public async Task<ActionResult> HandleCheckoutSessionComplete(Event stripeEvent)
    {
        _logger.LogInformation("Handling checkout session complete event in stripe service.");
        try
        {
            var checkoutSession = stripeEvent.Data.Object as Session ?? throw new Exception("Checkout Session not found in stripe event data.");
            var order = await _repo.GetOrderFromStripe(checkoutSession.Id) ?? throw new Exception("Order not found in database for stripe session.");
            
            order.OrderStatus = "Confirmed";
            order.CustomerAddress = null;// TODO get customer address from stripe session
            order.StripeCheckoutSession = checkoutSession.Id;
            
            var updated = await _repo.UpdateOrder(order);
            if (!updated) throw new Exception($"Failed to update order in database after successful payment {order.Id}");
        }
        catch (Exception e)
        {
            _logger.LogError("Error in HandleCheckoutSessionComplete. {0}", e.Message);
            return new BadRequestObjectResult($"Error in HandleCheckoutSessionComplete {e.Message}");
        }
        _logger.LogInformation("Finished handling checkout session complete in stripe service.");
        return new OkResult();
    }

    public async Task<ActionResult> HandleAsyncPaymentSucceed(Event stripeEvent)
    {
        throw new NotImplementedException();
    }

    public async Task<ActionResult> HandleAsyncPaymentFail(Event stripeEvent)
    {
        throw new NotImplementedException();
    }

    public async Task<ActionResult> HandleCheckoutExpired(Event stripeEvent)
    {
        throw new NotImplementedException();
    }
    
    //
    // Stripe Stock service methods
    //
    
    public async Task<bool> PersistStockToStripe(Guid stockId)
    {
        try
        {
            var stock = await _repo.GetStock(stockId) ??
                        throw new Exception($"Stock not found in database for stockID {stockId}");
            _logger.LogInformation("Persisting stock to stripe: {0}", stock.Name);

            var imageUri = stock.PhotoUri + StockUploadHelper.CleanUploadName(stock.Name) + "-" + stock.Id + "/1.jpeg";
            
            if (stock.DiscountPercentage > 100) throw new Exception("Discount percentage cannot be greater than 100");
            var price = CalculateNetPrice(stock);
            
            _logger.LogInformation($"Price: {price}");
            var options = new ProductCreateOptions
            {
                Name = stock.Name,
                Description = stock.Description,
                DefaultPriceData = new ProductDefaultPriceDataOptions
                {
                    Currency = "nzd",
                    UnitAmount = price
                },
                Images = [imageUri]
            };
            _logger.LogInformation("Product create options set. Creating product in stripe.");
            
            var service = new ProductService();
            var product = await service.CreateAsync(options);
            if (product == null) throw new Exception("Failed to create product in stripe");
            
            _logger.LogInformation("Product created in stripe. Updating stock with stripeID.");
            
            stock.StripeId = product.Id;
            var updated = await _repo.UpdateStock(stock);
            if (!updated) throw new Exception("Failed to update stock with stripeID");
            
            return true;
        }
        catch(Exception e)
        {
            _logger.LogError("Error in PersistStockToStripe: {0}", e.Message);
            return false;
        }
    }
    public async Task<bool> UpdateStripeStock(Stock stock)
    {
        try
        {
            // Get product and from stripe (including current default price object)
            var stockService = new ProductService();
            var priceService = new PriceService();
            var stripeId = stock.StripeId ?? throw new Exception("Stock has no stripeID");
            
            var product = await stockService.GetAsync(stripeId) ?? throw new Exception("Product not found in stripe");
            var productPrice = await priceService.GetAsync(product.DefaultPriceId) ?? throw new Exception("Product price not found in stripe");

            var price = CalculateNetPrice(stock);
            var stripePrice = productPrice.UnitAmount;

            // Update product in stripe if necessary
            var updated = false;
            if (product.Name != stock.Name && stock.Name != null)
            {
                product.Name = stock.Name;
                updated = true;
            }
            if (product.Description != stock.Description && stock.Description != null)
            {
                product.Description = stock.Description;
                updated = true;
            }
            if (price != stripePrice && stock.Price != null)
            {
                var priceCreateResult = await priceService.CreateAsync(new PriceCreateOptions
                {
                    Product = stripeId,
                    UnitAmount = price,
                    Currency = "nzd"
                });
                if (priceCreateResult == null) throw new StripeException("Failed to create price in stripe");

                product.DefaultPriceId = priceCreateResult.Id;
                updated = true;
            }
            if (product.Images[0] != stock.PhotoUri && stock.PhotoUri != null)
            {
                product.Images[0] = stock.PhotoUri;
                updated = true;
            }
            if (product.Active != stock.Active)
            {
                product.Active = stock.Active;
                updated = true;
            }
            if (!updated) return true;

            await stockService.UpdateAsync(stripeId, new ProductUpdateOptions
            {
                Name = product.Name,
                Description = product.Description,
                DefaultPrice = product.DefaultPriceId,
                Images = product.Images,
                Active = product.Active
            });
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("Error in UpdateStripeStock: {0}", e.Message);
            return false;
        }
    }
    private long CalculateNetPrice(Stock stock)
    {
        return (long)Math.Round(stock.Price * (100 - stock.DiscountPercentage), 0);
    }
    public async Task<bool> DeleteStripeStock(Stock stock)
    {
        var stripeId = stock.StripeId ?? throw new Exception("Stock has no stripeID");
        var stockService = new ProductService();
        try
        {
            await stockService.DeleteAsync(stripeId);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("Error in DeleteStripeStock: {0}", e.Message);
            return false;
        }
    }
}