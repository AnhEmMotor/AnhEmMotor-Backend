using Application.Common.Models;
using Application.Features.Outputs.Queries.GetVehicleAssignmentRequirements;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Entities;
using FluentAssertions;
using Moq;
using ProductEntity = Domain.Entities.Product;
using ProductCategoryEntity = Domain.Entities.ProductCategory;

namespace UnitTests;

public class VehicleAssignment
{
    private static OutputInfo BuildVinManagedInfo(int id, int variantId) => new()
    {
        Id = id,
        ProductVariantId = variantId,
        Count = 2,
        ProductVariant = new ProductVariant
        {
            Id = variantId,
            ProductId = 1,
            Product = new ProductEntity
            {
                Id = 1,
                Name = "Xe máy A",
                ProductCategory = new ProductCategoryEntity { ManagementType = "vin_number" }
            }
        }
    };

    private static Vehicle BuildVehicle(
        int id,
        string vin,
        int infoId,
        string receiptStatus,
        int? outputInfoId = null) => new()
    {
        Id = id,
        VinNumber = vin,
        EngineNumber = $"ENG-{id}",
        Status = "available",
        IsActive = true,
        ProductVariantId = 100,
        OutputInfoId = outputInfoId,
        InventoryReceiptInfoId = infoId,
        InventoryReceiptInfo = new InventoryReceiptInfo
        {
            Id = infoId,
            InventoryReceiptId = infoId,
            InventoryReceipt = new InventoryReceipt { Id = infoId, StatusId = receiptStatus }
        }
    };

    [Fact(DisplayName = "VIN_001 - Chỉ VIN thuộc phiếu nhập ĐÃ DUYỆT mới xuất hiện khi chọn")]
    public async Task OnlyApprovedReceiptVins_AreSelectable()
    {
        var output = new Output
        {
            Id = 1,
            StatusId = OrderStatus.Pending,
            OutputInfos = [BuildVinManagedInfo(11, 100)]
        };
        var readRepoMock = new Mock<IOutputReadRepository>();
        readRepoMock
            .Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(output);
        var vehicleRepoMock = new Mock<IVehicleReadRepository>();
        vehicleRepoMock
            .Setup(x => x.GetVehiclesForAssignmentAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                BuildVehicle(1, "VIN-OK-001", 501, "approve"),
                BuildVehicle(2, "VIN-DRAFT-002", 502, "draft"),
                BuildVehicle(3, "VIN-SENT-003", 503, "sent"),
                BuildVehicle(4, "VIN-REJECT-004", 504, "reject"),
                BuildVehicle(5, "VIN-ASSIGNED-005", 505, "approve", outputInfoId: 11)
            ]);

        var handler = new GetVehicleAssignmentRequirementsQueryHandler(readRepoMock.Object, vehicleRepoMock.Object);
        var result = await handler.Handle(
            new GetVehicleAssignmentRequirementsQuery { Id = 1, TargetStatusId = "delivering" },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var item = result.Value!.Items.Single();
        item.AvailableCount.Should().Be(1);
        item.AvailableVehicles.Select(v => v.VinNumber).Should().ContainSingle("VIN-OK-001");
        item.AssignedVehicles.Select(v => v.VinNumber).Should().Contain("VIN-ASSIGNED-005");
        item.CanFulfill.Should().BeTrue();
    }

    [Fact(DisplayName = "VIN_002 - Không còn VIN hợp lệ: CanFulfill = false, danh sách chọn rỗng")]
    public async Task NoApprovedReceiptVins_CannotFulfill()
    {
        var output = new Output
        {
            Id = 2,
            StatusId = OrderStatus.Pending,
            OutputInfos = [BuildVinManagedInfo(21, 100)]
        };
        var readRepoMock = new Mock<IOutputReadRepository>();
        readRepoMock
            .Setup(x => x.GetByIdWithDetailsAsync(2, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(output);
        var vehicleRepoMock = new Mock<IVehicleReadRepository>();
        vehicleRepoMock
            .Setup(x => x.GetVehiclesForAssignmentAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                BuildVehicle(6, "VIN-DRAFT-006", 601, "draft"),
                BuildVehicle(7, "VIN-SENT-007", 602, "sent")
            ]);

        var handler = new GetVehicleAssignmentRequirementsQueryHandler(readRepoMock.Object, vehicleRepoMock.Object);
        var result = await handler.Handle(
            new GetVehicleAssignmentRequirementsQuery { Id = 2, TargetStatusId = "waiting_pickup" },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var item = result.Value!.Items.Single();
        item.AvailableCount.Should().Be(0);
        item.AvailableVehicles.Should().BeEmpty();
        item.CanFulfill.Should().BeFalse();
    }
}
