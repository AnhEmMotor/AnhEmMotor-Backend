using Application.ApiContracts.SalesContracts.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.SalesContracts.Commands.UpdateSalesContractStatus;

public sealed record UpdateSalesContractStatusCommand(Guid ContractId, string Status, string? RejectReason = null, bool IsAdminApproval = false) : IRequest<Result<SalesContractResponse>>;
