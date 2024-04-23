using System.ComponentModel.DataAnnotations;

namespace ShopRepository.Models;

public class Order
{
    [Key]
    public int Id { get; set; }
    public string PaymentStatus { get; set; }
    public string TrackingNumber { get; set; }
    public DateTime Date { get; set; }
    public DateTime EstimatedArrivalTime { get; set; }
    public float TotalCost { get; set; }
    public string OrderStatus { get; set; }
}