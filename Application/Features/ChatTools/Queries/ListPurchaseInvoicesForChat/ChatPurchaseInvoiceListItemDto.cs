namespace Application.Features.ChatTools.Queries.ListPurchaseInvoicesForChat;

public record ChatPurchaseInvoiceListItemDto
{
    public string InvoiceNumber { get; init; } = string.Empty;

    public string? SupplierName { get; init; }

    public decimal TotalAmount { get; init; }

    public string Currency { get; init; } = "VND";

    public string Status { get; init; } = string.Empty;

    public string? PaymentStatus { get; init; }

    public DateTimeOffset InvoiceDate { get; init; }
}
