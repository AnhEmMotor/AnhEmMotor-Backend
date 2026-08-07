using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Features.InventoryOnHand.Notifications;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Commission;
using Application.Interfaces.Repositories.InventoryLedger;
using Application.Interfaces.Repositories.Lead.Lead;
using Application.Interfaces.Repositories.Logistics.Shipment;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.Vehicle;
using Application.Interfaces.Services.Logistics;
using Application.Interfaces.Services.Shipping;
using Domain.Constants;
using Domain.Constants.Lead;
using Domain.Constants.Logistics;
using Domain.Constants.Order;
using Domain.Constants.Product;
using Domain.Entities;
using Domain.Entities.Logistics;
using Mapster;
using MediatR;
using LeadEntity = Domain.Entities.Lead;
using OutputInfoEntity = Domain.Entities.OutputInfo;
using VehicleEntity = Domain.Entities.Vehicle;

namespace Application.Features.Outputs.Commands.UpdateOutputStatus;

public class UpdateOutputStatusCommandHandler(
    IOutputReadRepository readRepository,
    IOutputUpdateRepository updateRepository,
    ICommissionUpdateRepository commissionUpdateRepository,
    IUnitOfWork unitOfWork,
    IVehicleReadRepository? vehicleReadRepository = null,
    IVehicleUpdateRepository? vehicleUpdateRepository = null,
    ILeadReadRepository? leadReadRepository = null,
    ILeadInsertRepository? leadInsertRepository = null,
    IShippingService? shippingService = null,
    IShipmentInsertRepository? shipmentInsertRepository = null,
    IGeocodingService? geocodingService = null,
    IInventoryLedgerRepository? ledgerRepository = null,
    IPublisher? publisher = null) : IRequestHandler<UpdateOutputStatusCommand, Result<OrderDetailResponse>>
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
        var exportedCombos = new HashSet<(int VariantId, int? ColorId)>();
        switch (request.StatusId)
        {
            case OrderStatus.Completed:
                isCompleting = true;
                output.FinishedBy = request.CurrentUserId == Guid.Empty ? null : request.CurrentUserId;
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
                if (ledgerRepository != null)
                {
                    foreach (var outputInfo in output.OutputInfos)
                    {
                        if (outputInfo.ProductVariantId is null || outputInfo.Count is null || outputInfo.Count <= 0)
                        {
                            continue;
                        }
                        var variantId = outputInfo.ProductVariantId.Value;
                        var colorId = outputInfo.ProductVariantColorId;
                        var lastEntry = await ledgerRepository.GetLastEntryAsync(variantId, colorId, cancellationToken)
                            .ConfigureAwait(false);
                        var currentStock = lastEntry?.StockAfter ?? 0;
                        var exportQty = outputInfo.Count.Value;
                        var unitPrice = outputInfo.CostPrice ?? outputInfo.Price ?? 0;
                        var ledger = new InventoryLedger
                        {
                            TransactionDate = DateTimeOffset.UtcNow,
                            DocumentCode = $"OUT-{output.Id}",
                            TransactionType = "Xuất kho",
                            ProductVariantId = variantId,
                            ProductVariantColorId = colorId,
                            PartnerName = output.CustomerName,
                            ImportQty = 0,
                            ExportQty = exportQty,
                            UnitPrice = unitPrice,
                            TotalAmount = exportQty * unitPrice,
                            StockAfter = currentStock - exportQty
                        };
                        await ledgerRepository.AddAsync(ledger, cancellationToken).ConfigureAwait(false);
                        exportedCombos.Add((variantId, colorId));
                    }
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
                string trackingNumber = $"GHN-{output.Id}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                if (shippingService != null)
                {
                    var shippingResult = await shippingService.CreateShippingOrderAsync(output, cancellationToken)
                        .ConfigureAwait(false);
                    if (shippingResult.IsFailure)
                    {
                        return Result<OrderDetailResponse>.Failure(shippingResult.Errors!);
                    }
                    trackingNumber = shippingResult.Value;
                }
                if (shipmentInsertRepository != null)
                {
                    double? destLat = null;
                    double? destLon = null;
                    var addressParts = new List<string>();
                    if (!string.IsNullOrWhiteSpace(output.CustomerAddress))
                        addressParts.Add(output.CustomerAddress.Trim());
                    if (!string.IsNullOrWhiteSpace(output.WardName))
                        addressParts.Add(output.WardName.Trim());
                    if (!string.IsNullOrWhiteSpace(output.ProvinceName))
                        addressParts.Add(output.ProvinceName.Trim());
                    var fullAddress = string.Join(", ", addressParts);
                    if (geocodingService != null && !string.IsNullOrWhiteSpace(fullAddress))
                    {
                        var coords = await geocodingService.GetCoordinatesAsync(fullAddress, cancellationToken)
                            .ConfigureAwait(false);
                        if (coords.HasValue)
                        {
                            destLat = coords.Value.Latitude;
                            destLon = coords.Value.Longitude;
                        }
                    }
                    var shipment = new Shipment
                    {
                        TrackingNumber = trackingNumber,
                        CustomerName = output.CustomerName ?? string.Empty,
                        ShippingCost = output.ShippingFee ?? 0,
                        CodAmount = output.Total - (output.PaidAmount ?? 0),
                        CustomerPhone = output.CustomerPhone ?? string.Empty,
                        OriginAddress = "Kho AnhEmMotor",
                        OriginLatitude = LogisticsConstants.DefaultShowroomLatitude,
                        OriginLongitude = LogisticsConstants.DefaultShowroomLongitude,
                        DestinationAddress = fullAddress,
                        DestinationLatitude = destLat,
                        DestinationLongitude = destLon,
                        Type = ShipmentType.OrderDelivery,
                        OutputId = output.Id,
                        Items =
                            output.OutputInfos
                                .Select(
                                    oi => new ShipmentItem
                                {
                                    ProductVariantId = oi.ProductVariantId,
                                    ProductVariantColorId = oi.ProductVariantColorId,
                                    Quantity = oi.Count ?? 1
                                })
                                .ToList()
                    };
                    await shipmentInsertRepository.AddAsync(shipment, cancellationToken).ConfigureAwait(false);
                }
                break;
            case OrderStatus.Cancelled:
            case OrderStatus.Refunding:
            case OrderStatus.Refunded:
                await commissionUpdateRepository.VoidCommissionAsync(output.Id, cancellationToken).ConfigureAwait(false);
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
        await SyncLeadStatusAsync(output, request.StatusId, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (isCompleting)
        {
            await commissionUpdateRepository.CalculateAndRecordCommissionAsync(output.Id, cancellationToken)
                .ConfigureAwait(false);
            if (publisher != null && exportedCombos.Count > 0)
            {
                await publisher.Publish(new InventoryChangedNotification(exportedCombos), cancellationToken)
                    .ConfigureAwait(false);
            }
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
        if (selectedVehicleIds.Count == 0 && output.OutputInfos.SelectMany(oi => oi.Vehicles).Any())
        {
            selectedVehicleIds = output.OutputInfos.SelectMany(oi => oi.Vehicles).Select(v => v.Id).ToList();
        }
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
            if (vehicle.InventoryReceiptInfoId == null)
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

    private async Task SyncLeadStatusAsync(Output output, string orderStatusId, CancellationToken cancellationToken)
    {
        if (leadReadRepository == null || string.IsNullOrEmpty(output.CustomerPhone))
        {
            return;
        }
        var lead = output.Lead;
        if (lead == null)
        {
            lead = await leadReadRepository.GetByPhoneNumberAsync(output.CustomerPhone.Trim(), cancellationToken)
                .ConfigureAwait(false);
        }
        if (lead == null && output.LeadId.HasValue)
        {
            lead = await leadReadRepository.GetByIdAsync(output.LeadId.Value, cancellationToken).ConfigureAwait(false);
        }
        if (lead == null)
        {
            return;
        }
        string? targetLeadStatus = null;
        switch (orderStatusId)
        {
            case OrderStatus.Completed:
                targetLeadStatus = LeadStatus.Delivered;
                break;
            case OrderStatus.Delivering:
            case OrderStatus.WaitingPickup:
                targetLeadStatus = LeadStatus.Paperwork;
                break;
            case OrderStatus.WaitingInstallment:
            case OrderStatus.PaidProcessing:
            case OrderStatus.ConfirmedCod:
            case OrderStatus.DepositPaid:
            case OrderStatus.InstallmentApproved:
                targetLeadStatus = LeadStatus.Deposited;
                break;
        }
        if (targetLeadStatus != null && lead.Status != targetLeadStatus)
        {
            lead.Status = targetLeadStatus;
        }
    }
}
