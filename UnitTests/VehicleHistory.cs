using Application.Common.Models;
using Application.Features.Vehicles.Commands.CreateVehicleMaintenanceHistory;
using Application.Features.Vehicles.Commands.CreateVehiclePurchaseHistory;
using Application.Features.Vehicles.Commands.CreateVehicleWarrantyHistory;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class VehicleHistory
{
    [Fact(DisplayName = "VH_001 - Lưu lịch sử mua hàng cho xe thành công")]
    public async Task CreateVehiclePurchaseHistory_ValidData_SavesRecord()
    {
        var readRepo = new Mock<IVehicleReadRepository>();
        var writeRepo = new Mock<IVehiclePurchaseHistoryWriteRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        readRepo.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Vehicle { Id = 10, UserId = Guid.NewGuid(), IsActive = true });
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new CreateVehiclePurchaseHistoryCommandHandler(readRepo.Object, writeRepo.Object, unitOfWork.Object);
        var command = new CreateVehiclePurchaseHistoryCommand
        {
            VehicleId = 10,
            UserId = Guid.NewGuid(),
            PurchaseDate = DateTimeOffset.UtcNow,
            InvoiceNumber = "INV-1001",
            Amount = 250000000m,
            SellerName = "Anh Em Motor",
            Notes = "Đặt cọc"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        writeRepo.Verify(x => x.Add(It.Is<Domain.Entities.VehiclePurchaseHistory>(h =>
            h.VehicleId == 10 &&
            h.InvoiceNumber == "INV-1001" &&
            h.Amount == 250000000m)), Times.Once);
    }

    [Fact(DisplayName = "VH_002 - Lưu lịch sử bảo hành cho xe thành công")]
    public async Task CreateVehicleWarrantyHistory_ValidData_SavesRecord()
    {
        var readRepo = new Mock<IVehicleReadRepository>();
        var writeRepo = new Mock<IVehicleWarrantyHistoryWriteRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        readRepo.Setup(x => x.GetByIdAsync(22, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Vehicle { Id = 22, UserId = Guid.NewGuid(), IsActive = true });
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new CreateVehicleWarrantyHistoryCommandHandler(readRepo.Object, writeRepo.Object, unitOfWork.Object);
        var command = new CreateVehicleWarrantyHistoryCommand
        {
            VehicleId = 22,
            UserId = Guid.NewGuid(),
            StartDate = DateTimeOffset.UtcNow,
            EndDate = DateTimeOffset.UtcNow.AddYears(2),
            ProviderName = "Nhà sản xuất",
            PolicyNumber = "POL-001",
            Description = "Bảo hành chính hãng",
            Status = "Active",
            CoverageAmount = 50000000m
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        writeRepo.Verify(x => x.Add(It.Is<Domain.Entities.VehicleWarrantyHistory>(h =>
            h.VehicleId == 22 &&
            h.PolicyNumber == "POL-001" &&
            h.Status == "Active")), Times.Once);
    }

    [Fact(DisplayName = "VH_003 - Lưu lịch sử sửa chữa/bảo dưỡng cho xe thành công")]
    public async Task CreateVehicleMaintenanceHistory_ValidData_SavesRecord()
    {
        var readRepo = new Mock<IVehicleReadRepository>();
        var writeRepo = new Mock<IMaintenanceHistoryWriteRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        readRepo.Setup(x => x.GetByIdAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Vehicle { Id = 30, UserId = Guid.NewGuid(), IsActive = true });
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new CreateVehicleMaintenanceHistoryCommandHandler(readRepo.Object, writeRepo.Object, unitOfWork.Object);
        var command = new CreateVehicleMaintenanceHistoryCommand
        {
            VehicleId = 30,
            UserId = Guid.NewGuid(),
            MaintenanceDate = DateTimeOffset.UtcNow,
            Description = "Thay dầu và kiểm tra phanh",
            Mileage = 15200,
            TechnicianId = 5,
            PartsCost = 120000m,
            LaborCost = 80000m,
            PartsJson = "{\"items\":[{\"name\":\"Nhớt\",\"price\":120000}]}",
            NextMaintenanceDate = DateTimeOffset.UtcNow.AddMonths(3),
            NextMaintenanceOdo = 17800
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        writeRepo.Verify(x => x.Add(It.Is<Domain.Entities.MaintenanceHistory>(h =>
            h.VehicleId == 30 &&
            h.Description == "Thay dầu và kiểm tra phanh" &&
            h.TotalCost == 200000m &&
            h.NextMaintenanceOdo == 17800)), Times.Once);
    }
}
