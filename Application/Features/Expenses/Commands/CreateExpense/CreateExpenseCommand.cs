using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Features.Expenses.Commands.CreateExpense;

public sealed record CreateExpenseCommand(string Name, decimal Amount, DateTime ExpenseDate, int Category, string? Note = null)
    : IRequest<Result<ExpenseResponse>>;

public class ExpenseResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public int Category { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
