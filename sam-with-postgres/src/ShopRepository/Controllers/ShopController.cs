using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopRepository.Data;
using ShopRepository.Dtos;
using ShopRepository.Models;
using ShopRepository.Services;


namespace ShopRepository.Controllers;

[Route("api/shop")]
[Produces("application/json")]
public class ShopController(IShopRepo repo, StripeService stripeService, Sesemail sesemail, IConfiguration config, ILogger<ShopController> logger) : ControllerBase
{
    // TODO Move the majority of this logic outside the shop controller and into the inventory controller.
    // TestUp
    [HttpGet]
    public ActionResult GetAlive()
    {
        return Ok("You are now listening to the shop controller");
    }

//
// *CUSTOMER*
//
    // GET
    [HttpGet("GetCustomers")]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers([FromQuery] int limit = 20)
    {
        return Ok(await repo.GetAllCustomers(limit));
    }

    [HttpGet("GetCustomerFromStripe/{stripeId}")]
    public async Task<ActionResult<Customer>> GetCustomerFromStripe(string stripeId)
    {
        return Ok(await repo.GetCustomerFromStripe(stripeId));
    }

    [HttpGet("GetCustomerFromEmail/{email}")]
    public async Task<ActionResult<Customer>> GetCustomerFromEmail(string email)
    {
        return Ok(await repo.GetCustomerFromEmail(email));
    }

    [HttpGet("GetCustomerByID/{id:guid}")]
    public async Task<ActionResult<Customer>> GetCustomer(Guid id)
    {
        return Ok(await repo.GetCustomer(id));
    }

    [HttpGet("GetCustomerOrders/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetCustomerOrders(Guid customerId)
    {
        return Ok(await repo.GetCustomerOrders(customerId));
    }
    [HttpGet("GetOrderStock/{orderId:guid}")]
    public async Task<ActionResult<IEnumerable<StockRequest>>> GetOrderStock(Guid orderId)
    {
        return Ok(await repo.GetOrderStock(orderId));
    }

    // POST 
    [HttpPost("AddCustomer")]
    public async Task<ActionResult<CustomerInput>> AddCustomer([FromBody] CustomerInput nCustomer)
    {
        if (await repo.GetCustomerFromEmail(nCustomer.Email) != null)
            return BadRequest("Customer with that email already exists.");

        if (await repo.AddCustomer(nCustomer))
            return Ok(nCustomer);

        return BadRequest("Sign up failed, please try again.");
    }

    // PUT 

    // This method should only be called with a complete CustomerInput DTO as input.
    // Input DTO should be created from a fresh retrieval of the customer information.
    // You will have to retrieve the customer object to get ID anyway so this shouldn't be expensive.
    [HttpPut("UpdateCustomer/{id:guid}")]
    public async Task<ActionResult> UpdateCustomer(Guid id, [FromBody] CustomerInput? customer)
    {
        if (id == Guid.Empty || customer == null) return ValidationProblem("Invalid payload");

        var updated = await repo.GetCustomer(id);
        if (updated == null) return NotFound($"Could not find existing customer with id={id}. Update canceled.");

        updated.Email = customer.Email;
        updated.FirstName = customer.Fname;
        updated.LastName = customer.Fname;
        updated.Password = new PasswordHasher<Customer>().HashPassword(updated, customer.Pass);

        await repo.UpdateCustomer(updated);
        return Ok();
    }
//
// *STOCK*
//
    // GET
    [HttpGet("GetStockForSale")]
    public async Task<ActionResult<IEnumerable<Stock>>> GetALlStockForSale([FromQuery] int limit = 1000)
    {
        var stock = await repo.GetAllStock(limit);
        return Ok(stock.Where(s => s.Active));
    }
    
    [HttpGet("GetStock/{id:guid}")]
    public async Task<ActionResult<Stock>> GetStock(Guid id)
    {
        return Ok(await repo.GetStock(id));
    }

    [HttpGet("GetStockFromStripe/{stripeId}")]
    public async Task<ActionResult<IEnumerable<Stock>>> GetStockFromStrip(string stripeId)
    {
        return Ok(await repo.GetStockFromStripe(stripeId));
    }


    //
    // CUSTOMER ADDRESSES
    //

    // GET
    [HttpGet("GetCustomerAddresses/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<Address>>> GetCustomerAddresses(Guid customerId)
    {
        return Ok(await repo.GetCustomerAddresses(customerId));
    }

    [HttpGet("GetCustomerAddress/{customerId:guid}/{addressName}")]
    public async Task<ActionResult<Address>> GetCustomerAddress(Guid customerId, string addressName)
    {
        return Ok(await repo.GetCustomerAddress(customerId, addressName));
    }

    // POST
    [HttpPost("AddCustomerAddress")]
    public async Task<ActionResult<bool>> AddCustomerAddress([FromBody] Address address)
    {
        var addressExists = await repo.GetCustomerAddress(address.CustomerId, address.AddressName);
        if (addressExists != null) return BadRequest("Address already exists.");
        return Ok(await repo.AddCustomerAddress(address));
    }

    // PUT
    [HttpPut("UpdateCustomerAddress")]
    public async Task<ActionResult<bool>> UpdateCustomerAddress([FromBody] AddressInput nAddress)
    {
        var address = await repo.GetCustomerAddress(nAddress.CustomerId, nAddress.AddressName);
        if (address == null) return BadRequest("Address does not exist.");

        // Map the input to the existing address
        address.CustomerId = nAddress.CustomerId;
        address.AddressName = nAddress.AddressName;
        address.Line1 = nAddress.StreetNumber + " " + nAddress.Street;
        address.Line2 = nAddress.State;
        address.City = nAddress.City;
        address.PostCode = nAddress.PostCode;
        address.Country = nAddress.Country;
        address.Email = nAddress.Email;

        return Ok(await repo.UpdateCustomerAddress(address));
    }

    // DELETE
    [HttpDelete("DeleteCustomerAddress/{customerId:guid}/{addressName}")]
    public async Task<ActionResult<bool>> DeleteCustomerAddress(Guid customerId, string addressName)
    {
        var address = await repo.GetCustomerAddress(customerId, addressName);
        if (address == null) return BadRequest("Address does not exist.");

        return Ok(await repo.DeleteCustomerAddress(address));
    }
    //
    // AWS SES
    //
    
    [HttpPost("ContactUs")]
    public async Task<IActionResult> SendEmail([FromBody] Sesemail.EmailInput emailInput)
    {
        var emailService = sesemail;
        var result = await emailService.SendEmail(emailInput);
        return result ? Ok("Email sent successfully") : BadRequest("Failed to send email");
    }

//
// Order processing
//

    [HttpPost("ProcessCheckout")]
    public async Task<ActionResult> ProcessCheckout([FromBody] CheckoutSessionInput checkoutSession)
    {
        // Get the redirect URL from the config, get the order and stock requests from the checkout session.
        var redirectUrl = config["Payment:RedirectUrl"] ?? throw new InvalidOperationException("Config has no payment redirect URL");
        var order = checkoutSession.Order;
        var stockRequests = checkoutSession.StockRequests;
        foreach (var lineItem in stockRequests)
        {
            var stock = await repo.GetStock(lineItem.ProductId);
            if (stock == null) return BadRequest($"Product {lineItem.ProductId} not found in stock");
            
            lineItem.Subtotal = stock.Price * lineItem.Quantity;
            lineItem.ProductName = stock.Name;
            order.TotalCost += lineItem.Subtotal;
        }

        // Add order and stock requests to the local database
        var orderId = await repo.AddOrder(order);
        if (orderId == null) return BadRequest("Failed to add order to the DB");
        
        foreach (var requestInput in stockRequests)
        {
            var stockRequest = new StockRequest
            {
                OrderId = (Guid)orderId,
                ProductId = requestInput.ProductId,
                Quantity = requestInput.Quantity,
                Subtotal = requestInput.Subtotal,
                ProductName = requestInput.ProductName
            };
            var stockRequestResult = await repo.AddStockRequest(stockRequest);
            if (!stockRequestResult) return BadRequest($"Failed to add stock request to DB for product {requestInput.ProductId}");
        }
        
        // Call relevant payment service based on order payment method
        return order.PaymentType switch
        {
            PaymentProcessor.Stripe => await ProcessStripeOrder((Guid)orderId, redirectUrl),
            PaymentProcessor.Paypal => BadRequest("Paypal payment processing not yet implemented"),
            _ => BadRequest($"Order cannot be processed [Malformed PaymentType: {order.PaymentType}]")
        };
    }
    
    private async Task<ActionResult> ProcessStripeOrder(Guid orderId, string redirectUrl)
    {
        logger.LogInformation("ShopController is processing a stripe order. Creating checkout session...");
        try
        {
            var checkoutSession = await stripeService.CreateCheckoutSession(orderId, redirectUrl);
            if (!checkoutSession.Item1) throw new Exception($"Failed to create checkout session: {checkoutSession.Item2}");

            logger.LogInformation($"Creation complete...Redirecting to checkout session");
            
            return Ok(new {redirectUrl = checkoutSession.Item2}); // Not using a native redirect here as it freaks out the frontend CORS.
        }
        catch (Exception e)
        {
            logger.LogError("Error processing stripe order");
            return BadRequest(e.Message);
        }
    }
    // TODO add method for processing paypal orders
}