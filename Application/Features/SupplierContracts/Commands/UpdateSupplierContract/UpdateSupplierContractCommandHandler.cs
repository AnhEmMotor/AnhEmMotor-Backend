using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.SupplierContract;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using Mapster;

namespace Application.Features.SupplierContracts.Commands.UpdateSupplierContract;

public sealed class UpdateSupplierContractCommandHandler(
    ISupplierContractReadRepository readRepo,
    ISupplierContractUpdateRepository updateRepo,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateSupplierContractCommand, Result<SupplierContractResponse>>
{
    public async Task<Result<SupplierContractResponse>> Handle(UpdateSupplierContractCommand request, CancellationToken cancellationToken)
    {
        var entity = await readRepo.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (entity == null)
        {
            return Result<SupplierContractResponse>.Failure("Supplier contract not found.");
        }

        bool isCurrentlyActive = entity.Status == SupplierContractStatus.Active;
        bool statusBeingChanged = !string.IsNullOrWhiteSpace(request.Request.Status) && request.Request.Status != entity.Status;

        if (isCurrentlyActive && !statusBeingChanged)
        {
            if ((request.Request.CreditLimit.HasValue && request.Request.CreditLimit != entity.CreditLimit) ||
                (request.Request.DiscountRate.HasValue && request.Request.DiscountRate != entity.DiscountRate) ||
                (request.Request.ContractItems != null && request.Request.ContractItems.Any()))
            {
                return Result<SupplierContractResponse>.Failure("Cannot modify financial terms of an active contract. Please create an addendum.");
            }
        }

        var auditLogs = new List<SupplierContractAuditLog>();

        if (!string.IsNullOrWhiteSpace(request.Request.ContractNumber) && request.Request.ContractNumber != entity.ContractNumber)
        {
            var isDuplicate = await readRepo.IsContractNumberExistsAsync(request.Request.ContractNumber, request.Id, cancellationToken)
            .ConfigureAwait(false);
            if (isDuplicate)
            {
                return Result<SupplierContractResponse>.Failure("Contract number already exists.");
            }
            auditLogs.Add(new SupplierContractAuditLog
            {
                Action = "Update",
                Details = $"Contract number changed from {entity.ContractNumber} to {request.Request.ContractNumber}",
                ChangedBy = "system",
                OldValue = entity.ContractNumber,
                NewValue = request.Request.ContractNumber
            });
            entity.ContractNumber = request.Request.ContractNumber;
        }

        if (request.Request.SupplierId.HasValue) entity.SupplierId = request.Request.SupplierId;
        if (request.Request.ContractFilePath != null) entity.ContractFilePath = request.Request.ContractFilePath;
        if (request.Request.EffectiveDate != default) entity.EffectiveDate = request.Request.EffectiveDate;
        if (request.Request.ExpirationDate.HasValue) entity.ExpirationDate = request.Request.ExpirationDate;
        if (request.Request.ContractValue > 0) entity.ContractValue = request.Request.ContractValue;

        if (!string.IsNullOrWhiteSpace(request.Request.Status) && SupplierContractStatus.IsValid(request.Request.Status))
        {
            if (request.Request.Status != entity.Status)
            {
                auditLogs.Add(new SupplierContractAuditLog
                {
                    Action = "StatusChange",
                    Details = $"Status changed from {entity.Status} to {request.Request.Status}",
                    ChangedBy = "system",
                    OldValue = entity.Status,
                    NewValue = request.Request.Status
                });
            }
            entity.Status = request.Request.Status;
        }

        if (request.Request.Terms != null) entity.Terms = request.Request.Terms;
        if (request.Request.Note != null) entity.Note = request.Request.Note;

        if (request.Request.CreditLimit.HasValue && request.Request.CreditLimit != entity.CreditLimit)
        {
            auditLogs.Add(new SupplierContractAuditLog
            {
                Action = "Update",
                Details = "Credit limit changed",
                ChangedBy = "system",
                OldValue = entity.CreditLimit?.ToString(),
                NewValue = request.Request.CreditLimit.ToString()
            });
            entity.CreditLimit = request.Request.CreditLimit;
        }
        if (request.Request.PaymentWindowDays.HasValue) entity.PaymentWindowDays = request.Request.PaymentWindowDays;
        if (request.Request.BankAccountNumber != null) entity.BankAccountNumber = request.Request.BankAccountNumber;
        if (request.Request.BankName != null) entity.BankName = request.Request.BankName;
        if (request.Request.MinimumVolumePerMonth.HasValue) entity.MinimumVolumePerMonth = request.Request.MinimumVolumePerMonth;
        if (request.Request.DiscountRate.HasValue && request.Request.DiscountRate != entity.DiscountRate)
        {
            auditLogs.Add(new SupplierContractAuditLog
            {
                Action = "Update",
                Details = "Discount rate changed",
                ChangedBy = "system",
                OldValue = entity.DiscountRate?.ToString(),
                NewValue = request.Request.DiscountRate.ToString()
            });
            entity.DiscountRate = request.Request.DiscountRate;
        }
        if (request.Request.ParentContractId.HasValue) entity.ParentContractId = request.Request.ParentContractId;

        if (request.Request.ContractItems != null && (!isCurrentlyActive || statusBeingChanged))
        {
            entity.ContractItems.Clear();
            foreach (var item in request.Request.ContractItems)
            {
                entity.ContractItems.Add(new SupplierContractItem
                {
                    ProductVariantId = item.ProductVariantId,
                    WholesalePrice = item.WholesalePrice
                });
            }
            auditLogs.Add(new SupplierContractAuditLog
            {
                Action = "Update",
                Details = $"Updated SKU price list ({request.Request.ContractItems.Count} items)",
                ChangedBy = "system"
            });
        }

        foreach (var log in auditLogs)
        {
            log.SupplierContractId = entity.Id;
            entity.AuditLogs.Add(log);
        }

        updateRepo.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Adapt<SupplierContractResponse>();
    }
}
