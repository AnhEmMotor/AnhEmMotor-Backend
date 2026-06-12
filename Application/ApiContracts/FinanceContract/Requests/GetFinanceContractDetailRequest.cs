using Application.ApiContracts.FinanceContract.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.ApiContracts.FinanceContract.Requests;

public sealed record GetFinanceContractDetailRequest(Guid Id) : IRequest<Result<FinanceContractDetailResponse>>;
