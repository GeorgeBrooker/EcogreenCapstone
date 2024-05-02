using Amazon.DynamoDBv2.DataModel;

namespace ShopRepository.Models;

[DynamoDBTable("Customers")]
public class Customer
{
    [DynamoDBHashKey] public Guid Id { get; set; } = Guid.Empty;

    [DynamoDBProperty] public string? StripeId { get; set; }

    [DynamoDBProperty] public string? FirstName { get; set; }

    [DynamoDBProperty] public string? LastName { get; set; }

    [DynamoDBProperty] public string? Email { get; set; }

    // stored as a hash using PasswordHasher
    [DynamoDBProperty] public string? Password { get; set; }
}