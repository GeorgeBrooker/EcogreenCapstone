using System.ComponentModel.DataAnnotations;

namespace ShopRepository.Models;

public class Photo
{
    [Key] public string ImageUri { get; set; }

    public int? SizeW { get; set; }
    public int? SizeH { get; set; }
    public bool Hd { get; set; }
}