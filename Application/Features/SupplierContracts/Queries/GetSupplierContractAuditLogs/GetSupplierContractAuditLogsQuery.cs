using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.SupplierContracts.Queries.GetSupplierContractAuditLogs;

public class GetSupplierContractAuditLogsQuery : IRequest<Result<List<SupplierContractAuditLogResponse>>>
{
    public Guid SupplierContractId { get; set; }
}
