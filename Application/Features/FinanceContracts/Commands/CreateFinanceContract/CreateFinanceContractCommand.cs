using Application.ApiContracts.FinanceContract.Requests;
using Application.Common.Models;
using MediatR;

namespace Application.Features.FinanceContracts.Commands.CreateFinanceContract;

public sealed record CreateFinanceContractCommand(CreateFinanceContractRequest Request, Guid CurrentUserId)
    : IRequest<Result<Guid>>;
