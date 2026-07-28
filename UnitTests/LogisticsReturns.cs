using Application.Features.Logistics.Returns;
using Application.Features.Logistics.Returns.Commands.InspectReturn;
using Application.Features.Logistics.Returns.Queries.GetReturnDetail;
using Application.Features.Logistics.Returns.Queries.GetReturns;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Entities.Logistics;
using Domain.Enums;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class LogisticsReturns
{
    [Fact(DisplayName = "RET_001 - Phiếu bị từ chối giữ đúng trạng thái từ chối")]
    public async Task GetReturns_RejectedAction_ReturnsRejectedStatus()
    {
        var order = CreateReturnedOrder();
        order.ReturnAction = "rejected";
        order.RejectionReason = "Không đủ điều kiện hoàn tiền";
        order.InspectedAt = DateTime.UtcNow.AddHours(-1);

        var readRepository = new Mock<IParcelDeliveryOrderReadRepository>();
        readRepository
            .Setup(repository => repository.GetReturnedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([order]);
        var handler = new GetReturnsQueryHandler(readRepository.Object);

        var result = await handler.Handle(
            new GetReturnsQuery { Status = ReturnOrderStatus.Rejected },
            CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Status.Should().Be(ReturnOrderStatus.Rejected);
    }

    [Fact(DisplayName = "RET_002 - Chi tiết phiếu bị từ chối trả đúng trạng thái")]
    public async Task GetReturnDetail_RejectedAction_ReturnsRejectedStatus()
    {
        var order = CreateReturnedOrder();
        order.ReturnAction = "rejected";
        order.RejectionReason = "Không đủ điều kiện hoàn tiền";

        var readRepository = new Mock<IParcelDeliveryOrderReadRepository>();
        readRepository
            .Setup(repository => repository.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        var handler = new GetReturnDetailQueryHandler(readRepository.Object);

        var result = await handler.Handle(
            new GetReturnDetailQuery { Id = order.Id },
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be("rejected");
        result.RejectionReason.Should().Be(order.RejectionReason);
        result.InspectedAt.Should().Be(order.InspectedAt);
    }

    [Fact(DisplayName = "RET_003 - Duyệt hoàn tiền không xóa dữ liệu kiểm định")]
    public async Task InspectReturn_ApproveRefund_PreservesExistingInspectionData()
    {
        var inspectedAt = DateTime.UtcNow.AddHours(-2);
        var order = CreateReturnedOrder();
        order.InspectedAt = inspectedAt;
        order.RefundAmount = 500_000m;
        order.ReturnShippingCost = 30_000m;

        var readRepository = new Mock<IParcelDeliveryOrderReadRepository>();
        readRepository
            .Setup(repository => repository.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        var updateRepository = new Mock<IParcelDeliveryOrderUpdateRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new InspectReturnCommandHandler(
            readRepository.Object,
            updateRepository.Object,
            unitOfWork.Object);

        var result = await handler.Handle(
            new InspectReturnCommand
            {
                Id = order.Id,
                Action = "refund",
                BoxCondition = order.BoxCondition,
                ProductCondition = order.ProductCondition,
                ReturnProofImage = order.ReturnProofImage,
                ReturnInternalNote = order.ReturnInternalNote
            },
            CancellationToken.None);

        result.Should().BeTrue();
        order.ReturnAction.Should().Be("refund");
        order.InspectedAt.Should().Be(inspectedAt);
        order.RefundAmount.Should().Be(500_000m);
        order.ReturnShippingCost.Should().Be(30_000m);
        updateRepository.Verify(repository => repository.Update(order), Times.Once);
        unitOfWork.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static ParcelDeliveryOrder CreateReturnedOrder()
    {
        return new ParcelDeliveryOrder
        {
            Id = 10,
            TrackingNumber = "GHTK998877665",
            CustomerName = "Mai Thị J",
            CustomerPhone = "0900000000",
            CustomerAddress = "Biên Hòa",
            Carrier = "GHTK",
            Status = ParcelDeliveryStatus.Returned,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ReturnReason = "Sản phẩm trầy xước trong quá trình ship",
            BoxCondition = "Còn nguyên vẹn",
            ProductCondition = "Sử dụng tốt",
            ReturnProofImage = "/uploads/returns/rma-010.jpg",
            ReturnInternalNote = "Kho đã kiểm định",
            Items = []
        };
    }
}
