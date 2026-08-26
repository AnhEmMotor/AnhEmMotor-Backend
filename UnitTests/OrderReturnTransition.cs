using Application.Common.Models;
using Application.Features.Outputs.Commands.UpdateOutputStatus;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Commission;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ReturnRequest;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class OrderReturnTransition
{
    [Theory(DisplayName = "TRANS_001 - Bản đồ chuyển trạng thái cho phép completed→refunding, chặn các hướng sai")]
    [InlineData("completed", "refunding", true)]
    [InlineData("completed", "cancelled", false)]
    [InlineData("completed", "completed", false)]
    [InlineData("refunded", "refunding", false)]
    [InlineData("delivering", "refunding", true)]
    public void TransitionMap_AllowsCompletedToRefundingOnly(
        string currentStatus,
        string newStatus,
        bool expected)
    {
        OrderStatusTransitions.IsTransitionAllowed(currentStatus, newStatus).Should().Be(expected);
    }

    [Fact(DisplayName = "TRANS_002 - completed→refunding KHÔNG có yêu cầu trả hàng: bị chặn")]
    public async Task Guard_BlocksCompletedToRefundingWithoutReturnRequest()
    {
        var readRepoMock = new Mock<IOutputReadRepository>();
        var updateRepoMock = new Mock<IOutputUpdateRepository>();
        var commissionRepoMock = new Mock<ICommissionUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var returnRequestRepoMock = new Mock<IReturnRequestReadRepository>();
        var existingOutput = new Output { Id = 1, StatusId = "completed" };
        readRepoMock
            .Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);
        returnRequestRepoMock
            .Setup(x => x.HasActiveReturnRequestAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new UpdateOutputStatusCommandHandler(
            readRepoMock.Object,
            updateRepoMock.Object,
            commissionRepoMock.Object,
            unitOfWorkMock.Object,
            returnRequestReadRepository: returnRequestRepoMock.Object);
        var result = await handler.Handle(
            new UpdateOutputStatusCommand { Id = 1, StatusId = "refunding", CurrentUserId = Guid.NewGuid() },
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Contain("yêu cầu trả hàng");
        existingOutput.StatusId.Should().Be("completed");
        commissionRepoMock.Verify(
            x => x.VoidCommissionAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "TRANS_003 - completed→refunding CÓ yêu cầu trả hàng: đi qua, void hoa hồng")]
    public async Task Guard_AllowsCompletedToRefundingWithActiveReturnRequest()
    {
        var readRepoMock = new Mock<IOutputReadRepository>();
        var updateRepoMock = new Mock<IOutputUpdateRepository>();
        var commissionRepoMock = new Mock<ICommissionUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var returnRequestRepoMock = new Mock<IReturnRequestReadRepository>();
        var existingOutput = new Output
        {
            Id = 1,
            StatusId = "completed",
            OutputInfos = [new OutputInfo { ProductVariantId = 100, Count = 5 }]
        };
        readRepoMock
            .Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);
        returnRequestRepoMock
            .Setup(x => x.HasActiveReturnRequestAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new UpdateOutputStatusCommandHandler(
            readRepoMock.Object,
            updateRepoMock.Object,
            commissionRepoMock.Object,
            unitOfWorkMock.Object,
            returnRequestReadRepository: returnRequestRepoMock.Object);
        var result = await handler.Handle(
            new UpdateOutputStatusCommand { Id = 1, StatusId = "refunding", CurrentUserId = Guid.NewGuid() },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        existingOutput.StatusId.Should().Be("refunding");
        commissionRepoMock.Verify(
            x => x.VoidCommissionAsync(1, It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
