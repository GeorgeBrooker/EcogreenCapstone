using Microsoft.AspNetCore.Mvc;
using ShopRepository.Data;
using ShopRepository.Models;
using Stripe;

namespace ShopRepository.Controllers;

[Route("api/shopapi")]
[Produces("application/json")]
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

    [HttpGet("AllCustomers")]
    public ActionResult<StripeList<Stripe.Customer>> GetCustomers()
    {
        StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_API_KEY");
        var options = new CustomerListOptions { Limit = 10 };
        var service = new CustomerService();
        
        return Ok(service.List(options));
    }

    // GET api/shopapi/getorders
    [HttpGet("GetOrders")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        return Ok(await _repo.GetAllOrdersAsync());
    }

    // GET api/shopapi/getorder
    [HttpGet("GetOrder/{id:int}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var result = await _repo.GetOrderAsync(id);

        if (result == null) return NotFound();

        return Ok(result);
    }

    // POST api/shopapi/addorder
    [HttpPost("AddOrder")]
    public async Task<ActionResult<Order>> AddOrder(Order? order)
    {
        if (order == null) return ValidationProblem("Invalid input! Book not informed");

        await _repo.AddOrderAsync(order);
        return Ok();
    }
}