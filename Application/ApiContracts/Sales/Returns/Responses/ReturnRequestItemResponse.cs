namespace Application.ApiContracts.Sales.Returns.Responses;

public class ReturnRequestItemResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int Quantity { get; set; }
    public int ReturnQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}
