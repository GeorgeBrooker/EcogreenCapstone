using System.ComponentModel.DataAnnotations;

namespace ShopRepository.Dtos;

public class CustomerInput
{
    [Required] public string Fname { get; set; } 
    [Required] public string Lname { get; set; }
    [Required] public string Email { get; set; }
    [Required] public string Pass { get; set; }
}