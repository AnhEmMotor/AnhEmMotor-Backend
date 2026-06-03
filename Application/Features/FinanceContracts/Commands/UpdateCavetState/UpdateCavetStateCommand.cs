using Application.ApiContracts.FinanceContract.Requests;
using MediatR;

namespace Application.Features.FinanceContracts.Commands.UpdateCavetState;

public sealed record UpdateCavetStateCommand(Guid FinanceContractId, UpdateCavetStateRequest Request, Guid CurrentUserId)
    : IRequest;

