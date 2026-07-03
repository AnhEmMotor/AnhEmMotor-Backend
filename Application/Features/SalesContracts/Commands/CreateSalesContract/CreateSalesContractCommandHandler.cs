using Application.ApiContracts.SalesContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
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
    IOutputReadRepository orderReadRepo,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateSalesContractCommand, Result<SalesContractResponse>>
{
    public async Task<Result<SalesContractResponse>> Handle(
        CreateSalesContractCommand request,
        CancellationToken cancellationToken)
    {
        var order = await orderReadRepo.GetByIdWithDetailsAsync(request.OrderId, cancellationToken)
            .ConfigureAwait(false);
        if (order == null)
            return Result<SalesContractResponse>.Failure("Không tìm thấy đơn hàng tương ứng.");
        var contractNumber = $"HDMB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8]}";
        var entity = request.Adapt<SalesContract>();
        entity.ContractNumber = contractNumber;
        entity.Status = SalesContractStatus.Draft;
        entity.CustomerId = order.BuyerId;
        entity.CustomerFullName = order.CustomerName;
        entity.CustomerAddress = order.CustomerAddress;
        entity.CustomerPhone = order.CustomerPhone;
        entity.ShowroomName = "Anh Em Motor - Head Office";
        entity.ShowroomTaxCode = "0109876543";
        entity.ShowroomAddress = "123 Đường Láng, Láng Thượng, Đống Đa, Hà Nội";
        entity.ShowroomRepresentative = "Nguyễn Văn A - Giám đốc";
        var outputInfo = order.OutputInfos?.FirstOrDefault();
        if (outputInfo != null)
        {
            entity.VehicleModel = outputInfo.ProductVariant?.Product?.Name;
            entity.VehicleVersion = outputInfo.ProductVariant?.VariantName;
            entity.VehicleColor = outputInfo.ProductVariantColor?.ColorName;
            var vehicle = outputInfo.Vehicles?.FirstOrDefault();
            if (vehicle != null)
            {
                entity.FrameNumber = vehicle.VinNumber;
                entity.EngineNumber = vehicle.EngineNumber;
            }
        }
        entity.ActualSalePrice = order.Total;
        entity.DepositAmount = order.DepositAmount;
        entity.RemainingAmount = order.Total - order.DepositAmount;
        entity.FinalPaymentDeadline = DateTimeOffset.UtcNow.AddDays(7);
        insertRepo.Add(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var created = await readRepo.GetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
        if (created == null)
            return Result<SalesContractResponse>.Failure("Không thể tạo hợp đồng.");
        return Result<SalesContractResponse>.Success(created.Adapt<SalesContractResponse>());
    }
}
