namespace Application.Features.ChatTools.Queries.SearchSuppliersForChat;

public record ChatSupplierSearchResultDto
{
    public int SupplierId { get; init; }

    public string SupplierName { get; init; } = string.Empty;

    public string? Phone { get; init; }

    public string? Email { get; init; }

    public string? Address { get; init; }
}
