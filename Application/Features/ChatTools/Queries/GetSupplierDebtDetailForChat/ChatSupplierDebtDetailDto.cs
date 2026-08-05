namespace Application.Features.ChatTools.Queries.GetSupplierDebtDetailForChat;

public record ChatSupplierDebtDetailDto
{
    public int LogId { get; init; }

    public DateTimeOffset PaymentDate { get; init; }

    public decimal AmountPaid { get; init; }

    public decimal RemainingDebt { get; init; }

    public string Currency { get; init; } = "VND";
}
