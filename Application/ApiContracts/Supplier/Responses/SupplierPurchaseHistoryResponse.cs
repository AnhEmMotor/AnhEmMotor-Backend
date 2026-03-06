namespace Application.ApiContracts.Supplier.Responses;

public sealed class SupplierPurchaseHistoryResponse
{
    public int Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public decimal TotalPayable { get; set; }

    public string StatusId { get; set; } = string.Empty;

    public int TotalItems { get; set; }
}
