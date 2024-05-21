using System.Text.Json.Serialization;

namespace ShopRepository.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentProcessor
{
    Stripe,
    Paypal,
}