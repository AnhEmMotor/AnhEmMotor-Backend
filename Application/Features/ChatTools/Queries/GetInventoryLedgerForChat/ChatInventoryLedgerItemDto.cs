namespace Application.Features.ChatTools.Queries.GetInventoryLedgerForChat;

public record ChatInventoryLedgerItemDto
{
    public int Id { get; init; }

    public DateTimeOffset Date { get; init; }

    public string VoucherCode { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public string ProductName { get; init; } = string.Empty;

    public string VariantName { get; init; } = string.Empty;

    public string? ColorName { get; init; }

    public int ImportQty { get; init; }

    public int ExportQty { get; init; }

    public decimal UnitPrice { get; init; }

    public decimal TotalAmount { get; init; }

    public int Balance { get; init; }

    public string Currency { get; init; } = "VND";
}
