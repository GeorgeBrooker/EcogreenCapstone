using Amazon.DynamoDBv2.DataModel;

namespace ShopRepository.Models;

[DynamoDBTable("Addresses")]
public class Address
{
    [DynamoDBHashKey] public Guid CustomerId { get; set; }
    [DynamoDBRangeKey] public required string AddressName { get; set; } = "Default";
    [DynamoDBProperty] public required string StreetNumber { get; set; }
    [DynamoDBProperty] public required string Street { get; set; }
    [DynamoDBProperty] public required string City { get; set; }
    [DynamoDBProperty] public required string PostCode { get; set; }
    [DynamoDBProperty] public required string Country { get; set; } = "New Zealand";
    [DynamoDBProperty] public required string PhoneNumber { get; set; }
    [DynamoDBProperty] public string? Email { get; set; }
}