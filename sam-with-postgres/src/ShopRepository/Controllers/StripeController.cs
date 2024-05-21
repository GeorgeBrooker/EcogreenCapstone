using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using ShopRepository.Dtos;
using ShopRepository.Services;
using Stripe;
using Stripe.Checkout;

namespace ShopRepository.Controllers;
// TODO review this controller it may be better as a service
[Route("api/stripe")]
[Produces("application/json")]
public class StripeController : ControllerBase
{
    private readonly string _redirectUrl;

    private readonly StripeService _stripeService;
    // API KEY CONFIG
    public StripeController(IConfiguration config, StripeService stripeService)
    {
        StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
        _redirectUrl = config["Stripe:RedirectUrl"] ?? throw new Exception("Stripe Redirect URL not found in config");
        _stripeService = stripeService;
    }
}