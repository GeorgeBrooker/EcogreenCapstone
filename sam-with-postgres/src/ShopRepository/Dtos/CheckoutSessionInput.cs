using System.ComponentModel.DataAnnotations;

namespace ShopRepository.Dtos;

public class CheckoutSessionInput
{
    public OrderInput Order { get; set; }
    public StockRequestInput[] StockRequests { get; set; }
    //TODO temp removed all requred tags
}