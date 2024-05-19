using System.ComponentModel.DataAnnotations;

namespace ShopRepository.Dtos;

public class OrderInput
{
    [Required] public required string PaymentId { get; set; }
    [Required] public required Guid CustomerId { get; set; }
    [Required] public required string CustomerAddress { get; set; }
    [Required] public required string OrderStatus { get; set; } = "Pending";
    public string? DeliveryLabel { get; set; }
    public string? Tracking { get; set; }
    public string? PackageRef { get; set; }
}