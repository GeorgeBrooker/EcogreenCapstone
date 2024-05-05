namespace ShopRepository.Dtos;

public class StockInput
{
    public required string Name { get; set; }
    public string? StripeId { get; set; }
    public int? TotalStock { get; set; }
    public string PhotoUri { get; set; } = "";
    public required bool CreateWithoutStripeLink { get; set; } = false;
}