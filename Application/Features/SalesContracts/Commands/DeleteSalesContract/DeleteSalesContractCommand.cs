using Application.Common.Models;
using MediatR;

namespace Application.Features.SalesContracts.Commands.DeleteSalesContract;

public sealed record DeleteSalesContractCommand(Guid Id) : IRequest<Result>;
