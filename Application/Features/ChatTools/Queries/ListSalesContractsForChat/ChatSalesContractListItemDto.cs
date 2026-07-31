namespace Application.Features.ChatTools.Queries.ListSalesContractsForChat;

public record ChatSalesContractListItemDto
{
    public Guid Id { get; init; }

    public string ContractNumber { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string? CustomerFullName { get; init; }

    public string CustomerPhone { get; init; } = string.Empty;

    public string VehicleModel { get; init; } = string.Empty;

    public decimal ActualSalePrice { get; init; }

    public string Currency { get; init; } = "VND";

    public DateTimeOffset? SignedDate { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}
