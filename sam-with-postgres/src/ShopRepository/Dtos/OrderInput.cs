using System.ComponentModel.DataAnnotations;

namespace ShopRepository.Dtos;

public class OrderInput
{
    [Required] public string paymentId { get; set; }
    [Required] public string customerId { get; set; }
    public string deliveryLabel { get; set; }
    public string tracking { get; set; }
    public string packageRef { get; set; }
}