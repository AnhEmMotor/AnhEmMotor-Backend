using Application.ApiContracts.FinanceContract.Requests;
using Application.ApiContracts.FinanceContract.Responses;
using MediatR;

namespace Application.Features.FinanceContracts.Queries.GetFinanceContractDetail;

public sealed record GetFinanceContractDetailQuery(GetFinanceContractDetailRequest Request, Guid CurrentUserId) : IRequest<FinanceContractDetailResponse>;

