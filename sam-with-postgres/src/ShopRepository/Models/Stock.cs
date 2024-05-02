using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;

namespace ShopRepository.Models;

[DynamoDBTable("Stock")]
public class Stock
{
    [DynamoDBHashKey] public Guid Id { get; set; } = Guid.Empty;
    
    [DynamoDBProperty] 
    public string? StripeId { get; set; }
    [DynamoDBProperty]
    public string? Name { get; set; }
    [DynamoDBProperty]
    public int? TotalStock { get; set; }

    [DynamoDBProperty] public IEnumerable<string> PhotoUri { get; set; } = new List<string>();
}