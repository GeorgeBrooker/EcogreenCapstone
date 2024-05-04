using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ShopRepository.Dtos;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class CustomerInput
{
    [Required] public required string Fname { get; set; }
    [Required] public required string Lname { get; set; }
    [Required] public required string Email { get; set; }
    [Required] public required string Pass { get; set; }
}