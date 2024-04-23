using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;

namespace ShopRepository.Models;

[DynamoDBTable("Customers")]
public class Customer
{
    [DynamoDBHashKey] public string Id{ get; set; }

    [DynamoDBProperty]
    public string FirstName { get; set; }
    [DynamoDBProperty]
    public string LastName { get; set; }
    [DynamoDBProperty]
    public string Email { get; set; }
    // stored as bcrypt hash
    [DynamoDBProperty]
    private string Password { get; set; }
}