using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopRepository.Data;
using ShopRepository.Dtos;
using ShopRepository.Helper;
using ShopRepository.Models;

namespace ShopRepository.Controllers;

[Route("api/inventory")]
[Produces("application/json")]
public class InventoryController(IShopRepo repo, StockUploadHelper stockUploader) : ControllerBase
{
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

    [HttpGet("GetOrderByPaymentId/{id}")]
    public async Task<ActionResult<Order>> GetOrderByPaymentId(string id)
    {
        return Ok(await repo.GetOrderFromPaymentId(id));
    }

    [HttpGet("GetOrderStock/{id:guid}")]
    public async Task<ActionResult<IEnumerable<StockRequest>>> GetOrderStock(Guid id)
    {
        return Ok(await repo.GetOrderStock(id));
    }

    // POST
    // This will need to be modified to sync with stripe. We may want to call this internally from a different endpoint.
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

        updated.PaymentIntentId = order.PaymentId;
        updated.CustomerId = order.CustomerId;
        updated.DeliveryLabelUid = order.DeliveryLabel;
        updated.TrackingNumber = order.Tracking;
        updated.PackageReference = order.PackageRef;

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
// STOCK PHOTOS
//
    [HttpPost("UploadPhotos/{stockId:guid}")]
    public async Task<ActionResult<bool>> UploadPhotos(Guid stockId)
    {
        var form = Request.Form;
        var files = Request.Form.Keys;
        var images = new byte[files.Count][];

        var i = 0;
        foreach (var key in files)
        {
            images[i] = Convert.FromBase64String(form[key]!);
            i++;
        }

        var result = await stockUploader.UploadImages(images, stockId);

        if (result)
            return Ok();

        return BadRequest("Image upload failed");
    }
}