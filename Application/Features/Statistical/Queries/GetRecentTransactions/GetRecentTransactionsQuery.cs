using Application.ApiContracts.Statistical.Responses;
using MediatR;

namespace Application.Features.Statistical.Queries.GetRecentTransactions;

public sealed record GetRecentTransactionsQuery(int Limit = 50) : IRequest<List<TransactionLogResponse>>;
