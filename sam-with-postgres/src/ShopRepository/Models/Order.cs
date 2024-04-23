using Amazon.DynamoDBv2.DataModel;
using Stripe;

namespace ShopRepository.Models;

[DynamoDBTable("Orders")]
public class Order
{
    [DynamoDBHashKey] public Guid Id { get; set; } = Guid.Empty;
    
    // Payment info
    [DynamoDBProperty]
    public string PaymentIntentID { get; set; }
    [DynamoDBProperty] // 1-m relationship
    public string CustomerID { get; set; }
    
    // Delivery info
    [DynamoDBProperty]
    public string DeliveryLabelUID { get; set; }
    [DynamoDBProperty]
    public string TrackingNumber { get; set; }
    [DynamoDBProperty]
    public string PackageReference { get; set; }
}