using Application.Features.Sales.Returns.Commands.ProcessReturnRequest;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.ReturnRequest;
using Domain.Entities;
using FluentAssertions;
using Moq;
using InventoryReceiptStatuses = Domain.Constants.InventoryReceipt.InventoryReceiptStatus;

namespace UnitTests;

public class SalesReturns
{
    [Fact(DisplayName = "RETURNS_001 - Phiếu nhập kho từ trả hàng dùng trạng thái hợp lệ")]
    public async Task ProcessReturnRequest_Restock_UsesValidInventoryReceiptStatus()
    {
        var returnRequest = new ReturnRequest
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
        var readRepository = new Mock<IReturnRequestReadRepository>();
        var writeRepository = new Mock<IReturnRequestWriteRepository>();
        var receiptRepository = new Mock<IInventoryReceiptInsertRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        InventoryReceipt? insertedReceipt = null;

        readRepository
            .Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(returnRequest);
        writeRepository
            .Setup(repository => repository.UpdateAsync(returnRequest, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        receiptRepository
            .Setup(repository => repository.Add(It.IsAny<InventoryReceipt>()))
            .Callback<InventoryReceipt>(receipt => insertedReceipt = receipt);
        unitOfWork
            .Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new ProcessReturnRequestCommandHandler(
            readRepository.Object,
            writeRepository.Object,
            receiptRepository.Object,
            unitOfWork.Object);

        await handler.Handle(
            new ProcessReturnRequestCommand
            {
                ReturnRequestId = 1,
                Status = "completed",
                ReturnAction = "restock"
            },
            TestContext.Current.CancellationToken);

        insertedReceipt.Should().NotBeNull();
        insertedReceipt!.StatusId.Should().Be(InventoryReceiptStatuses.Approve);
        InventoryReceiptStatuses.IsValid(insertedReceipt.StatusId).Should().BeTrue();
    }
}
