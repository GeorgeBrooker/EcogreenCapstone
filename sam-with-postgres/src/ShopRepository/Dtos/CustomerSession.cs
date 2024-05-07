using System.ComponentModel.DataAnnotations;

namespace ShopRepository.Dtos;

public class CustomerSession
{
    [Required] public required string Fname { get; set; }
    [Required] public required string Lname { get; set; }
    [Required] public required string Email { get; set; }
    [Required] public required string Id { get; set; }
}