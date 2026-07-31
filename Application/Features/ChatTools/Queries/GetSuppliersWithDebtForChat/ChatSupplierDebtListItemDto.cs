namespace Application.Features.ChatTools.Queries.GetSuppliersWithDebtForChat;

public record ChatSupplierDebtListItemDto
{
    public int SupplierId { get; init; }

    public string SupplierName { get; init; } = string.Empty;

    public decimal DebtAmount { get; init; }

    public string Currency { get; init; } = "VND";
}
