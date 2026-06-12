using Application.Common.Models;
using Application.Interfaces.Repositories.SalesContract;
using Domain.Entities;
using MediatR;

namespace Application.Features.SalesContracts.Commands.DeleteSalesContract;

public sealed record DeleteSalesContractCommand(Guid Id) : IRequest<Result>;
