using Application.Common.Models;
using MediatR;

namespace Application.Features.SupplierContracts.Commands.DeleteSupplierContract;

public sealed record DeleteSupplierContractCommand(Guid Id) : IRequest<Result>;
