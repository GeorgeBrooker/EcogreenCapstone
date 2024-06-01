using Amazon.DynamoDBv2.DataModel;

namespace ShopRepository.Models;

[DynamoDBTable("Addresses")]
public class Address
{
    [DynamoDBHashKey] public Guid CustomerId { get; set; }
    [DynamoDBRangeKey] public required string AddressName { get; set; } = "Default";
    [DynamoDBProperty] public required string Line1 { get; set; }
    [DynamoDBProperty] public required string Line2 { get; set; }
    [DynamoDBProperty] public required string City { get; set; }
    [DynamoDBProperty] public required string PostCode { get; set; }
    [DynamoDBProperty] public required string Country { get; set; } = "New Zealand";
    [DynamoDBProperty] public string? Email { get; set; }
}