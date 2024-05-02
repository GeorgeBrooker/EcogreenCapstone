using Amazon.DynamoDBv2.DataModel;
using Stripe;

namespace ShopRepository.Models;

[DynamoDBTable("Orders")]
public class Order
{
    [DynamoDBHashKey] public Guid Id { get; set; } = Guid.Empty;
    
    // Payment info
    [DynamoDBProperty]
    public required string PaymentIntentId { get; set; }
    [DynamoDBProperty] 
    public required string CustomerId { get; set; } // 1-m relationship [This is the customer GUID from our DB not the stripe ID of the customer. StripeID can be retrieved via payment intent]
    
    // Delivery info
    [DynamoDBProperty]
    public string? DeliveryLabelUid { get; set; }
    [DynamoDBProperty]
    public string? TrackingNumber { get; set; }
    
    //Unique package reference for business use.
    [DynamoDBProperty]
    public string? PackageReference { get; set; }
}