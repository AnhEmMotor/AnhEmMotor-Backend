namespace Application.Features.ChatTools.Queries.ListFinanceContractsForChat;

public record ChatFinanceContractListItemDto
{
    public Guid Id { get; init; }

    public string ContractNumber { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string? CustomerName { get; init; }

    public string? PartnerName { get; init; }

    public decimal? PrincipalAmount { get; init; }

    public string Currency { get; init; } = "VND";
}
