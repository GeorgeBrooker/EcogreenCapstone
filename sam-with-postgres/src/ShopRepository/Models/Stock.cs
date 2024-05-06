using Amazon.DynamoDBv2.DataModel;

namespace ShopRepository.Models;

[DynamoDBTable("Stock")]
public class Stock
{
    [DynamoDBHashKey] public Guid Id { get; set; } = Guid.Empty;

    [DynamoDBProperty] public string? StripeId { get; set; }

    [DynamoDBProperty] public string? Name { get; set; }
    [DynamoDBProperty] public long? TotalStock { get; set; }

    [DynamoDBProperty] public string PhotoUri { get; set; } = "";
}