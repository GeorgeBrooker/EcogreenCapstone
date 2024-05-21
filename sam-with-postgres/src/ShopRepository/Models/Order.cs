using Amazon.DynamoDBv2.DataModel;

namespace ShopRepository.Models;

[DynamoDBTable("Orders")]
public class Order
{
    [DynamoDBHashKey] public Guid Id { get; set; } = Guid.Empty;
    [DynamoDBProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Payment info
    [DynamoDBProperty] public string? StripeCheckoutSession { get; set; } = null;// Stripe checkout session ID, will be null until the order is for a different payment service.

    [DynamoDBProperty]
    public required Guid CustomerId { get; set; } // 1-m relationship on customer GUID TODO make this a range key, Or could we even hash on this?

    [DynamoDBProperty]
    public string OrderStatus { get; set; } =
        "Pending"; // Order status can be one of the following: "Pending", "Processing", "Confirmed", "Shipped", "Delivered", "Cancelled" TODO make this an enum


    // Delivery info
    [DynamoDBProperty] public string? DeliveryLabelUid { get; set; }
    [DynamoDBProperty] public string? TrackingNumber { get; set; }

    //Name of the address the customer has attached to this order.
    //We can use this to find the address itself by querying the DB on
    //hashkey customerID and rangekey addressName.
    // Might need to rethink this implementation if we can get away with using paypal/stripe addresses.
    [DynamoDBProperty] public string? CustomerAddress { get; set; }
    [DynamoDBProperty] public string? PackageReference { get; set; } //Unique package reference for business use.
}