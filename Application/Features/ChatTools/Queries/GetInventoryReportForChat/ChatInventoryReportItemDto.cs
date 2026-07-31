namespace Application.Features.ChatTools.Queries.GetInventoryReportForChat;

public record ChatInventoryReportItemDto
{
    public string ProductName { get; init; } = string.Empty;

    public string? VariantName { get; init; }

    public string? ColorName { get; init; }

    public int StockQty { get; init; }

    public int ImportedQty { get; init; }

    public int ExportedQty { get; init; }

    public string Status { get; init; } = string.Empty;
}
