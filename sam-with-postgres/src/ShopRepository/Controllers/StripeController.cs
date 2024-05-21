using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using ShopRepository.Dtos;
using ShopRepository.Services;
using Stripe;
using Stripe.Checkout;

namespace ShopRepository.Controllers;
// This controller is responsible for handing stripe webhook callbacks.
[Route("api/stripe")]
[Produces("application/json")]
public class StripeController : ControllerBase
{
    private readonly StripeService _stripeService;
    private readonly IConfiguration _config;
    // API KEY CONFIG
    public StripeController(IConfiguration config, StripeService stripeService)
    {
        _config = config;
        _stripeService = stripeService;
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
    }
    
    //TODO I think this could be remade as a generic webhook handler for all stripe events. (Give that its just a marshalling function)
    [HttpPost("CheckoutHook")]
    public async Task<ActionResult> HandleCheckoutHook()
    {
        string secret;
        if (_config["Environment"] == "local")
            secret = _config["Stripe:WebhookSecretLocal"] ?? throw new Exception("Stripe:WebhookSecretLocal secret not found");
        else
            secret = _config["Stripe:WebhookSecretCheckout"] ?? throw new Exception("Stripe:WebhookSecretCheckout secret not found");

        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], secret);
            return stripeEvent.Type switch
            {
                "checkout.session.completed" => await _stripeService.HandleCheckoutSessionComplete(stripeEvent),
                "checkout.session.async_payment_succeeded" => await _stripeService.HandleAsyncPaymentSucceed(stripeEvent),
                "checkout.session.async_payment_failed" => await _stripeService.HandleAsyncPaymentFail(stripeEvent),
                "checkout.session.expired" => await _stripeService.HandleCheckoutExpired(stripeEvent),
                _ => Ok()
            };
        }
        catch (Exception e)
        {
            Console.WriteLine($"Webhook error: {e.Message}");
            return BadRequest();
        }
    }
}