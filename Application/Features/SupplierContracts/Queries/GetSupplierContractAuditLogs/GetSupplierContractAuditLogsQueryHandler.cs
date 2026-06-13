using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.SupplierContract;
using Domain.Constants;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.SupplierContracts.Queries.GetSupplierContractAuditLogs;

public class GetSupplierContractAuditLogsQueryHandler(
    ISupplierContractReadRepository repository) : IRequestHandler<GetSupplierContractAuditLogsQuery, Result<List<SupplierContractAuditLogResponse>>>
{
    public async Task<Result<List<SupplierContractAuditLogResponse>>> Handle(
        GetSupplierContractAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var contract = await repository.GetByIdAsync(
            request.SupplierContractId,
            cancellationToken,
            DataFetchMode.All).ConfigureAwait(false);

        if (contract == null)
        {
            return Result<List<SupplierContractAuditLogResponse>>.Failure("Supplier contract not found.");
        }

        var auditLogs = await repository.GetAuditLogsAsync(request.SupplierContractId, cancellationToken).ConfigureAwait(false);

        var result = auditLogs
            .OrderByDescending(al => al.CreatedAt)
            .Adapt<List<SupplierContractAuditLogResponse>>();

        return Result<List<SupplierContractAuditLogResponse>>.Success(result);
    }
}
