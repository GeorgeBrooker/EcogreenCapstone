using System.ComponentModel.DataAnnotations;

namespace ShopRepository.Dtos;

public class OrderInput(string payId, string customId)
{
    [Required] public string PaymentId { get; set; } = payId;
    [Required] public string CustomerId { get; set; } = customId;
    public string? DeliveryLabel { get; set; }
    public string? Tracking { get; set; }
    public string? PackageRef { get; set; }
}