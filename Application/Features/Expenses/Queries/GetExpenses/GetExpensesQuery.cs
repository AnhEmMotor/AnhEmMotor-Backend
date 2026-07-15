using Application.Common.Models;
using Application.Features.Expenses.Responses;
using MediatR;
using Sieve.Models;
using Domain.Primitives;

public sealed record GetExpensesQuery(SieveModel SieveModel) : IRequest<Result<PagedResult<ExpenseResponse>>>;
