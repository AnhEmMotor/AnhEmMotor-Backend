using Application.Features.Vehicles.Commands.CreateVehicle;
using Application.Features.Vehicles.Commands.DeleteClientVehicle;
using Application.Features.Vehicles.Commands.RegisterVehicle;
using Application.Features.Vehicles.Commands.UpdateClientVehicle;
using Application.Features.Vehicles.Queries.GetClientVehicle;
using Application.Features.Vehicles.Queries.GetClientVehicleDetail;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Lead.Lead;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.Vehicle;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class VehicleAsset
{
    private readonly Mock<IVehicleReadRepository> _readRepoMock;
    private readonly Mock<IVehicleUpdateRepository> _updateRepoMock;
    private readonly Mock<ILeadReadRepository> _leadReadRepoMock;
    private readonly Mock<IProductReadRepository> _productReadRepoMock;
    private readonly Mock<IMaintenanceHistoryReadRepository> _maintenanceHistoryReadRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public VehicleAsset()
    {
        _readRepoMock = new Mock<IVehicleReadRepository>();
        _updateRepoMock = new Mock<IVehicleUpdateRepository>();
        _leadReadRepoMock = new Mock<ILeadReadRepository>();
        _productReadRepoMock = new Mock<IProductReadRepository>();
        _maintenanceHistoryReadRepoMock = new Mock<IMaintenanceHistoryReadRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact(DisplayName = "VAS_003 - Chặn trùng lặp số máy (EngineNumber)")]
    public async Task CreateVehicle_DuplicateEngineNumber_ReturnsBadRequest()
    {
        var engineNumber = "ENG999";
        _leadReadRepoMock.Setup(x => x.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _productReadRepoMock.Setup(x => x.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _readRepoMock.Setup(x => x.ExistsByVinAsync("VIN001", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _readRepoMock.Setup(x => x.ExistsByEngineNumberAsync(engineNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = new CreateVehicleCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _leadReadRepoMock.Object,
            _productReadRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateVehicleCommand
        {
            VinNumber = "VIN001",
            EngineNumber = engineNumber,
            LeadId = 1,
            ProductId = 1
        };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Engine number already exists.");
    }

    [Fact(DisplayName = "VAS_008 - Ngăn chặn tạo tài sản khi thiếu số khung")]
    public async Task CreateVehicle_EmptyVin_ReturnsBadRequest()
    {
        _leadReadRepoMock.Setup(x => x.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _productReadRepoMock.Setup(x => x.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new CreateVehicleCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _leadReadRepoMock.Object,
            _productReadRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateVehicleCommand
        {
            VinNumber = string.Empty,
            EngineNumber = "ENG123",
            LeadId = 1,
            ProductId = 1
        };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("VIN cannot be empty.");
    }

    [Fact(DisplayName = "VAS_009 - Đăng ký xe khách hàng lưu đầy đủ thông tin")]
    public async Task RegisterVehicleCommand_ValidData_SavesVehicleProfile()
    {
        _readRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Vehicle>());
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new RegisterVehicleCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new RegisterVehicleCommand
        {
            UserId = Guid.NewGuid(),
            LicensePlate = "59A-12345",
            VinNumber = "VIN_TEST_001",
            EngineNumber = "ENG_TEST_001",
            Color = "Đỏ",
            PurchaseDate = DateTimeOffset.UtcNow,
            WarrantyDate = DateTimeOffset.UtcNow.AddYears(2),
            CurrentOdo = 1250
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        _updateRepoMock.Verify(
            x => x.Add(It.Is<Domain.Entities.Vehicle>(v =>
                v.LicensePlate == "59A-12345" &&
                v.VinNumber == "VIN_TEST_001" &&
                v.EngineNumber == "ENG_TEST_001" &&
                v.Color == "Đỏ" &&
                v.CurrentOdo == 1250)), Times.Once);
    }

    [Fact(DisplayName = "VAS_011 - Lấy thông tin xe theo id của khách hàng")]
    public async Task GetClientVehicleQuery_ValidUserVehicle_ReturnsVehicle()
    {
        var userId = Guid.NewGuid();
        var vehicle = new Domain.Entities.Vehicle { Id = 10, UserId = userId, LicensePlate = "59A-11111", VinNumber = "VIN_GET_001", EngineNumber = "ENG_GET_001", IsActive = true };
        _readRepoMock.Setup(x => x.GetByUserIdAndIdAsync(userId, 10, It.IsAny<CancellationToken>())).ReturnsAsync(vehicle);

        var handler = new GetClientVehicleQueryHandler(_readRepoMock.Object);
        var result = await handler.Handle(new GetClientVehicleQuery(10, userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(10);
        result.Value.LicensePlate.Should().Be("59A-11111");
    }

    [Fact(DisplayName = "VAS_012 - Cập nhật thông tin xe của khách hàng")]
    public async Task UpdateClientVehicleCommand_ValidData_UpdatesVehicle()
    {
        var userId = Guid.NewGuid();
        var vehicle = new Domain.Entities.Vehicle { Id = 11, UserId = userId, LicensePlate = "OLD", Color = "Xanh", CurrentOdo = 100, IsActive = true };
        _readRepoMock.Setup(x => x.GetByUserIdAndIdAsync(userId, 11, It.IsAny<CancellationToken>())).ReturnsAsync(vehicle);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new UpdateClientVehicleCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var result = await handler.Handle(new UpdateClientVehicleCommand
        {
            VehicleId = 11,
            UserId = userId,
            LicensePlate = "59A-22222",
            Color = "Đỏ",
            CurrentOdo = 500,
            WarrantyDate = DateTimeOffset.UtcNow.AddYears(1)
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        vehicle.LicensePlate.Should().Be("59A-22222");
        vehicle.Color.Should().Be("Đỏ");
        vehicle.CurrentOdo.Should().Be(500);
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Domain.Entities.Vehicle>()), Times.Once);
    }

    [Fact(DisplayName = "VAS_013 - Xóa mềm xe của khách hàng")]
    public async Task DeleteClientVehicleCommand_ValidData_DeactivatesVehicle()
    {
        var userId = Guid.NewGuid();
        var vehicle = new Domain.Entities.Vehicle { Id = 12, UserId = userId, IsActive = true };
        _readRepoMock.Setup(x => x.GetByUserIdAndIdAsync(userId, 12, It.IsAny<CancellationToken>())).ReturnsAsync(vehicle);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new DeleteClientVehicleCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var result = await handler.Handle(new DeleteClientVehicleCommand(12, userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        vehicle.IsActive.Should().BeFalse();
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Domain.Entities.Vehicle>()), Times.Once);
    }

    [Fact(DisplayName = "VAS_014 - Lấy chi tiết xe trả về bảo hành còn lại và trạng thái bảo trì")]
    public async Task GetClientVehicleDetailQuery_ValidVehicle_ReturnsWarrantyAndMaintenanceInfo()
    {
        var userId = Guid.NewGuid();
        var vehicle = new Domain.Entities.Vehicle
        {
            Id = 13,
            UserId = userId,
            LicensePlate = "59A-33333",
            VinNumber = "VIN_DETAIL_001",
            EngineNumber = "ENG_DETAIL_001",
            IsActive = true,
            WarrantyDate = DateTimeOffset.UtcNow.AddDays(45),
            NextMaintenanceDate = DateTime.UtcNow.AddDays(10),
            LastMaintenanceDate = DateTime.UtcNow.AddDays(-30)
        };
        _readRepoMock.Setup(x => x.GetByUserIdAndIdAsync(userId, 13, It.IsAny<CancellationToken>())).ReturnsAsync(vehicle);
        _maintenanceHistoryReadRepoMock.Setup(x => x.GetByVehicleIdAsync(13, It.IsAny<CancellationToken>(), Domain.Constants.DataFetchMode.ActiveOnly))
            .ReturnsAsync(new List<Domain.Entities.MaintenanceHistory>());

        var handler = new GetClientVehicleDetailQueryHandler(_readRepoMock.Object, _maintenanceHistoryReadRepoMock.Object);
        var result = await handler.Handle(new GetClientVehicleDetailQuery(13, userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.WarrantyRemainingDays.Should().BeGreaterThan(0);
        result.Value.MaintenanceStatus.Should().Be("DueSoon");
        result.Value.NextMaintenanceDate.Should().NotBeNull();
    }
}
