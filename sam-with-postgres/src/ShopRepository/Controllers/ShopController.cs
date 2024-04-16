using Microsoft.AspNetCore.Mvc;
using ShopRepository.Models;
using ShopRepository.Data;

namespace ShopRepository.Controllers;


[Route("api/shopapi")]
[Produces("application/json")]
public class BooksController : ControllerBase
{
    private readonly IShopRepo _repo;

    public BooksController(IShopRepo shopRepo)
    {
        this._repo = shopRepo;
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

        if (result == null)
        {
            return NotFound();
        }

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
