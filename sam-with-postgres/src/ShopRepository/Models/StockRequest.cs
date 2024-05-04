using Amazon.DynamoDBv2.DataModel;

namespace ShopRepository.Models;

[DynamoDBTable("StockRequests")]
public class StockRequest
{
    // Weak child entity of orderId. Partitioned on orderId and sorted by product type.
    [DynamoDBHashKey] public Guid OrderId { get; set; }
    [DynamoDBRangeKey] public Guid ProductId { get; set; }

    [DynamoDBProperty] public int Quantity { get; set; }
}