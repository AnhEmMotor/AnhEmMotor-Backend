using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Services.HR;
using Domain.Constants;
using Domain.Constants.Order;
using Mapster;
using MediatR;

using Application.Interfaces.Repositories.Vehicle;
using Application.Interfaces.Repositories.Lead.Lead;
using VehicleEntity = Domain.Entities.Vehicle;
using LeadEntity = Domain.Entities.Lead;
using OutputInfoEntity = Domain.Entities.OutputInfo;

namespace Application.Features.Outputs.Commands.UpdateOutputStatus;

public sealed class UpdateOutputStatusCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    ICommissionService commissionService,
    IUnitOfWork unitOfWork,
    IVehicleReadRepository? vehicleReadRepository = null,
    IVehicleUpdateRepository? vehicleUpdateRepository = null,
    ILeadReadRepository? leadReadRepository = null,
    ILeadInsertRepository? leadInsertRepository = null) : IRequestHandler<UpdateOutputStatusCommand, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        UpdateOutputStatusCommand request,
        CancellationToken cancellationToken)
    {
        var output = await readRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        if (output is null)
        {
            return Error.NotFound($"Không tìm thấy đơn hàng có ID {request.Id}.", "Id");
        }
        if (!OrderStatus.IsValid(request.StatusId))
        {
            return Error.BadRequest($"Trạng thái '{request.StatusId}' không hợp lệ.", "StatusId");
        }
        if (!OrderStatusTransitions.IsTransitionAllowed(output.StatusId, request.StatusId))
        {
            var allowed = OrderStatusTransitions.GetAllowedTransitions(output.StatusId);
            return Error.BadRequest(
                $"Không thể chuyển từ '{output.StatusId}' sang '{request.StatusId}'. Chỉ được chuyển sang: {string.Join(", ", allowed)}",
                "StatusId");
        }
        bool isCompleting = false;
        switch (request.StatusId)
        {
            case OrderStatus.Completed:
                isCompleting = true;
                output.FinishedBy = request.CurrentUserId;
                var deductionResult = await updateRepository.HandleInventoryTransactionAsync(
                    output.Id,
                    true,
                    cancellationToken)
                    .ConfigureAwait(false);
                if (deductionResult.IsFailure)
                {
                    return Result<OrderDetailResponse>.Failure(deductionResult.Errors!);
                }
                break;
            case OrderStatus.Delivering:
                // Check vehicle-managed products
                var vehicleOutputInfos = output.OutputInfos
                    .Where(oi => oi.ProductVariant?.Product?.ProductCategory != null &&
                                 string.Equals(oi.ProductVariant.Product.ProductCategory.ManagementType, "vin_number", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (vehicleOutputInfos.Count > 0)
                {
                    if (vehicleReadRepository == null || vehicleUpdateRepository == null || leadReadRepository == null || leadInsertRepository == null)
                    {
                        throw new System.InvalidOperationException("Vehicle/Lead repositories are not injected.");
                    }

                    int requiredVehicleCount = vehicleOutputInfos.Sum(oi => oi.Count ?? 0);
                    if (request.SelectedVehicleIds == null || request.SelectedVehicleIds.Count != requiredVehicleCount)
                    {
                        return Error.BadRequest($"Danh sách xe (SelectedVehicleIds) phải có đúng {requiredVehicleCount} phần tử cho các sản phẩm quản lý theo số khung.", "SelectedVehicleIds");
                    }

                    var vehicles = await vehicleReadRepository.GetByIdsAsync(request.SelectedVehicleIds, cancellationToken).ConfigureAwait(false);
                    if (vehicles.Count != request.SelectedVehicleIds.Count)
                    {
                        return Error.BadRequest("Một hoặc nhiều mã xe không tồn tại trong hệ thống.", "SelectedVehicleIds");
                    }

                    foreach (var v in vehicles)
                    {
                        if (!v.IsActive)
                        {
                            return Error.BadRequest($"Xe có số khung (VIN) {v.VinNumber} đang ở trạng thái không hoạt động.", "SelectedVehicleIds");
                        }
                        if (v.InputInfoId == null)
                        {
                            return Error.BadRequest($"Xe có số khung (VIN) {v.VinNumber} chưa được nhập kho.", "SelectedVehicleIds");
                        }
                        if (v.OutputInfoId != null)
                        {
                            return Error.BadRequest($"Xe có số khung (VIN) {v.VinNumber} đã được bán hoặc xuất kho trước đó.", "SelectedVehicleIds");
                        }
                    }

                    var remainingVehicles = new List<VehicleEntity>(vehicles);
                    var matchedVehiclesMap = new Dictionary<OutputInfoEntity, List<VehicleEntity>>();

                    foreach (var oi in vehicleOutputInfos)
                    {
                        int count = oi.Count ?? 0;
                        var matches = remainingVehicles.Where(v =>
                            v.ProductId == oi.ProductVariant.ProductId &&
                            (!oi.ProductVariantColorId.HasValue || (v.InputInfo != null && v.InputInfo.ProductVariantColorId == oi.ProductVariantColorId))
                        ).Take(count).ToList();

                        if (matches.Count < count)
                        {
                            var colorMsg = oi.ProductVariantColorId.HasValue ? " và màu sắc đã chọn" : "";
                            return Error.BadRequest($"Không tìm thấy đủ xe phù hợp trong danh sách SelectedVehicleIds cho sản phẩm '{oi.ProductVariant.Product.Name}'{colorMsg}. Cần: {count}, tìm thấy: {matches.Count}.", "SelectedVehicleIds");
                        }

                        matchedVehiclesMap[oi] = matches;
                        foreach (var m in matches)
                        {
                            remainingVehicles.Remove(m);
                        }
                    }

                    if (remainingVehicles.Count > 0)
                    {
                        return Error.BadRequest("Danh sách SelectedVehicleIds chứa xe không khớp với bất kỳ sản phẩm nào trong đơn hàng.", "SelectedVehicleIds");
                    }

                    LeadEntity? lead = null;
                    if (!string.IsNullOrWhiteSpace(output.CustomerPhone))
                    {
                        lead = await leadReadRepository.GetByPhoneNumberAsync(output.CustomerPhone.Trim(), cancellationToken).ConfigureAwait(false);
                        if (lead is null)
                        {
                            lead = new LeadEntity
                            {
                                FullName = output.CustomerName?.Trim() ?? string.Empty,
                                PhoneNumber = output.CustomerPhone.Trim(),
                                Address = output.CustomerAddress?.Trim() ?? string.Empty,
                                Status = Domain.Constants.Lead.LeadStatus.New,
                                Source = Domain.Constants.Lead.LeadSource.WebStore
                            };
                            await leadInsertRepository.AddAsync(lead, cancellationToken).ConfigureAwait(false);
                        }
                    }

                    foreach (var pair in matchedVehiclesMap)
                    {
                        var oi = pair.Key;
                        foreach (var v in pair.Value)
                        {
                            v.OutputInfoId = oi.Id;
                            if (lead is not null)
                            {
                                v.Lead = lead;
                            }
                            vehicleUpdateRepository.Update(v);
                        }
                    }
                }

                var checkResult = await updateRepository.HandleInventoryTransactionAsync(
                    output.Id,
                    false,
                    cancellationToken)
                    .ConfigureAwait(false);
                if (checkResult.IsFailure)
                {
                    return Result<OrderDetailResponse>.Failure(checkResult.Errors!);
                }
                break;
            case OrderStatus.Cancelled:
            case OrderStatus.Refunding:
            case OrderStatus.Refunded:
                await commissionService.VoidCommissionAsync(output.Id, cancellationToken).ConfigureAwait(false);
                break;
            default:
                foreach (var outputInfo in output.OutputInfos)
                {
                    if (outputInfo.ProductVarientId.HasValue && outputInfo.Count.HasValue)
                    {
                        var stock = await readRepository.GetStockQuantityByVariantIdAsync(
                            outputInfo.ProductVarientId.Value,
                            outputInfo.ProductVariantColorId,
                            cancellationToken)
                            .ConfigureAwait(false);
                        if (stock < outputInfo.Count.Value)
                        {
                            return Error.BadRequest(
                                $"Sản phẩm ID {outputInfo.ProductVarientId} không đủ tồn kho. Hiện có: {stock}, cần: {outputInfo.Count.Value}",
                                "Products");
                        }
                    }
                }
                break;
        }
        output.StatusId = request.StatusId;
        output.LastStatusChangedAt = DateTimeOffset.UtcNow;
        updateRepository.Update(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (isCompleting)
        {
            await commissionService.CalculateAndRecordCommissionAsync(output.Id, cancellationToken)
                .ConfigureAwait(false);
        }
        var updated = await readRepository.GetByIdWithDetailsAsync(output.Id, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(updated);
        return updated.Adapt<OrderDetailResponse>();
    }
}
