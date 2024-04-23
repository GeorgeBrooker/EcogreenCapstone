using System.ComponentModel.DataAnnotations;

namespace ShopRepository.Models;

public class Stock
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public string Manufacturer { get; set; }
    public float Price { get; set; }
    public float BaseCost { get; set; }
    public float Profit => Price - BaseCost;
    
    public IEnumerable<Photo> photos { get; set; }
}