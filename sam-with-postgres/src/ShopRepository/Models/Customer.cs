using System.ComponentModel.DataAnnotations;

namespace ShopRepository.Models;

public class Customer
{
    [Key]
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    // Add other properties for delivery info (country, organization, etc.)
    public string PasswordHash { get; set; }
    public string Email { get; set; }

    public IEnumerable<Order> orders { get; set; }
}