using Amazon.DynamoDBv2.DataModel;
using Stripe;

namespace ShopRepository.Models;

[DynamoDBTable("Orders")]
public class Order
{
    [DynamoDBHashKey] public Guid Id { get; set; } = Guid.Empty;
    
    // Payment info
    [DynamoDBProperty]
    public string? PaymentIntentId { get; set; }
    [DynamoDBProperty] // 1-m relationship
    public string? CustomerId { get; set; }
    
    // Delivery info
    [DynamoDBProperty]
    public string? DeliveryLabelUid { get; set; }
    [DynamoDBProperty]
    public string? TrackingNumber { get; set; }
    
    //Unique package reference for business use.
    [DynamoDBProperty]
    public string? PackageReference { get; set; }
}