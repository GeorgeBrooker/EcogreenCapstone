using Amazon.DynamoDBv2.DataModel;

namespace ShopRepository.Models;

[DynamoDBTable("Stock")]
public class Stock
{
    [DynamoDBHashKey] public Guid Id { get; set; } = Guid.Empty;
    [DynamoDBProperty] public required string Name { get; set; }
    [DynamoDBProperty] public string? StripeId { get; set; }
    [DynamoDBProperty] public long? TotalStock { get; set; }
    [DynamoDBProperty] public string PhotoUri { get; set; } = "";
    [DynamoDBProperty] public string Description { get; set; } = "";
    [DynamoDBProperty] public decimal Price { get; set; } = 50;
    [DynamoDBProperty] public int DiscountPercentage { get; set; }
    // Shipping
    [DynamoDBProperty] public double CMheight { get; set; } = 30;
    [DynamoDBProperty] public double CMwidth { get; set; } = 10;
    [DynamoDBProperty] public double CMlength { get; set; } = 10;
    [DynamoDBProperty] public double KGweight { get; set; } = 0.5;
    [DynamoDBProperty] public decimal ShippingCost { get; set; } = 5;
    
    [DynamoDBProperty] public bool Active { get; set; } = true;
    
    public Stock DeepCopy()
    {
        return new Stock
        {
            Id = Id,
            Name = Name,
            StripeId = StripeId,
            TotalStock = TotalStock,
            PhotoUri = PhotoUri,
            Description = Description,
            Price = Price,
            DiscountPercentage = DiscountPercentage
        };
    }
}