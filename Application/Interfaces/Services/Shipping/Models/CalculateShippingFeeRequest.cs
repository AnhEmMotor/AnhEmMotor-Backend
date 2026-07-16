namespace Application.Interfaces.Services.Shipping.Models;

public class CalculateShippingFeeRequest
{
    public int ToWardIdV2 { get; set; }
    public string ToAddressV2 { get; set; } = string.Empty;
    public bool IsNewToAddress { get; set; } = true;
    public string ToWardCode { get; set; } = string.Empty;
    public List<ShippingItemDto> Items { get; set; } = new();
}

public class ShippingItemDto
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int? Length { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Weight { get; set; }
}
