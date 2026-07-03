namespace Application.Features.Expenses.Responses;

public class ExpenseResponse
{
public int Id { get; set; }
public string Name { get; set; } = string.Empty;
public decimal Amount { get; set; }
public DateTime ExpenseDate { get; set; }
public int Category { get; set; }
public string? Note { get; set; }
public DateTime CreatedAt { get; set; }
public string CategoryText { get; set; } = string.Empty;
}
