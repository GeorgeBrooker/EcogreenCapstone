using Microsoft.AspNetCore.Mvc;
using Amazon.DynamoDBv2.DataModel;
using ShopRepository.Data;
using ShopRepository.Dtos;
using ShopRepository.Models;

namespace ShopRepository.Controllers;

[Route("api/shop")]
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

    [HttpPost("AddCustomer")]
    public ActionResult<CustomerInput> AddCustomer(CustomerInput nCustomer)
    {
        if (_repo.AddCustomer(nCustomer).Result)
            return Ok(nCustomer);
        
        return BadRequest();
    }

    [HttpGet("GetCustomers")]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers([FromQuery] int limit = 10)
    {
        return Ok(await _repo.GetAllCustomers(limit));
    }
}