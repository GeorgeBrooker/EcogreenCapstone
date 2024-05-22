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
    private readonly string _secret;
    // API KEY CONFIG
    public StripeController(IConfiguration config, StripeService stripeService)
    {
        _config = config;
        _stripeService = stripeService;
        if (_config["Environment"] == "local")
            _secret = _config["Stripe:WebhookSecretLocal"] ?? throw new Exception("Stripe:WebhookSecretLocal secret not found");
        else
            _secret = _config["Stripe:WebhookSecretCheckout"] ?? throw new Exception("Stripe:WebhookSecretCheckout secret not found");
        
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
    }
    
    //TODO I think this could be remade as a generic webhook handler for all stripe events. (Give that its just a marshalling function)
    [HttpPost]
    public async Task<ActionResult> DelegateWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _secret);
            return stripeEvent.Type switch
            {
                // Handle Checkout Session events
                "checkout.session.completed" => await _stripeService.HandleCheckoutSessionComplete(stripeEvent),
                "checkout.session.async_payment_succeeded" => await _stripeService.HandleAsyncPaymentSucceed(stripeEvent),
                "checkout.session.async_payment_failed" => await _stripeService.HandleAsyncPaymentFail(stripeEvent),
                "checkout.session.expired" => await _stripeService.HandleCheckoutExpired(stripeEvent),
                // Handle stock events
                
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