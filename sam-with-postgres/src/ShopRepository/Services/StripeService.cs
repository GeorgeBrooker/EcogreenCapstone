using Microsoft.AspNetCore.Mvc;
using ShopRepository.Data;
using ShopRepository.Dtos;
using ShopRepository.Models;
using Stripe;
using Stripe.Checkout;

namespace ShopRepository.Services;

public class StripeService
{
    private string PublishableKey;
    private readonly IShopRepo _repo;
    
    public StripeService(IConfiguration config, IShopRepo shopRepo)
    {
        _repo = shopRepo;
        var secretKey = config["stripe:SecretKey"] ?? throw new Exception("Stripe Secret Key not found in config");
        PublishableKey = config["stripe:PublishableKey"] ?? throw new Exception("Stripe Publishable Key not found in config");
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

            return new Tuple<bool, string> (true, session.Url);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in CreateCheckoutSession: {e.Message}");
            return new Tuple<bool, string>(false, e.Message);
        }
    }
    
    // Creates a line items options list (Price, quantity) for a given orderID by querying the stripe API for the Price object of each item in the order.
    private async Task<List<SessionLineItemOptions>> GetLineItems(Guid orderId)
    {
        try
        {
            var productService = new ProductService();
            var orderLineItems = await _repo.GetOrderStock(orderId);
        
            var lineItems = new List<SessionLineItemOptions>();
            foreach (var stockItem in orderLineItems)
            {   
                Console.WriteLine($"stockItem ProductId={stockItem.ProductId}, Quantity={stockItem.Quantity}");
                // Get quantity of item from order and stock object from the database
                var quantity = stockItem.Quantity;
                var stock = await _repo.GetStock(stockItem.ProductId);
                if (stock?.StripeId == null) throw new Exception($"Stock either could not be found in DB, or has no StripeID. productID={stockItem.ProductId}");
                
                Console.WriteLine($"Stock found stripeId={stock.StripeId}");
                // Get price of item from stripeAPI
                var stripeProd = await productService.GetAsync(stock.StripeId);
                var stripePrice = stripeProd?.DefaultPriceId;
                if (stripePrice == null) throw new Exception($"Stripe product either could not be found or has no default price, stripeID={stock.StripeId}");
                
                Console.WriteLine($"StripePriceId=:{stripePrice}");
                lineItems.Add(new SessionLineItemOptions{ Price = stripePrice, Quantity = quantity });
            }
            return lineItems;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in GetLineItems: {e.Message}");
        }
        return null;
    }
}