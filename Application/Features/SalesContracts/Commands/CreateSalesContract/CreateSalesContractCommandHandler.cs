using Application.ApiContracts.SalesContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Invoice;
using Application.Interfaces.Repositories.SalesContract;
using Domain.Constants;
using Domain.Entities;
using Mapster;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.SalesContracts.Commands.CreateSalesContract;

public class CreateSalesContractCommandHandler(
    ISalesContractReadRepository readRepo,
    ISalesContractInsertRepository insertRepo,
    IInvoiceReadRepository invoiceReadRepo,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateSalesContractCommand, Result<SalesContractResponse>>
{
    public async Task<Result<SalesContractResponse>> Handle(
        CreateSalesContractCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = await invoiceReadRepo.GetByIdAsync(request.InvoiceId, cancellationToken)
            .ConfigureAwait(false);
        if (invoice == null)
            return Result<SalesContractResponse>.Failure("Không tìm thấy hóa đơn tương ứng.");

        if (!string.Equals(invoice.Status, "completed", StringComparison.OrdinalIgnoreCase))
            return Result<SalesContractResponse>.Failure(
                "Chỉ có thể tạo hợp đồng từ hóa đơn đã được duyệt/hoàn tất.");

        var existingContract = await readRepo.GetByInvoiceIdAsync(request.InvoiceId, cancellationToken)
            .ConfigureAwait(false);
        if (existingContract != null)
            return Result<SalesContractResponse>.Failure(
                $"Hóa đơn #{request.InvoiceId} đã được liên kết với hợp đồng {existingContract.ContractNumber}.");
        
        var contractNumber = $"HDMB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8]}";
        var entity = request.Adapt<SalesContract>();
        
        entity.InvoiceId = invoice.Id;
        entity.ContractNumber = contractNumber;
        entity.Status = SalesContractStatus.Draft;
        entity.CustomerId = invoice.UserId; // This might be empty, but we can set it
        entity.CustomerFullName = invoice.CustomerName;
        entity.CustomerCCCD = invoice.CustomerIdCard;
        entity.CustomerAddress = invoice.CustomerAddress;
        entity.CustomerPhone = invoice.CustomerPhone;
        entity.ShowroomName = "Anh Em Motor - Head Office";
        entity.ShowroomTaxCode = "0109876543";
        entity.ShowroomAddress = "123 Đường Láng, Láng Thượng, Đống Đa, Hà Nội";
        entity.ShowroomRepresentative = "Nguyễn Văn A - Giám đốc";
        
        entity.VehicleModel = invoice.VehicleModel;
        entity.VehicleVersion = invoice.VehicleVersion;
        entity.VehicleColor = invoice.VehicleColor;
        entity.FrameNumber = invoice.ChassisNo;
        entity.EngineNumber = invoice.EngineNo;
        
        entity.ActualSalePrice = invoice.VehiclePrice; // Or invoice.TotalAmount depending on logic
        var depositPercent = invoice.DepositPercentage ?? 100;
        entity.DepositAmount = (invoice.TotalAmount * depositPercent) / 100;
        entity.RemainingAmount = invoice.TotalAmount - entity.DepositAmount;
        
        entity.FinalPaymentDeadline = DateTimeOffset.UtcNow.AddDays(7);
        
        insertRepo.Add(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        
        var created = await readRepo.GetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
        if (created == null)
            return Result<SalesContractResponse>.Failure("Không thể tạo hợp đồng.");
            
        return Result<SalesContractResponse>.Success(created.Adapt<SalesContractResponse>());
    }
}
