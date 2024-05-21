using ShopRepository.Models;

namespace ShopRepository.Dtos;

public class OrderInput
{
    public string PaymentId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerAddress { get; set; }
    public string OrderStatus { get; set; } = "Pending";
    
     // I hate enums so goddam much
    public PaymentProcessor PaymentType { get; set; } //Used to assign order to a payment processor
    // TODO TEMP DISABLING REQUIRED FOR ALL ABOVE PROPERTIES
    public string? DeliveryLabel { get; set; }
    public string? Tracking { get; set; }
    public string? PackageRef { get; set; }
}