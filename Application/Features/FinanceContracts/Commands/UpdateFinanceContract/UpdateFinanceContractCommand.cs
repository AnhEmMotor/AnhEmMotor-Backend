using Application.ApiContracts.FinanceContract.Requests;
using Application.Common.Models;
using MediatR;

namespace Application.Features.FinanceContracts.Commands.UpdateFinanceContract;

public sealed record UpdateFinanceContractCommand(Guid Id, UpdateFinanceContractRequest Request, Guid CurrentUserId) : IRequest<Result<Guid>>;
