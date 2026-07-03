using Application.Common.Models;
using MediatR;

namespace Application.Features.Expenses.Commands.DeleteExpense;

public sealed record DeleteExpenseCommand(int Id) : IRequest<Result<bool>>;
