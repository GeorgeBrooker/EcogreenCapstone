using System.ComponentModel.DataAnnotations;

namespace ShopRepository.Dtos;

public class StockInput
{
    public required string Name { get; set; }
    public string? StripeId { get; set; }
    public int? TotalStock { get; set; }
    public IEnumerable<string> PhotoUri { get; set; } = new List<string>();
    public required bool CreateWithoutStripeLink { get; set; } = false;
}