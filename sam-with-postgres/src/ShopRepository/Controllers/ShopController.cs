using Microsoft.AspNetCore.Mvc;
using ShopRepository.Data;
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
}