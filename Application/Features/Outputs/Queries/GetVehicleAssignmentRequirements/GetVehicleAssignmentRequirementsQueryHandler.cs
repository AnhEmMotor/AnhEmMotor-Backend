using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Constants.Product;
using Domain.Entities;
using MediatR;

namespace Application.Features.Outputs.Queries.GetVehicleAssignmentRequirements;

public class GetVehicleAssignmentRequirementsQueryHandler(
    IOutputReadRepository outputReadRepository,
    IVehicleReadRepository vehicleReadRepository) : IRequestHandler<GetVehicleAssignmentRequirementsQuery, Result<VehicleAssignmentRequirementResponse>>
{
    public async Task<Result<VehicleAssignmentRequirementResponse>> Handle(
        GetVehicleAssignmentRequirementsQuery request,
        CancellationToken cancellationToken)
    {
        var output = await outputReadRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        if (output is null)
        {
            return Error.NotFound($"Không tìm thấy đơn hàng có ID {request.Id}.", "Id");
        }
        if (!OrderStatus.IsValid(request.TargetStatusId))
        {
            return Error.BadRequest($"Trạng thái '{request.TargetStatusId}' không hợp lệ.", "TargetStatusId");
        }
        var response = new VehicleAssignmentRequirementResponse
        {
            OrderId = output.Id,
            TargetStatusId = request.TargetStatusId,
            RequiresVehicleAssignment = OrderVehicleAssignmentStatus.RequiresVehicleAssignment(request.TargetStatusId)
        };
        if (!response.RequiresVehicleAssignment)
        {
            return response;
        }
        var vehicleManagedInfos = output.OutputInfos
            .Where(
                oi => string.Equals(
                    oi.ProductVariant?.Product?.ProductCategory?.ManagementType,
                    ProductManagementType.VinNumber,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (vehicleManagedInfos.Count == 0)
        {
            return response;
        }
        var productVariantIds = vehicleManagedInfos
            .Select(oi => oi.ProductVariantId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var vehicles = await vehicleReadRepository.GetVehiclesForAssignmentAsync(productVariantIds, cancellationToken)
            .ConfigureAwait(false);
        foreach (var info in vehicleManagedInfos)
        {
            var productId = info.ProductVariant?.ProductId;
            var assigned = vehicles
                .Where(v => v.OutputInfoId == info.Id)
                .Select(ToOption)
                .ToList();
            var available = vehicles
                .Where(
                    v => v.ProductVariantId == info.ProductVariantId &&
                        v.ProductVariantColorId == info.ProductVariantColorId &&
                        string.Equals(v.Status, VehicleStatus.Available, StringComparison.OrdinalIgnoreCase) &&
                        v.OutputInfoId == null &&
                        v.IsActive &&
                        v.InventoryReceiptInfoId != null)
                .Select(ToOption)
                .ToList();
            var requiredCount = info.Count ?? 0;
            response.Items
                .Add(
                    new VehicleAssignmentRequirementItemResponse
                    {
                        OutputInfoId = info.Id,
                        ProductId = productId,
                        ProductName = info.ProductVariant?.Product?.Name,
                        ProductVariantId = info.ProductVariantId,
                        ProductVariantName = BuildVariantName(info.ProductVariant),
                        ProductVariantColorId = info.ProductVariantColorId,
                        ColorName = info.ProductVariantColor?.ColorName ?? info.ProductVariantColor?.ColorCode,
                        RequiredCount = requiredCount,
                        AssignedVehicles = assigned,
                        AvailableVehicles = available,
                        AvailableCount = available.Count,
                        CanFulfill = assigned.Count + available.Count >= requiredCount
                    });
        }
        return response;
    }

    private static VehicleAssignmentOptionResponse ToOption(Vehicle vehicle)
    {
        return new VehicleAssignmentOptionResponse
        {
            Id = vehicle.Id,
            VinNumber = vehicle.VinNumber,
            EngineNumber = vehicle.EngineNumber,
            Status = vehicle.Status
        };
    }

    private static string? BuildVariantName(ProductVariant? variant)
    {
        var names = variant?.VariantOptionValues?
            .Select(vov => vov.OptionValue?.Name).Where(name => !string.IsNullOrWhiteSpace(name)).ToList();
        return names is { Count: > 0 } ? string.Join(" - ", names) : null;
    }
}
