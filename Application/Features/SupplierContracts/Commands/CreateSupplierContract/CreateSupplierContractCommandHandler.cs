using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.SupplierContract;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.SupplierContracts.Commands.CreateSupplierContract;

public class CreateSupplierContractCommandHandler(
    ISupplierContractReadRepository readRepo,
    ISupplierContractInsertRepository insertRepo,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateSupplierContractCommand, Result<SupplierContractResponse>>
{
    public async Task<Result<SupplierContractResponse>> Handle(
        CreateSupplierContractCommand request,
        CancellationToken cancellationToken)
    {
        var isDuplicate = await readRepo.IsContractNumberExistsAsync(request.ContractNumber, null, cancellationToken)
            .ConfigureAwait(false);
        if (isDuplicate)
        {
            return Result<SupplierContractResponse>.Failure("Contract number already exists.");
        }
        var entity = request.Adapt<SupplierContract>();
        if (request.ContractItems != null && request.ContractItems.Any())
        {
            entity.ContractItems = request.ContractItems
                .Select(
                    item => new SupplierContractItem
                    {
                        ProductVariantId = item.ProductVariantId,
                        WholesalePrice = item.WholesalePrice
                    })
                .ToList();
        }
        if (entity.AuditLogs == null)
            entity.AuditLogs = [];
        entity.AuditLogs
            .Add(
                new SupplierContractAuditLog
                {
                    Action = "Create",
                    Details = $"Created contract {request.ContractNumber}",
                    ChangedBy = "system",
                    IpAddress = null
                });
        insertRepo.Add(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var created = await readRepo.GetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
        return created!.Adapt<SupplierContractResponse>();
    }
}
