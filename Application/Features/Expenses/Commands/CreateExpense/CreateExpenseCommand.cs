using Application.Common.Models;
using Application.Features.Expenses.Responses;
using Domain.Entities;
using MediatR;

namespace Application.Features.Expenses.Commands.CreateExpense;

public sealed record CreateExpenseCommand(string Name, decimal Amount, DateTime ExpenseDate, int Category, string? Note = null) : IRequest<Result<ExpenseResponse>>;
