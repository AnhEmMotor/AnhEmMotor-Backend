namespace Application.Features.ChatTools.Queries.ListSupplierContractsForChat;

public record ChatSupplierContractListItemDto
{
    public Guid Id { get; init; }

    public string ContractNumber { get; init; } = string.Empty;

    public string? SupplierName { get; init; }

    public string Status { get; init; } = string.Empty;

    public decimal ContractValue { get; init; }

    public string Currency { get; init; } = "VND";

    public DateTime EffectiveDate { get; init; }

    public DateTime? ExpirationDate { get; init; }
}
