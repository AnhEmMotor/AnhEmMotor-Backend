using Application.Common.Models;
using MediatR;

namespace Application.Features.SupplierContracts.Commands.RestoreSupplierContract;

public sealed record RestoreSupplierContractCommand(Guid Id) : IRequest<Result>;
