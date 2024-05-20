namespace ShopRepository.Dtos;

public class StockInput
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public int DiscountPercentage { get; set; } = 0;
    public string? StripeId { get; set; }
    public int? TotalStock { get; set; }

    public string PhotoUri { get; set; } =
        "https://kashish-web-asset-bucket.s3.ap-southeast-2.amazonaws.com/stock-photos/";

    public required bool CreateWithoutStripeLink { get; init; } = false;
}