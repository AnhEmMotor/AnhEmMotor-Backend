using Application.Common.Models;
using Application.Features.Sales.Returns.Commands.ProcessReturnRequest;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.Logistics.Shipment;
using Application.Interfaces.Repositories.ReturnRequest;
using Application.Interfaces.Services.Shipping;
using Domain.Entities;
using Domain.Entities.Logistics;
using Domain.Enums;
using FluentAssertions;
using Moq;
using InventoryReceiptStatuses = Domain.Constants.InventoryReceipt.InventoryReceiptStatus;

namespace UnitTests;

public class SalesReturns
{
    private readonly Mock<IReturnRequestReadRepository> _readRepository = new();
    private readonly Mock<IReturnRequestWriteRepository> _writeRepository = new();
    private readonly Mock<IInventoryReceiptInsertRepository> _receiptRepository = new();
    private readonly Mock<IShipmentReadRepository> _shipmentReadRepository = new();
    private readonly Mock<IShipmentInsertRepository> _shipmentInsertRepository = new();
    private readonly Mock<IShipmentUpdateRepository> _shipmentUpdateRepository = new();
    private readonly Mock<Application.Interfaces.Repositories.Output.IOutputReadRepository> _outputReadRepository = new();
    private readonly Mock<IShippingService> _shippingService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private ReturnRequest BuildReturnRequest() => new()
    {
        Id = 1,
        OrderId = 10,
        OrderCode = "ORD-2026-0001",
        Items =
        [
            new ReturnRequestItem
            {
                Id = 1,
                ProductId = 100,
                ProductName = "Sản phẩm trả hàng",
                Quantity = 1,
                ReturnQuantity = 1,
                Sku = "RETURN-001"
            }
        ]
    };

    private void SetupCommonMocks(ReturnRequest returnRequest)
    {
        _readRepository
            .Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(returnRequest);
        _writeRepository
            .Setup(repository => repository.UpdateAsync(returnRequest, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWork
            .Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private ProcessReturnRequestCommandHandler BuildHandler() => new(
        _readRepository.Object,
        _writeRepository.Object,
        _receiptRepository.Object,
        _shipmentReadRepository.Object,
        _shipmentInsertRepository.Object,
        _shipmentUpdateRepository.Object,
        _outputReadRepository.Object,
        _shippingService.Object,
        _unitOfWork.Object);

    private ProcessReturnRequestCommand BuildCommand(string status, string? action = "restock") => new()
    {
        ReturnRequestId = 1,
        Status = status,
        ReturnAction = action
    };

    [Fact(DisplayName = "RETURNS_001 - Phiếu nhập kho từ trả hàng offline dùng trạng thái hợp lệ")]
    public async Task ProcessOfflineReturn_Restock_UsesValidInventoryReceiptStatus()
    {
        var returnRequest = BuildReturnRequest();
        SetupCommonMocks(returnRequest);
        InventoryReceipt? insertedReceipt = null;
        _receiptRepository
            .Setup(repository => repository.Add(It.IsAny<InventoryReceipt>()))
            .Callback<InventoryReceipt>(receipt => insertedReceipt = receipt);

        await BuildHandler().Handle(BuildCommand("completed"), TestContext.Current.CancellationToken);

        insertedReceipt.Should().NotBeNull();
        insertedReceipt!.StatusId.Should().Be(InventoryReceiptStatuses.Approve);
        InventoryReceiptStatuses.IsValid(insertedReceipt.StatusId).Should().BeTrue();
    }

    [Fact(DisplayName = "RETURNS_002 - Đơn duyệt hoàn hàng: tạo đơn GHN thu hồi về showroom và hoãn restock")]
    public async Task ProcessCarrierReturn_CreatesGhnReturnPickupOrderAndDefersRestock()
    {
        var returnRequest = BuildReturnRequest();
        SetupCommonMocks(returnRequest);
        var output = new Output { Id = 10, CustomerAddress = "Hà Nội", CustomerName = "Khách A", CustomerPhone = "0123456789" };
        _outputReadRepository
            .Setup(repo => repo.GetByIdAsync(10, It.IsAny<CancellationToken>(), Domain.Constants.DataFetchMode.All))
            .ReturnsAsync(output);
        _shippingService
            .Setup(service => service.CreateReturnPickupOrderAsync(output, returnRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("GHN-RET-123456"));
        InventoryReceipt? insertedReceipt = null;
        _receiptRepository
            .Setup(repository => repository.Add(It.IsAny<InventoryReceipt>()))
            .Callback<InventoryReceipt>(receipt => insertedReceipt = receipt);

        var result = await BuildHandler().Handle(BuildCommand("completed"), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        insertedReceipt.Should().NotBeNull();
        insertedReceipt!.StatusId.Should().Be(Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Sent);
        _shipmentInsertRepository.Verify(repo => repo.AddAsync(It.Is<Shipment>(s => s.TrackingNumber == "GHN-RET-123456"), It.IsAny<CancellationToken>()), Times.Once);
        returnRequest.OriginalTrackingNumber.Should().Be("GHN-RET-123456");
    }

    [Fact(DisplayName = "RETURNS_003 - GHN từ chối tạo đơn thu hồi: fallback nhập kho nội bộ an toàn")]
    public async Task ProcessCarrierReturn_GhnFails_FallbacksToImmediateRestock()
    {
        var returnRequest = BuildReturnRequest();
        SetupCommonMocks(returnRequest);
        var output = new Output { Id = 10 };
        _outputReadRepository
            .Setup(repo => repo.GetByIdAsync(10, It.IsAny<CancellationToken>(), Domain.Constants.DataFetchMode.All))
            .ReturnsAsync(output);
        _shippingService
            .Setup(service => service.CreateReturnPickupOrderAsync(output, returnRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure("GHN Error: address not found"));
        InventoryReceipt? insertedReceipt = null;
        _receiptRepository
            .Setup(repository => repository.Add(It.IsAny<InventoryReceipt>()))
            .Callback<InventoryReceipt>(receipt => insertedReceipt = receipt);

        var result = await BuildHandler().Handle(BuildCommand("completed"), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        insertedReceipt.Should().NotBeNull();
        returnRequest.Status.Should().Be("completed");
        _writeRepository.Verify(
            repository => repository.UpdateAsync(returnRequest, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWork.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "RETURNS_004 - Mã placeholder GHN-: vẫn restock ngay như đơn offline")]
    public async Task ProcessPlaceholderTracking_StillRestocksImmediately()
    {
        var returnRequest = BuildReturnRequest();
        SetupCommonMocks(returnRequest);
        var shipment = new Shipment { TrackingNumber = "GHN-10-1717000000", OutputId = 10 };
        _shipmentReadRepository
            .Setup(repository => repository.GetByOutputIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);
        InventoryReceipt? insertedReceipt = null;
        _receiptRepository
            .Setup(repository => repository.Add(It.IsAny<InventoryReceipt>()))
            .Callback<InventoryReceipt>(receipt => insertedReceipt = receipt);

        var result = await BuildHandler().Handle(BuildCommand("completed"), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        insertedReceipt.Should().NotBeNull();
        _shippingService.Verify(
            service => service.SwitchToReturnOrderAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "RETURNS_005 - Từ chối yêu cầu trả hàng: không gọi GHN, không tạo phiếu")]
    public async Task RejectReturn_NoCarrierCallNoReceipt()
    {
        var returnRequest = BuildReturnRequest();
        SetupCommonMocks(returnRequest);
        var shipment = new Shipment { TrackingNumber = "5ENLKKHD", OutputId = 10 };
        _shipmentReadRepository
            .Setup(repository => repository.GetByOutputIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        var result = await BuildHandler().Handle(
            BuildCommand("rejected"),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        returnRequest.Status.Should().Be("rejected");
        _shippingService.Verify(
            service => service.SwitchToReturnOrderAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _receiptRepository.Verify(
            repository => repository.Add(It.IsAny<InventoryReceipt>()),
            Times.Never);
    }
}
