using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace ShopRepository.Controllers;

[Route("api/stripe")]
[Produces("application/json")]
public class StripeController : ControllerBase
{
    // API KEY CONFIG
    public StripeController() { StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_API_KEY"); }
    
    [HttpGet("AllCustomers")]
    public ActionResult<StripeList<Stripe.Customer>> GetCustomers()
    {
        
        var options = new CustomerListOptions { Limit = 10 };
        var service = new CustomerService();
        
        return Ok(service.List(options));
    }
}