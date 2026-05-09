using Application.Features.Vehicles.Commands.CreateVehicle;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Lead;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants;
using FluentAssertions;
using MockQueryable;
using Moq;
using LeadEntity = Domain.Entities.Lead;
using ProductEntity = Domain.Entities.Product;
using VehicleEntity = Domain.Entities.Vehicle;

namespace UnitTests;

public class VehicleAsset
{
    private readonly Mock<IVehicleReadRepository> _readRepoMock;
    private readonly Mock<IVehicleUpdateRepository> _updateRepoMock;
    private readonly Mock<ILeadReadRepository> _leadReadRepoMock;
    private readonly Mock<IProductReadRepository> _productReadRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public VehicleAsset()
    {
        _readRepoMock = new Mock<IVehicleReadRepository>();
        _updateRepoMock = new Mock<IVehicleUpdateRepository>();
        _leadReadRepoMock = new Mock<ILeadReadRepository>();
        _productReadRepoMock = new Mock<IProductReadRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact(DisplayName = "VAS_003 - Chặn trùng lặp số máy (EngineNumber)")]
    public async Task CreateVehicle_DuplicateEngineNumber_ReturnsBadRequest()
    {
        var engineNumber = "ENG999";
        var existingVehicles = new List<VehicleEntity> { new() { EngineNumber = engineNumber } }.BuildMock();
        _readRepoMock.Setup(x => x.GetQuery(DataFetchMode.All)).Returns(existingVehicles);
        _leadReadRepoMock.Setup(x => x.GetQueryable()).Returns(new List<LeadEntity> { new() { Id = 1 } }.BuildMock());
        _productReadRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>()))
            .Returns(new List<ProductEntity> { new() { Id = 1 } }.BuildMock());
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
        var existingVehicles = new List<VehicleEntity>().BuildMock();
        _readRepoMock.Setup(x => x.GetQuery(DataFetchMode.All)).Returns(existingVehicles);
        _leadReadRepoMock.Setup(x => x.GetQueryable()).Returns(new List<LeadEntity> { new() { Id = 1 } }.BuildMock());
        _productReadRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>()))
            .Returns(new List<ProductEntity> { new() { Id = 1 } }.BuildMock());
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
}
