using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopRepository.Data;
using ShopRepository.Dtos;
using ShopRepository.Models;

namespace ShopRepository.Controllers;

[Route("api/shop")]
[Produces("application/json")]

// TODO update database hashkey format to allow for multiple primary keys.
public class ShopController : ControllerBase
{
    private readonly IShopRepo _repo;

    public ShopController(IShopRepo shopRepo)
    {
        _repo = shopRepo;
    }

    // TestUp
    [HttpGet]
    public ActionResult GetAlive()
    {
        return Ok("You are now listening to the shop controller");
    }
    
    // *CUSTOMER*
    // GET
    [HttpGet("GetCustomers")]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers([FromQuery] int limit = 10)
    {
        return Ok(await _repo.GetAllCustomers(limit));
    }

    [HttpGet("GetCustomerFromStripe/{stripeId}")]
    public async Task<ActionResult<Customer>> GetCustomerFromStripe(string stripeId)
    {
        return Ok(await _repo.GetCustomerFromStripe(stripeId));
    }

    [HttpGet("GetCustomerFromEmail/{email}")]
    public async Task<ActionResult<Customer>> GetCustomerFromEmail(string email)
    {
        return Ok(await _repo.GetCustomerFromEmail(email));
    }

    [HttpGet("GetCustomerByID/{id}")]
    public async Task<ActionResult<Customer>> GetCustomer(Guid id)
    {
        return Ok(await _repo.GetCustomer(id));
    }

    [HttpGet("GetCustomerOrders/{customerId}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetCustomerOrders(Guid customerId)
    {
        return Ok(await _repo.GetCustomerOrders(customerId));
    }
    
    // POST 
    [HttpPost("AddCustomer")]
    public async Task<ActionResult<CustomerInput>> AddCustomer([FromBody] CustomerInput nCustomer)
    {
        if (await _repo.AddCustomer(nCustomer))
            return Ok(nCustomer);
        
        return BadRequest();
    }
    
    // PUT 
    
    // This method should only be called with a complete CustomerInput DTO as input.
    // Input DTO should be created from a fresh retrieval of the customer information.
    // You will have to retrieve the customer object to get ID anyway so this shouldn't be expensive.
    [HttpPut("UpdateCustomer/{id}")]
    public async Task<ActionResult> UpdateCustomer(Guid id, [FromBody] CustomerInput? customer)
    {
        if (id == Guid.Empty || customer == null) return ValidationProblem("Invalid payload");
        
        var updated =  await _repo.GetCustomer(id);
        if (updated == null) return NotFound($"Could not find existing customer with id={id}. Update canceled.");

        updated.Email = customer.Email;
        updated.FirstName = customer.Fname;
        updated.LastName = customer.Fname;
        updated.Password = new PasswordHasher<Customer>().HashPassword(updated, customer.Pass);

        await _repo.UpdateCustomer(updated);
        return Ok();
    }
    
    // DELETE
    [HttpDelete("DeleteCustomer/{id}")]
    public async Task<ActionResult> DeleteCustomer(Guid id)
    {
        if (id == Guid.Empty) return ValidationProblem("Invalid request ID");

        var customer = await _repo.GetCustomer(id);
        if (customer == null)
            return NotFound($"No customer exists with id {id}");

        await _repo.DeleteCustomer(customer.Id);
        return Ok();
    }
    
    // *ORDERS*
    
    // GET
    [HttpGet("GetOrders")]
    public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders(int limit = 20)
    {
        return Ok(await _repo.GetAllOrders(limit));
    }
    [HttpGet("GetOrder/{id}")]
    public async Task<ActionResult<Order>> GetOrderById(Guid id)
    {
        return Ok(await _repo.GetOrder(id));
    }
    [HttpGet("GetOrderByPaymentId/{id}")]
    public async Task<ActionResult<Order>> GetOrderByPaymentId(string id)
    {
        return Ok(await _repo.GetOrderFromPaymentId(id));
    }
    
    // POST
    // This will need to be modified to sync with stripe. We may want to call this internally from a different endpoint.
    [HttpPost("AddOrder")]
    public async Task<ActionResult<OrderInput>> AddOrder([FromBody] OrderInput nOrder)
    {
        if (await _repo.AddOrder(nOrder))
            return Ok(nOrder);
        
        return BadRequest();
    }
    
    // PUT
    // This method should only be called with a complete OrderInput DTO as input.
    // See equivalent customer method for more details
    [HttpPut("UpdateOrder/{id}")]
    public async Task<ActionResult> UpdateOrder(Guid id, [FromBody] OrderInput? order)
    {
        if (Guid.Empty == id || order == null) return ValidationProblem("Invalid payload");

        var updated = await _repo.GetOrder(id);
        if (updated == null) throw new Exception($"Could not find existing order with id={id}. Update canceled.");

        updated.PaymentIntentId = order.paymentId;
        updated.CustomerId = order.customerId;
        updated.DeliveryLabelUid = order.deliveryLabel;
        updated.TrackingNumber = order.tracking;
        updated.PackageReference = order.packageRef;

        await _repo.UpdateOrder(updated);
        return Ok();
    }
    
    // DELETE
    public async Task<ActionResult> DeleteOrder(Guid id)
    {
        if (Guid.Empty == id) return ValidationProblem("Invalid request ID");

        var order = await _repo.GetOrder(id);
        if (order == null) return NotFound($"No order exists with id={id}");

        await _repo.DeleteOrder(id);
        return Ok();
    }
    
    // STOCK
    // GET
    
}