namespace Application.Features.ChatTools.Queries.GetSupplierStatisticsForChat;

public record ChatSupplierStatisticsDto
{
    public int TotalSuppliers { get; init; }

    public int FinancialSuppliers { get; init; }

    public int CreditSuppliers { get; init; }
}
