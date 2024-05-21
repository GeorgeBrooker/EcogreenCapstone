using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopRepository.Data;
using ShopRepository.Dtos;
using ShopRepository.Models;
using ShopRepository.Services;

namespace ShopRepository.Controllers;

[Route("api/shop")]
[Produces("application/json")]
public class ShopController(IShopRepo repo, StripeService stripeService, IConfiguration config, ILogger<ShopController> logger) : ControllerBase
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

    // DELETE
    [HttpDelete("DeleteCustomer/{id:guid}")]
    public async Task<ActionResult> DeleteCustomer(Guid id)
    {
        if (id == Guid.Empty) return ValidationProblem("Invalid request ID");

        var customer = await repo.GetCustomer(id);
        if (customer == null)
            return NotFound($"No customer exists with id {id}");

        await repo.DeleteCustomer(customer.Id);
        return Ok();
    }

//
// *ORDERS*
// 
    // GET
    [HttpGet("GetOrders")]
    public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders([FromQuery] int limit = 20)
    {
        return Ok(await repo.GetAllOrders(limit));
    }

    [HttpGet("GetOrder/{id:guid}")]
    public async Task<ActionResult<Order>> GetOrderById(Guid id)
    {
        return Ok(await repo.GetOrder(id));
    }

    [HttpGet("GetOrderByStripeCheckout/{id}")]
    public async Task<ActionResult<Order>> GetOrderByPaymentId(string id)
    {
        return Ok(await repo.GetOrderFromStripe(id));
    }

    [HttpGet("GetOrderStock/{id:guid}")]
    public async Task<ActionResult<IEnumerable<StockRequest>>> GetOrderStock(Guid id)
    {
        return Ok(await repo.GetOrderStock(id));
    }

    // POST
    // TODO This will need to be modified to sync with stripe. We may want to call this internally from a different endpoint.
    [HttpPost("AddOrder")]
    public async Task<ActionResult<OrderInput>> AddOrder([FromBody] OrderInput nOrder)
    {
        if (await repo.AddOrder(nOrder) != null)
            return Ok(nOrder);

        return BadRequest();
    }

    // PUT
    // This method should only be called with a complete OrderInput DTO as input.
    // See equivalent customer method for more details
    [HttpPut("UpdateOrder/{id:guid}")]
    public async Task<ActionResult> UpdateOrder(Guid id, [FromBody] OrderInput? order)
    {
        if (Guid.Empty == id || order == null) return ValidationProblem("Invalid payload");

        var updated = await repo.GetOrder(id);
        if (updated == null) throw new Exception($"Could not find existing order with id={id}. Update canceled.");

        updated.StripeCheckoutSession = order.StripeCheckoutSession;
        updated.CustomerId = order.CustomerId;
        updated.DeliveryLabelUid = order.DeliveryLabel;
        updated.TrackingNumber = order.Tracking;
        updated.PackageReference = order.PackageRef;
        updated.CustomerAddress = order.CustomerAddress;
        updated.OrderStatus = order.OrderStatus;

        await repo.UpdateOrder(updated);
        return Ok();
    }

    // DELETE
    [HttpDelete("DeleteOrder/{id:guid}")]
    public async Task<ActionResult> DeleteOrder(Guid id)
    {
        if (Guid.Empty == id) return ValidationProblem("Invalid request ID");

        var order = await repo.GetOrder(id);
        if (order == null) return NotFound($"No order exists with id={id}");

        await repo.DeleteOrder(id);
        return Ok();
    }

//
// *STOCK*
//
    // GET
    [HttpGet("GetAllStock")]
    public async Task<ActionResult<IEnumerable<Stock>>> GetAllStock([FromQuery] int limit = 20)
    {
        return Ok(await repo.GetAllStock(limit));
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

    // POST
    [HttpPost("AddStock")]
    public async Task<ActionResult<bool>> AddStock([FromBody] StockInput stock)
    {
        return Ok(await repo.AddStock(stock));
    }

    // PUT
    [HttpPut("UpdateStock")]
    public async Task<ActionResult<bool>> UpdateStock([FromBody] StockInput stock)
    {
        if (stock.StripeId == null)
            return BadRequest("Cannot update implicitly via stripeId when input stock has no stripeId");

        var retrievedStock = await repo.GetStockFromStripe(stock.StripeId);

        if (retrievedStock == null)
            return BadRequest($"Stock could not be found with StripeId={stock.StripeId}");

        retrievedStock.Name = stock.Name;
        retrievedStock.StripeId = stock.StripeId;
        retrievedStock.TotalStock = stock.TotalStock;
        retrievedStock.PhotoUri = stock.PhotoUri;
        retrievedStock.Description = stock.Description;
        retrievedStock.Price = stock.Price;
        retrievedStock.DiscountPercentage = stock.DiscountPercentage;

        return Ok(await repo.UpdateStock(retrievedStock));
    }

    [HttpPut("UpdateStock/{stockId:guid}")]
    public async Task<ActionResult<bool>> UpdateStock(Guid stockId, [FromBody] StockInput stock)
    {
        var retrievedStock = await repo.GetStock(stockId);
        if (retrievedStock == null) return BadRequest($"Stock could not be found with GUID={stockId}");

        retrievedStock.Name = stock.Name;
        retrievedStock.StripeId = stock.StripeId;
        retrievedStock.TotalStock = stock.TotalStock;
        retrievedStock.PhotoUri = stock.PhotoUri;
        retrievedStock.Description = stock.Description;
        retrievedStock.Price = stock.Price;
        retrievedStock.DiscountPercentage = stock.DiscountPercentage;

        return Ok(await repo.UpdateStock(retrievedStock));
    }

    // DELETE
    [HttpDelete("DeleteStock/{id:guid}")]
    public async Task<ActionResult<bool>> DeleteStock(Guid id)
    {
        if (Guid.Empty == id) return ValidationProblem("StockId in request is not a valid Id.");

        var retrievedStock = await repo.GetStock(id);
        if (retrievedStock == null)
            return NotFound($"No stock exists with stockId={id}");

        await repo.DeleteStock(id);
        return Ok();
    }

//
// *StockRequests*
//
    // GET
    [HttpGet("GetStockRequest/{orderId:guid}/{stockId:guid}")]
    public async Task<ActionResult<StockRequest>> GetStockRequest(Guid orderId, Guid stockId)
    {
        return Ok(await repo.GetStockRequest(orderId, stockId));
    }

    // POST

    // TESTING ONLY. StockRequest SHOULD ONLY BE CREATED BY BACKEND BUSINESS LOGIC, NOT FRONTEND REQUESTS.
    // FOR THIS REASON NO DTO IS PROVIDED
    [HttpPost("AddStockRequest")]
    public async Task<ActionResult<bool>> AddStockRequest(
        [FromQuery] Guid orderId,
        [FromQuery] Guid stockId,
        [FromQuery] int quantity)
    {
        var stockRequest = new StockRequest
        {
            OrderId = orderId,
            ProductId = stockId,
            Quantity = quantity
        };

        return Ok(await repo.AddStockRequest(stockRequest));
    }

    // PUT
    [HttpPost("UpdateStockRequest")]
    public async Task<ActionResult<bool>> UpdateStockRequest(
        [FromQuery] Guid? orderId,
        [FromQuery] Guid? stockId,
        [FromQuery] int? quantity)
    {
        if (orderId == null || stockId == null || quantity == null)
            return BadRequest("All attributes must be specified to update a stockRequest");

        var stockRq = await repo.GetStockRequest((Guid)orderId, (Guid)stockId);
        if (stockRq == null)
            return NotFound($"StockRequest for stock id={stockId} in order id={orderId} could not be found");

        stockRq.Quantity = (int)quantity;
        await repo.UpdateStockRequest(stockRq);

        return true;
    }

    // Delete
    [HttpDelete("DeleteStockRequest/{orderId:guid}/{stockId:guid}")]
    public async Task<ActionResult<bool>> DeleteStockRequest(Guid orderId, Guid stockId)
    {
        if (Guid.Empty == orderId || Guid.Empty == stockId)
            return ValidationProblem("One or more of the inputted Ids are invalid.");

        var retreivedStockRq = await repo.GetStockRequest(orderId, stockId);
        if (retreivedStockRq == null)
            return NotFound($"No request for stock id={stockId} could be found in order id={orderId}");

        await repo.DeleteStockRequest(retreivedStockRq);
        return Ok();
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
        address.StreetNumber = nAddress.StreetNumber;
        address.Street = nAddress.Street;
        address.City = nAddress.City;
        address.PostCode = nAddress.PostCode;
        address.Country = nAddress.Country;
        address.PhoneNumber = nAddress.PhoneNumber;
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
// Order processing
//

    [HttpPost("ProcessCheckout")]
    public async Task<ActionResult> ProcessCheckout([FromBody] CheckoutSessionInput checkoutSession)
    {
        // Get the redirect URL from the config, get the order and stock requests from the checkout session.
        var redirectUrl = config["Payment:RedirectUrl"] ?? throw new InvalidOperationException("Config has no payment redirect URL");
        var order = checkoutSession.Order;
        var stockRequests = checkoutSession.StockRequests;
        
        // Add order and stock requests to the local database
        var orderId = await repo.AddOrder(order);
        if (orderId == null) return BadRequest("Failed to add order to the DB");
        
        foreach (var requestInput in stockRequests)
        {
            var stockRequest = new StockRequest
            {
                OrderId = (Guid)orderId,
                ProductId = requestInput.ProductId,
                Quantity = requestInput.Quantity
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