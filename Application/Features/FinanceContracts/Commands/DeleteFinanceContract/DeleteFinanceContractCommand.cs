using Application.Common.Models;
using MediatR;

namespace Application.Features.FinanceContracts.Commands.DeleteFinanceContract;

public sealed record DeleteFinanceContractCommand(Guid Id) : IRequest<Result>;
