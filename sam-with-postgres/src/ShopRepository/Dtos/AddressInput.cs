namespace ShopRepository.Dtos;

public class AddressInput
{
    public required Guid CustomerId { get; set; }
    public required string AddressName { get; set; } = "Default";
    public required string StreetNumber { get; set; }
    public required string Street { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string PostCode { get; set; }
    public required string Country { get; set; } = "New Zealand";
    public required string PhoneNumber { get; set; }
    public string? Email { get; set; }
}