namespace Application.Features.ChatTools.Queries.ListExpensesForChat;

public record ChatExpenseListItemDto
{
    public string Name { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string Currency { get; init; } = "VND";

    public DateTime ExpenseDate { get; init; }

    public string CategoryText { get; init; } = string.Empty;
}
