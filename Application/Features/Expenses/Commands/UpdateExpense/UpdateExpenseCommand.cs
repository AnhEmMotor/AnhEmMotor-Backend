using Application.Common.Models;
using Application.Features.Expenses.Responses;
using MediatR;

namespace Application.Features.Expenses.Commands.UpdateExpense;

public sealed record UpdateExpenseCommand(
	int Id,
	string Name,
	decimal Amount,
	DateTime ExpenseDate,
	int Category,
	string? Note = null) : IRequest<Result<ExpenseResponse>>;
