using Amazon.DynamoDBv2.DataModel;

namespace ShopRepository.Models;

[DynamoDBTable("Customers")]
public class Customer
{
    [DynamoDBHashKey] public Guid Id { get; set; } = Guid.Empty;

    [DynamoDBProperty] public string? StripeId { get; set; }

    [DynamoDBProperty] public required string FirstName { get; set; }

    [DynamoDBProperty] public required string LastName { get; set; }

    [DynamoDBProperty] public required string Email { get; set; }

    // stored as a hash using PasswordHasher
    [DynamoDBProperty] public required string Password { get; set; }
}