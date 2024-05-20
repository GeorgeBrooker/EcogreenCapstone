using Amazon.DynamoDBv2.DataModel;

namespace ShopRepository.Models;

[DynamoDBTable("Orders")]
public class Order
{
    [DynamoDBHashKey] public Guid Id { get; set; } = Guid.Empty;
    [DynamoDBProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Payment info
    [DynamoDBProperty]
    public required string PaymentIntentId { get; set; } // StripeID can be retrieved via payment intent


    [DynamoDBProperty]
    public required Guid CustomerId { get; set; } // 1-m relationship on customer GUID TODO make this a range key

    [DynamoDBProperty]
    public string OrderStatus { get; set; } =
        "Pending"; // Order status can be one of the following: "Pending", "Processing", "Shipped", "Delivered", "Cancelled"


    // Delivery info
    [DynamoDBProperty] public string? DeliveryLabelUid { get; set; }
    [DynamoDBProperty] public string? TrackingNumber { get; set; }

    //Name of the address the customer has attached to this order.
    //We can use this to find the address itself by querying the DB on
    //hashkey customerID and rangekey addressName.
    [DynamoDBProperty] public required string CustomerAddress { get; set; }
    [DynamoDBProperty] public string? PackageReference { get; set; } //Unique package reference for business use.
}