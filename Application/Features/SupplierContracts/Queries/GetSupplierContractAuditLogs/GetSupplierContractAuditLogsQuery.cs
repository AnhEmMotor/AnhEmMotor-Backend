using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.SupplierContract;
using Domain.Entities;
using MediatR;

namespace Application.Features.SupplierContracts.Queries.GetSupplierContractAuditLogs;

public sealed class GetSupplierContractAuditLogsQuery : IRequest<Result<List<SupplierContractAuditLogResponse>>>
{
    public Guid SupplierContractId { get; set; }
}
