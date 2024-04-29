using Amazon.DynamoDBv2.DataModel;

namespace ShopRepository.Models;

[DynamoDBTable("StockRequests")]
public class StockRequest
{
    [DynamoDBHashKey] public Guid Id { get; set; } = Guid.NewGuid();

    [DynamoDBProperty] 
    public Guid? ProductId { get; set; }
    [DynamoDBProperty] 
    public int? Quantity { get; set; }
    
    // Parent order
    [DynamoDBProperty] 
    public Guid? OrderId { get; set; }
}