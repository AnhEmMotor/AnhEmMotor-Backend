using Application.Common.Models;
using Application.Features.Sales.Returns.Commands.ProcessReturnArrival;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.ReturnRequest;
using Domain.Entities;
using FluentAssertions;
using Moq;
using InventoryReceiptStatuses = Domain.Constants.InventoryReceipt.InventoryReceiptStatus;

namespace UnitTests;

public class ReturnArrival
{
    private readonly Mock<IReturnRequestReadRepository> _readRepository = new();
    private readonly Mock<IReturnRequestWriteRepository> _writeRepository = new();
    private readonly Mock<IInventoryReceiptInsertRepository> _receiptRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact(DisplayName = "ARRIVAL_001 - Hàng hoàn về kho: tạo phiếu nhập tự động duyệt cho yêu cầu chờ restock")]
    public async Task Arrival_WithPendingRestockRequests_CreatesApprovedReceipts()
    {
        var returnRequests = new List<Domain.Entities.ReturnRequest>
        {
            new()
            {
                Id = 7,
                OrderId = 10,
                Status = "completed",
                ReturnAction = "restock",
                Items =
                [
                    new ReturnRequestItem { Id = 1, Quantity = 1, ReturnQuantity = 1, ProductId = 100 },
                    new ReturnRequestItem { Id = 2, Quantity = 2, ReturnQuantity = 2, ProductId = 101 }
                ]
            },
            new()
            {
                Id = 8,
                OrderId = 10,
                Status = "completed",
                ReturnAction = "restock",
                Items = [new ReturnRequestItem { Id = 3, Quantity = 3, ReturnQuantity = 3, ProductId = 102 }]
            }
        };
        _readRepository
            .Setup(repository => repository.GetByOrderIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(returnRequests);
        var insertedReceipts = new List<InventoryReceipt>();
        _receiptRepository
            .Setup(repository => repository.Add(It.IsAny<InventoryReceipt>()))
            .Callback<InventoryReceipt>(receipt => insertedReceipts.Add(receipt));
        _unitOfWork
            .Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new ProcessReturnArrivalCommandHandler(
            _readRepository.Object,
            _writeRepository.Object,
            _receiptRepository.Object,
            _unitOfWork.Object);
        var result = await handler.Handle(
            new ProcessReturnArrivalCommand { OutputId = 10 },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
        insertedReceipts.Should().HaveCount(2);
        insertedReceipts.Select(receipt => receipt.Notes).Should().Contain(
            ["Restock from Return Request #7", "Restock from Return Request #8"]);
        insertedReceipts.Should().OnlyContain(receipt => receipt.StatusId == InventoryReceiptStatuses.Approve);
        insertedReceipts.Should().OnlyContain(receipt => InventoryReceiptStatuses.IsValid(receipt.StatusId));
        insertedReceipts.Should().OnlyContain(receipt => receipt.SourceOrderId == 10);
        insertedReceipts.First().InventoryReceiptInfos.Select(info => info.Count).Should()
            .BeEquivalentTo(new[] { 1, 2 });
        _unitOfWork.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "ARRIVAL_002 - Bom hàng / không có yêu cầu restock: không tạo gì")]
    public async Task Arrival_WithoutPendingRequests_DoesNothing()
    {
        _readRepository
            .Setup(repository => repository.GetByOrderIdAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new ProcessReturnArrivalCommandHandler(
            _readRepository.Object,
            _writeRepository.Object,
            _receiptRepository.Object,
            _unitOfWork.Object);
        var result = await handler.Handle(
            new ProcessReturnArrivalCommand { OutputId = 20 },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        _receiptRepository.Verify(
            repository => repository.Add(It.IsAny<InventoryReceipt>()),
            Times.Never);
        _unitOfWork.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
