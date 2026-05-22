using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Lead.Lead;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.Vehicle;
using Application.Interfaces.Services.HR;
using Domain.Constants;
using Domain.Constants.Lead;
using Domain.Constants.Order;
using Domain.Constants.Product;
using Domain.Entities;
using Mapster;
using MediatR;
using LeadEntity = Domain.Entities.Lead;
using OutputInfoEntity = Domain.Entities.OutputInfo;
using VehicleEntity = Domain.Entities.Vehicle;

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
        if (OrderVehicleAssignmentStatus.RequiresVehicleAssignment(request.StatusId))
        {
            var assignmentResult = await AssignVehiclesToOrderAsync(output, request, cancellationToken)
                .ConfigureAwait(false);
            if (assignmentResult.IsFailure)
            {
                return Result<OrderDetailResponse>.Failure(assignmentResult.Errors!);
            }
        }
        bool isCompleting = false;
        switch (request.StatusId)
        {
            case OrderStatus.Completed:
                isCompleting = true;
                output.FinishedBy = request.CurrentUserId;
                foreach (var vehicle in output.OutputInfos.SelectMany(oi => oi.Vehicles))
                {
                    vehicle.Status = VehicleStatus.Sold;
                    vehicleUpdateRepository?.Update(vehicle);
                }
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
            case OrderStatus.PaidProcessing:
                break;
            default:
                foreach (var outputInfo in output.OutputInfos)
                {
                    if (outputInfo.ProductVariantId.HasValue && outputInfo.Count.HasValue)
                    {
                        var stock = await readRepository.GetStockQuantityByVariantIdAsync(
                            outputInfo.ProductVariantId.Value,
                            outputInfo.ProductVariantColorId,
                            cancellationToken)
                            .ConfigureAwait(false);
                        if (stock < outputInfo.Count.Value)
                        {
                            return Error.BadRequest(
                                $"Sản phẩm ID {outputInfo.ProductVariantId} không đủ tồn kho. Hiện có: {stock}, cần: {outputInfo.Count.Value}",
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

    private async Task<Result> AssignVehiclesToOrderAsync(
        Output output,
        UpdateOutputStatusCommand request,
        CancellationToken cancellationToken)
    {
        var vehicleOutputInfos = output.OutputInfos
            .Where(
                oi => string.Equals(
                    oi.ProductVariant?.Product?.ProductCategory?.ManagementType,
                    ProductManagementType.VinNumber,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (vehicleOutputInfos.Count == 0)
        {
            return Result.Success();
        }
        if (vehicleReadRepository == null ||
            vehicleUpdateRepository == null ||
            leadReadRepository == null ||
            leadInsertRepository == null)
        {
            throw new InvalidOperationException("Vehicle/Lead repositories are not injected.");
        }
        var selectedVehicleIds = request.SelectedVehicleIds?.Distinct().ToList() ?? [];
        var requiredVehicleCount = vehicleOutputInfos.Sum(oi => oi.Count ?? 0);
        if (selectedVehicleIds.Count != requiredVehicleCount)
        {
            return Result.Failure(
                Error.BadRequest(
                    $"Danh sách xe (SelectedVehicleIds) phải có đúng {requiredVehicleCount} phần tử cho các sản phẩm quản lý theo số khung.",
                    "SelectedVehicleIds"));
        }
        var vehicles = await vehicleReadRepository.GetByIdsAsync(selectedVehicleIds, cancellationToken)
            .ConfigureAwait(false);
        if (vehicles.Count != selectedVehicleIds.Count)
        {
            return Result.Failure(
                Error.BadRequest("Một hoặc nhiều mã xe không tồn tại trong hệ thống.", "SelectedVehicleIds"));
        }
        foreach (var vehicle in vehicles)
        {
            if (!vehicle.IsActive)
            {
                return Result.Failure(
                    Error.BadRequest(
                        $"Xe có số khung (VIN) {vehicle.VinNumber} đang ở trạng thái không hoạt động.",
                        "SelectedVehicleIds"));
            }
            if (vehicle.InputInfoId == null)
            {
                return Result.Failure(
                    Error.BadRequest(
                        $"Xe có số khung (VIN) {vehicle.VinNumber} chưa được nhập kho.",
                        "SelectedVehicleIds"));
            }
            if (string.Equals(vehicle.Status, VehicleStatus.Sold, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure(
                    Error.BadRequest($"Xe có số khung (VIN) {vehicle.VinNumber} đã được bán.", "SelectedVehicleIds"));
            }
            if (vehicle.OutputInfoId.HasValue && vehicleOutputInfos.All(oi => oi.Id != vehicle.OutputInfoId.Value))
            {
                return Result.Failure(
                    Error.BadRequest(
                        $"Xe có số khung (VIN) {vehicle.VinNumber} đã được giữ cho đơn hàng khác.",
                        "SelectedVehicleIds"));
            }
        }
        var remainingVehicles = new List<VehicleEntity>(vehicles);
        var matchedVehiclesMap = new Dictionary<OutputInfoEntity, List<VehicleEntity>>();
        foreach (var outputInfo in vehicleOutputInfos)
        {
            var count = outputInfo.Count ?? 0;
            var matches = remainingVehicles
                .Where(
                    v => v.ProductVariantId == outputInfo.ProductVariantId &&
                        v.ProductVariantColorId == outputInfo.ProductVariantColorId)
                .Take(count)
                .ToList();
            if (matches.Count < count)
            {
                var colorMsg = outputInfo.ProductVariantColorId.HasValue ? " và màu sắc đã chọn" : string.Empty;
                return Result.Failure(
                    Error.BadRequest(
                        $"Không tìm thấy đủ xe phù hợp trong danh sách SelectedVehicleIds cho sản phẩm '{outputInfo.ProductVariant!.Product!.Name}'{colorMsg}. Cần: {count}, tìm thấy: {matches.Count}.",
                        "SelectedVehicleIds"));
            }
            matchedVehiclesMap[outputInfo] = matches;
            foreach (var match in matches)
            {
                remainingVehicles.Remove(match);
            }
        }
        if (remainingVehicles.Count > 0)
        {
            return Result.Failure(
                Error.BadRequest(
                    "Danh sách SelectedVehicleIds chứa xe không khớp với bất kỳ sản phẩm nào trong đơn hàng.",
                    "SelectedVehicleIds"));
        }
        var selectedSet = selectedVehicleIds.ToHashSet();
        foreach (var assignedVehicle in output.OutputInfos.SelectMany(oi => oi.Vehicles))
        {
            if (!selectedSet.Contains(assignedVehicle.Id) &&
                !string.Equals(assignedVehicle.Status, VehicleStatus.Sold, StringComparison.OrdinalIgnoreCase))
            {
                assignedVehicle.OutputInfoId = null;
                assignedVehicle.Status = VehicleStatus.Available;
                vehicleUpdateRepository.Update(assignedVehicle);
            }
        }
        var lead = await GetOrCreateLeadAsync(output, cancellationToken).ConfigureAwait(false);
        foreach (var pair in matchedVehiclesMap)
        {
            foreach (var vehicle in pair.Value)
            {
                vehicle.OutputInfoId = pair.Key.Id;
                vehicle.Status = string.Equals(
                        request.StatusId,
                        OrderStatus.Completed,
                        StringComparison.OrdinalIgnoreCase)
                    ? VehicleStatus.Sold
                    : VehicleStatus.AssignedToOrder;
                if (lead is not null)
                {
                    vehicle.Lead = lead;
                }
                vehicleUpdateRepository.Update(vehicle);
            }
        }
        return Result.Success();
    }

    private async Task<LeadEntity?> GetOrCreateLeadAsync(Output output, CancellationToken cancellationToken)
    {
        if (leadReadRepository == null ||
            leadInsertRepository == null ||
            string.IsNullOrWhiteSpace(output.CustomerPhone))
        {
            return null;
        }
        var lead = await leadReadRepository.GetByPhoneNumberAsync(output.CustomerPhone.Trim(), cancellationToken)
            .ConfigureAwait(false);
        if (lead is not null)
        {
            return lead;
        }
        lead = new LeadEntity
        {
            FullName = output.CustomerName?.Trim() ?? string.Empty,
            PhoneNumber = output.CustomerPhone.Trim(),
            Address = output.CustomerAddress?.Trim() ?? string.Empty,
            Status = LeadStatus.New,
            Source = LeadSource.WebStore
        };
        await leadInsertRepository.AddAsync(lead, cancellationToken).ConfigureAwait(false);
        return lead;
    }
}
