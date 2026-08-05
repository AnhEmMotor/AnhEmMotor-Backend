namespace Application.Features.ChatTools.Queries.GetProductStockForChat;

public record ChatProductStockPublicDto
{
    private const int LowStockThreshold = 5;

    public int VariantId { get; init; }

    public string? VariantName { get; init; }

    public string StockStatus { get; init; } = string.Empty;

    public static ChatProductStockPublicDto FromInternal(ChatProductStockDto dto) => new()
    {
        VariantId = dto.VariantId,
        VariantName = dto.VariantName,
        StockStatus =
            dto.StockQuantity <= 0 ? "het_hang" : dto.StockQuantity <= LowStockThreshold ? "sap_het" : "con_hang"
    };
}
