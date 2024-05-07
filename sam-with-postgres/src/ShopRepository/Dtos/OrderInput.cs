using System.ComponentModel.DataAnnotations;

namespace ShopRepository.Dtos;

public class OrderInput
{
    [Required] public required string PaymentId { get; set; }
    [Required] public required string CustomerId { get; set; }
    public string? DeliveryLabel { get; set; }
    public string? Tracking { get; set; }
    public string? PackageRef { get; set; }
}