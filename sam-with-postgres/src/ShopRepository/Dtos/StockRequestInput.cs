namespace ShopRepository.Dtos;

public class StockRequestInput
{
    public Guid ProductId { get; set; }
    public long Quantity { get; set; }
    //TODO TEMP REMOVING REQUIRED FOR ALL ABOVE INPUTS.
    public Guid OrderId { get; set; } // Not required on input as this is assigned when a new order is created.
    public decimal? Subtotal { get; set; } // Not required on input as this is calculated from the stock price and quantity.
    public string? ProductName { get; set; }
}