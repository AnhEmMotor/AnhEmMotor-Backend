namespace Application.Features.ChatTools.Queries.GetDebtLogsMissingProofsForChat;

public record ChatDebtLogMissingProofItemDto
{
    public int Id { get; init; }

    public int SupplierId { get; init; }

    public string? SupplierName { get; init; }

    public decimal AmountPaid { get; init; }

    public decimal RemainingDebt { get; init; }

    public DateTimeOffset PaymentDate { get; init; }
}
