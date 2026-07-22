using Application.Common.Models;
using Application.Features.Expenses.Responses;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

public sealed record GetExpensesQuery(SieveModel SieveModel) : IRequest<Result<PagedResult<ExpenseResponse>>>;
