using Application.ApiContracts.Output.Responses;
using Application.Features.Outputs.Commands.CreateOutput;
using Application.Features.Outputs.Commands.CreateOutputByManager;
using Application.Features.Outputs.Commands.DeleteOutput;
using Application.Features.Outputs.Commands.DeleteManyOutputs;
using Application.Features.Outputs.Commands.RestoreOutput;
using Application.Features.Outputs.Commands.RestoreManyOutputs;
using Application.Features.Outputs.Commands.UpdateOutput;
using Application.Features.Outputs.Commands.UpdateOutputForManager;
using Application.Features.Outputs.Commands.UpdateOutputStatus;
using Application.Features.Outputs.Commands.UpdateManyOutputStatus;
using Application.Features.Outputs.Queries.GetOutputById;
using Application.Features.Outputs.Queries.GetOutputsByUserId;
using Application.Features.Outputs.Queries.GetOutputsByUserIdByManager;
using Application.Features.Outputs.Queries.GetOutputsList;
using Application.Features.Outputs.Queries.GetDeletedOutputsList;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Constants;
using Domain.Primitives;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using Sieve.Models;
using Xunit;

namespace UnitTests;

public class SalesOrder
{
    private readonly Mock<IOutputInsertRepository> _insertRepoMock;
    private readonly Mock<IOutputUpdateRepository> _updateRepoMock;
    private readonly Mock<IOutputDeleteRepository> _deleteRepoMock;
    private readonly Mock<IOutputReadRepository> _readRepoMock;
    private readonly Mock<IProductVariantReadRepository> _variantRepoMock;
    private readonly Mock<IUserReadRepository> _userRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISievePaginator> _paginatorMock;

    public SalesOrder()
    {
        _insertRepoMock = new Mock<IOutputInsertRepository>();
        _updateRepoMock = new Mock<IOutputUpdateRepository>();
        _deleteRepoMock = new Mock<IOutputDeleteRepository>();
        _readRepoMock = new Mock<IOutputReadRepository>();
        _variantRepoMock = new Mock<IProductVariantReadRepository>();
        _userRepoMock = new Mock<IUserReadRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _paginatorMock = new Mock<ISievePaginator>();
    }

    [Fact(DisplayName = "SO_001 - CreateOutput tạo đơn hàng thành công")]
    public async Task CreateOutput_ValidRequest_ShouldCallInsertRepository()
    {
        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputCommand
        {
            OutputInfos =
            [
                new() { ProductId = 1, Count = 5 }
            ]
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().BeGreaterThan(0);
        _insertRepoMock.Verify(x => x.Add(It.IsAny<Output>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_002 - CreateOutput validates BuyerId không null")]
    public async Task CreateOutput_WithNullBuyerId_ShouldStillProcess()
    {
        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputCommand
        {
            OutputInfos = [new() { ProductId = 1, Count = 1 }]
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().BeGreaterThan(0);
        _insertRepoMock.Verify(x => x.Add(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_003 - CreateOutput tính toán COGS đúng")]
    public async Task CreateOutput_ShouldCalculateTotalCOGS()
    {
        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputCommand
        {
            OutputInfos =
            [
                new() { ProductId = 1, Count = 2  },
                new() { ProductId = 2, Count = 3 }
            ]
        };

        Output? capturedOutput = null;
        _insertRepoMock.Setup(x => x.Add(It.IsAny<Output>()))
            .Callback<Output>((output) => capturedOutput = output);

        var result = await handler.Handle(command, CancellationToken.None);

        capturedOutput.Should().NotBeNull();
        capturedOutput!.OutputInfos.Should().HaveCount(2);
    }

    [Fact(DisplayName = "SO_004 - CreateOutput tạo OutputInfo cho từng sản phẩm")]
    public async Task CreateOutput_WithMultipleProducts_ShouldCreateOutputInfos()
    {
        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputCommand
        {
            OutputInfos =
            [
                new() { ProductId = 1, Count = 1},
                new() { ProductId = 2, Count = 1 },
                new() { ProductId = 3, Count = 1 }
            ]
        };

        Output? capturedOutput = null;
        _insertRepoMock.Setup(x => x.Add(It.IsAny<Output>()))
            .Callback<Output>((output) => capturedOutput = output);

        var result = await handler.Handle(command, CancellationToken.None);

        capturedOutput.Should().NotBeNull();
        capturedOutput!.OutputInfos.Should().HaveCount(3);
    }

    [Fact(DisplayName = "SO_005 - CreateOutput set trạng thái Pending mặc định")]
    public async Task CreateOutput_WithoutStatusId_ShouldDefaultToPending()
    {
        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputCommand
        {
            BuyerId = Guid.NewGuid(),
            OutputInfos = [new() { ProductId = 1, Count = 1 }]
        };

        Output? capturedOutput = null;
        _insertRepoMock.Setup(x => x.Add(It.IsAny<Output>()))
            .Callback<Output>((output) => capturedOutput = output);

        var result = await handler.Handle(command, CancellationToken.None);

        capturedOutput.Should().NotBeNull();
        result.Value!.StatusId.Should().Be("pending");
    }

    [Fact(DisplayName = "SO_006 - UpdateOutputStatus kiểm tra transition hợp lệ")]
    public async Task UpdateOutputStatus_ValidTransition_ShouldUpdateStatus()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand
        {
            Id = 1,
            StatusId = "confirmed_cod",
            CurrentUserId = Guid.NewGuid()
        };

        var existingOutput = new Output { Id = 1, StatusId = "pending" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None);

        existingOutput.StatusId.Should().Be("confirmed_cod");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_007 - UpdateOutputStatus từ Pending -> ConfirmedCod")]
    public async Task UpdateOutputStatus_PendingToConfirmedCod_ShouldSucceed()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand
        {
            Id = 1,
            StatusId = "confirmed_cod",
            CurrentUserId = Guid.NewGuid()
        };

        var existingOutput = new Output { Id = 1, StatusId = "pending" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None);

        existingOutput.StatusId.Should().Be("confirmed_cod");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_008 - UpdateOutputStatus từ ConfirmedCod -> Delivering")]
    public async Task UpdateOutputStatus_ConfirmedCodToDelivering_ShouldSucceed()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand
        {
            Id = 1,
            StatusId = "delivering",
            CurrentUserId = Guid.NewGuid()
        };

        var existingOutput = new Output { Id = 1, StatusId = "confirmed_cod" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None);

        existingOutput.StatusId.Should().Be("delivering");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_009 - UpdateOutputStatus từ Delivering -> Completed")]
    public async Task UpdateOutputStatus_DeliveringToCompleted_ShouldSucceed()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand
        {
            Id = 1,
            StatusId = "completed",
            CurrentUserId = Guid.NewGuid()
        };

        var existingOutput = new Output { Id = 1, StatusId = "delivering" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None);

        existingOutput.StatusId.Should().Be("completed");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_010 - UpdateOutputStatus từ Pending -> Deposit50")]
    public async Task UpdateOutputStatus_PendingToDeposit50_ShouldSucceed()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand
        {
            Id = 1,
            StatusId = "deposit_50",
            CurrentUserId = Guid.NewGuid()
        };

        var existingOutput = new Output { Id = 1, StatusId = "pending" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None);

        existingOutput.StatusId.Should().Be("deposit_50");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_011 - UpdateOutputStatus từ Deposit50 -> Confirmed50")]
    public async Task UpdateOutputStatus_Deposit50ToConfirmed50_ShouldSucceed()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand
        {
            Id = 1,
            StatusId = "confirmed_50",
            CurrentUserId = Guid.NewGuid()
        };

        var existingOutput = new Output { Id = 1, StatusId = "deposit_50" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None);

        existingOutput.StatusId.Should().Be("confirmed_50");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_012 - UpdateOutputStatus từ Confirmed50 -> Delivering")]
    public async Task UpdateOutputStatus_Confirmed50ToDelivering_ShouldSucceed()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand
        {
            Id = 1,
            StatusId = "delivering",
            CurrentUserId = Guid.NewGuid()
        };

        var existingOutput = new Output { Id = 1, StatusId = "confirmed_50" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None);

        existingOutput.StatusId.Should().Be("delivering");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_013 - UpdateOutputStatus từ Pending -> Refund")]
    public async Task UpdateOutputStatus_PendingToRefund_ShouldSucceed()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand
        {
            Id = 1,
            StatusId = "refund",
            CurrentUserId = Guid.NewGuid()
        };

        var existingOutput = new Output { Id = 1, StatusId = "pending" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None);

        existingOutput.StatusId.Should().Be("refund");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_014 - UpdateOutputStatus chặn transition không hợp lệ")]
    public async Task UpdateOutputStatus_InvalidTransition_ShouldThrowException()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand
        {
            Id = 1,
            StatusId = "completed",
            CurrentUserId = Guid.NewGuid()
        };

        var existingOutput = new Output { Id = 1, StatusId = "pending" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_015 - UpdateOutputStatus cập nhật LastStatusChangedAt")]
    public async Task UpdateOutputStatus_ShouldUpdateLastStatusChangedAt()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand
        {
            Id = 1,
            StatusId = "confirmed_cod",
            CurrentUserId = Guid.NewGuid()
        };

        var existingOutput = new Output { Id = 1, StatusId = "pending" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None);

        existingOutput.LastStatusChangedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "SO_016 - UpdateOutputStatus set FinishedBy khi Completed")]
    public async Task UpdateOutputStatus_ToCompleted_ShouldSetFinishedBy()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var currentUserId = Guid.NewGuid();
        var command = new UpdateOutputStatusCommand
        {
            Id = 1,
            StatusId = "completed",
            CurrentUserId = currentUserId
        };

        var existingOutput = new Output { Id = 1, StatusId = "delivering" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None);

        existingOutput.FinishedBy.Should().Be(currentUserId);
    }

    [Fact(DisplayName = "SO_017 - CreateOutputByAdmin kiểm tra quyền")]
    public async Task CreateOutputByAdmin_ShouldAllowManagerToCreate()
    {
        var handler = new CreateOutputByManagerCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _userRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputByManagerCommand
        {
            BuyerId = Guid.NewGuid(),
            StatusId = "pending",
            OutputInfos = [new() { ProductId = 1, Count = 1 }]
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().BeGreaterThan(0);
        _insertRepoMock.Verify(x => x.Add(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_018 - CreateOutputByAdmin cho phép set BuyerId")]
    public async Task CreateOutputByAdmin_ShouldAllowCustomBuyerId()
    {
        var handler = new CreateOutputByManagerCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _userRepoMock.Object,
            _unitOfWorkMock.Object);

        var customBuyerId = Guid.NewGuid();
        var command = new CreateOutputByManagerCommand
        {
            BuyerId = customBuyerId,
            StatusId = "pending",
            OutputInfos = [new() { ProductId = 1, Count = 1 }]
        };

        Output? capturedOutput = null;
        _insertRepoMock.Setup(x => x.Add(It.IsAny<Output>()))
            .Callback<Output>((output) => capturedOutput = output);

        var result = await handler.Handle(command, CancellationToken.None);

        capturedOutput.Should().NotBeNull();
        result.Value!.BuyerId.Should().Be(customBuyerId);
    }

    [Fact(DisplayName = "SO_019 - UpdateOutputForManager kiểm tra ownership")]
    public async Task UpdateOutputForManager_ShouldCheckOwnership()
    {
        var handler = new UpdateOutputForManagerCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _deleteRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputForManagerCommand
        {
            Id = 1,
            OutputInfos = [new() { ProductId = 1, Count = 2 }]
        };

        var existingOutput = new Output { Id = 1, CreatedBy = Guid.NewGuid() };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None);

        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_020 - UpdateOutput kiểm tra quyền Manager")]
    public async Task UpdateOutput_ShouldRequireManagerPermission()
    {
        var handler = new UpdateOutputCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _deleteRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputCommand
        {
            Id = 1,
            OutputInfos = [new() { ProductId = 1, Count = 3 }]
        };

        var existingOutput = new Output { Id = 1, StatusId = "pending" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None);

        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_021 - DeleteOutput soft delete")]
    public async Task DeleteOutput_ShouldPerformSoftDelete()
    {
        var handler = new DeleteOutputCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new DeleteOutputCommand() { Id = 1 };

        var existingOutput = new Output { Id = 1, StatusId = "pending" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None);

        _deleteRepoMock.Verify(x => x.Delete(It.IsAny<Output>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_022 - DeleteOutput set DeletedAt timestamp")]
    public async Task DeleteOutput_ShouldSetDeletedAtTimestamp()
    {
        var handler = new DeleteOutputCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new DeleteOutputCommand() {  Id = 1};

        var existingOutput = new Output { Id = 1, StatusId = "pending" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None);

        existingOutput.DeletedAt.Should().NotBeNull();
        existingOutput.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "SO_023 - RestoreOutput xóa DeletedAt")]
    public async Task RestoreOutput_ShouldClearDeletedAt()
    {
        var handler = new RestoreOutputCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new RestoreOutputCommand() { Id = 1 };

        var deletedOutput = new Output { Id = 1, DeletedAt = DateTime.UtcNow };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), DataFetchMode.All))
            .ReturnsAsync(deletedOutput);

        await handler.Handle(command, CancellationToken.None);

        deletedOutput.DeletedAt.Should().BeNull();
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_024 - DeleteManyOutputs xóa nhiều đơn")]
    public async Task DeleteManyOutputs_ShouldDeleteMultipleOrders()
    {
        var handler = new DeleteManyOutputsCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new DeleteManyOutputsCommand { Ids = [1, 2, 3] };

        var existingOutputs = new List<Output>
        {
            new() { Id = 1, StatusId = "pending" },
            new() { Id = 2, StatusId = "pending" },
            new() { Id = 3, StatusId = "pending" }
        };
        _readRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutputs);

        await handler.Handle(command, CancellationToken.None);

        _deleteRepoMock.Verify(x => x.Delete(It.IsAny<IEnumerable<Output>>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_025 - RestoreManyOutputs khôi phục nhiều đơn")]
    public async Task RestoreManyOutputs_ShouldRestoreMultipleOrders()
    {
        var handler = new RestoreManyOutputsCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new RestoreManyOutputsCommand { Ids = [1, 2, 3] };

        var deletedOutputs = new List<Output>
        {
            new() { Id = 1, DeletedAt = DateTime.UtcNow },
            new() { Id = 2, DeletedAt = DateTime.UtcNow },
            new() { Id = 3, DeletedAt = DateTime.UtcNow }
        };
        _readRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), DataFetchMode.All))
            .ReturnsAsync(deletedOutputs);

        await handler.Handle(command, CancellationToken.None);

        deletedOutputs.Should().AllSatisfy(x => x.DeletedAt.Should().BeNull());
    }

    [Fact(DisplayName = "SO_026 - UpdateManyOutputStatus cập nhật nhiều đơn")]
    public async Task UpdateManyOutputStatus_ShouldUpdateMultipleOrders()
    {
        var handler = new UpdateManyOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateManyOutputStatusCommand
        {
            Ids = [1, 2, 3],
            StatusId = "confirmed_cod"
        };

        var existingOutputs = new List<Output>
        {
            new () { Id = 1, StatusId = "pending" },
            new () { Id = 2, StatusId = "pending" },
            new () { Id = 3, StatusId = "pending" }
        };
        _readRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutputs);

        await handler.Handle(command, CancellationToken.None);

        existingOutputs.Should().AllSatisfy(x => x.StatusId.Should().Be("confirmed_cod"));
    }

    [Fact(DisplayName = "SO_027 - GetOutputById trả về đơn hàng")]
    public async Task GetOutputById_WithValidId_ShouldReturnOrder()
    {
        var handler = new GetOutputByIdQueryHandler(_readRepoMock.Object);

        var query = new GetOutputByIdQuery() { Id = 1 };

        var existingOutput = new Output { Id = 1, StatusId = "pending" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Value!.Id.Should().Be(1);
    }

    [Fact(DisplayName = "SO_028 - GetOutputsByUserId chỉ lấy đơn của user")]
    public async Task GetOutputsByUserId_ShouldReturnOnlyUserOrders()
    {
        var handler = new GetOutputsByUserrIdQueryHandler(
            _readRepoMock.Object,
            _paginatorMock.Object);

        var userId = Guid.NewGuid();
        var query = new GetOutputsByUserIdQuery() { BuyerId = userId, SieveModel = new SieveModel() };

        var userOutputs = new List<Output>
        {
            new () { Id = 1, BuyerId = userId },
            new () { Id = 2, BuyerId = userId }
        }.AsQueryable();

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(userOutputs);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        _paginatorMock.Verify(x => x.ApplyAsync<Output, OutputResponse>(It.IsAny<IQueryable<Output>>(), It.IsAny<SieveModel>(), It.IsAny<DataFetchMode?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_029 - GetOutputsList hỗ trợ phân trang")]
    public async Task GetOutputsList_ShouldSupportPagination()
    {
        var handler = new GetOutputsListQueryHandler(
            _readRepoMock.Object,
            _paginatorMock.Object);

        var query = new GetOutputsListQuery() { SieveModel = new SieveModel { Page = 1, PageSize = 10 } };

        var outputs = new List<Output>
        {
            new () { Id = 1 },
            new () { Id = 2 }
        }.AsQueryable();

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(outputs);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        _paginatorMock.Verify(x => x.ApplyAsync<Output, OutputResponse>(It.IsAny<IQueryable<Output>>(), It.IsAny<SieveModel>(), It.IsAny<DataFetchMode?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_030 - GetOutputsList filter theo status")]
    public async Task GetOutputsList_ShouldFilterByStatus()
    {
        var handler = new GetOutputsListQueryHandler(
            _readRepoMock.Object,
            _paginatorMock.Object);

        var query = new GetOutputsListQuery() { SieveModel = new SieveModel { Filters = "StatusId==pending" } };

        var outputs = new List<Output>
        {
            new () { Id = 1, StatusId = "pending" },
            new () { Id = 2, StatusId = "confirmed_cod" }
        }.AsQueryable();

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(outputs);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        _paginatorMock.Verify(x => x.ApplyAsync<Output, OutputResponse>(It.IsAny<IQueryable<Output>>(), It.IsAny<SieveModel>(), It.IsAny<DataFetchMode?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_031 - GetOutputsList sort theo CreatedAt")]
    public async Task GetOutputsList_ShouldSortByCreatedAt()
    {
        var handler = new GetOutputsListQueryHandler(
            _readRepoMock.Object,
            _paginatorMock.Object);

        var query = new GetOutputsListQuery() { SieveModel = new SieveModel { Sorts = "-CreatedAt" } };

        var outputs = new List<Output>
        {
            new () { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new () { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-1) }
        }.AsQueryable();

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(outputs);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        _paginatorMock.Verify(x => x.ApplyAsync<Output, OutputResponse>(It.IsAny<IQueryable<Output>>(), It.IsAny<SieveModel>(), It.IsAny<DataFetchMode?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_032 - GetDeletedOutputsList chỉ lấy đơn đã xóa")]
    public async Task GetDeletedOutputsList_ShouldReturnOnlyDeletedOrders()
    {
        var handler = new GetDeletedOutputsListQueryHandler(
            _readRepoMock.Object,
            _paginatorMock.Object);

        var query = new GetDeletedOutputsListQuery() { SieveModel = new SieveModel()};

        var deletedOutputs = new List<Output>
        {
            new () { Id = 1, DeletedAt = DateTime.UtcNow },
            new () { Id = 2, DeletedAt = DateTime.UtcNow }
        }.AsQueryable();

        _readRepoMock.Setup(x => x.GetQueryable(DataFetchMode.DeletedOnly)).Returns(deletedOutputs);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        _paginatorMock.Verify(x => x.ApplyAsync<Output, OutputResponse>(It.IsAny<IQueryable<Output>>(), It.IsAny<SieveModel>(), It.IsAny<DataFetchMode?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_033 - CreateOutput với nhiều sản phẩm")]
    public async Task CreateOutput_WithManyProducts_ShouldProcessAll()
    {
        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputCommand
        {
            BuyerId = Guid.NewGuid(),
            OutputInfos =
            [
                new() { ProductId = 1, Count = 5 },
                new() { ProductId = 2, Count = 3 },
                new() { ProductId = 3, Count = 2 },
                new() { ProductId = 4, Count = 1 }
            ]
        };

        Output? capturedOutput = null;
        _insertRepoMock.Setup(x => x.Add(It.IsAny<Output>()))
            .Callback<Output>((output) => capturedOutput = output);

        var result = await handler.Handle(command, CancellationToken.None);

        capturedOutput.Should().NotBeNull();
        capturedOutput!.OutputInfos.Should().HaveCount(4);
    }

    [Fact(DisplayName = "SO_034 - CreateOutput tính tổng tiền")]
    public async Task CreateOutput_ShouldCalculateTotalAmount()
    {
        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputCommand
        {
            BuyerId = Guid.NewGuid(),
            OutputInfos =
            [
                new() { ProductId = 1, Count = 2 },
                new() { ProductId = 2, Count = 3}
            ]
        };

        Output? capturedOutput = null;
        _insertRepoMock.Setup(x => x.Add(It.IsAny<Output>()))
            .Callback<Output>((output) => capturedOutput = output);

        var result = await handler.Handle(command, CancellationToken.None);

        capturedOutput.Should().NotBeNull();
        capturedOutput!.OutputInfos.Should().HaveCount(2);
    }

    [Fact(DisplayName = "SO_035 - CreateOutput validation số lượng > 0")]
    public async Task CreateOutput_WithZeroQuantity_ShouldThrowException()
    {
        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputCommand
        {
            BuyerId = Guid.NewGuid(),
            OutputInfos = 
            {
                new() { ProductId = 1, Count = 0 }
            }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_036 - CreateOutput validation ProductId exists")]
    public async Task CreateOutput_WithNonExistentProduct_ShouldThrowException()
    {
        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputCommand
        {
            BuyerId = Guid.NewGuid(),
            OutputInfos =
            [
                new() { ProductId = 999, Count = 1 }
            ]
        };

        _variantRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync([]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_037 - UpdateOutput không thể sửa sau khi Completed")]
    public async Task UpdateOutput_CompletedOrder_ShouldThrowException()
    {
        var handler = new UpdateOutputCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _deleteRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputCommand
        {
            Id = 1,
            OutputInfos =  { new() { ProductId = 1, Count = 5 } }
        };

        var completedOutput = new Output { Id = 1, StatusId = "completed" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(completedOutput);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_038 - UpdateOutput kiểm tra soft deleted")]
    public async Task UpdateOutput_DeletedOrder_ShouldThrowException()
    {
        var handler = new UpdateOutputCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _deleteRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputCommand
        {
            Id = 1,
            OutputInfos =  { new() { ProductId = 1, Count = 5 } }
        };

        var deletedOutput = new Output { Id = 1, DeletedAt = DateTime.UtcNow };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(deletedOutput);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_039 - DeleteOutput không xóa đơn đã Completed")]
    public async Task DeleteOutput_CompletedOrder_ShouldThrowException()
    {
        var handler = new DeleteOutputCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new DeleteOutputCommand() { Id = 1 };

        var completedOutput = new Output { Id = 1, StatusId = "completed" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(completedOutput);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_040 - RestoreOutput chỉ khôi phục đơn đã xóa")]
    public async Task RestoreOutput_NonDeletedOrder_ShouldThrowException()
    {
        var handler = new RestoreOutputCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new RestoreOutputCommand() { Id = 1 };

        var activeOutput = new Output { Id = 1, DeletedAt = null };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), DataFetchMode.All))
            .ReturnsAsync(activeOutput);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_041 - UpdateOutputStatus lưu FinishedBy")]
    public async Task UpdateOutputStatus_ToCompleted_ShouldSaveFinishedBy()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var finishedBy = Guid.NewGuid();
        var command = new UpdateOutputStatusCommand
        {
            Id = 1,
            StatusId = "completed",
            CurrentUserId = finishedBy
        };

        var existingOutput = new Output { Id = 1, StatusId = "delivering" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None);

        existingOutput.FinishedBy.Should().Be(finishedBy);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_042 - CreateOutput set CreatedBy")]
    public async Task CreateOutput_ShouldSetCreatedBy()
    {
        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var createdBy = Guid.NewGuid();
        var command = new CreateOutputCommand
        {
            BuyerId = createdBy,
            OutputInfos =  { new() { ProductId = 1, Count = 1 } }
        };

        Output? capturedOutput = null;
        _insertRepoMock.Setup(x => x.Add(It.IsAny<Output>()))
            .Callback<Output>((output) => capturedOutput = output);

        var result = await handler.Handle(command, CancellationToken.None);

        capturedOutput.Should().NotBeNull();
        capturedOutput!.CreatedBy.Should().Be(createdBy);
    }

    [Fact(DisplayName = "SO_043 - GetOutputById không trả về đơn đã xóa")]
    public async Task GetOutputById_DeletedOrder_ShouldReturnNull()
    {
        var handler = new GetOutputByIdQueryHandler(_readRepoMock.Object);

        var query = new GetOutputByIdQuery() { Id = 1 };

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync((Output?)null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_044 - GetOutputsList exclude soft deleted")]
    public async Task GetOutputsList_ShouldExcludeSoftDeleted()
    {
        var handler = new GetOutputsListQueryHandler(
            _readRepoMock.Object,
            _paginatorMock.Object);

        var query = new GetOutputsListQuery() { SieveModel = new SieveModel() };

        var outputs = new List<Output>
        {
            new () { Id = 1, DeletedAt = null },
            new () { Id = 2, DeletedAt = null }
        }.AsQueryable();

        _readRepoMock.Setup(x => x.GetQueryable(DataFetchMode.ActiveOnly)).Returns(outputs);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        _readRepoMock.Verify(x => x.GetQueryable(DataFetchMode.ActiveOnly), Times.Once);
    }

    [Fact(DisplayName = "SO_045 - CreateOutput validation BuyerId not empty")]
    public async Task CreateOutput_WithEmptyBuyerId_ShouldThrowException()
    {
        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputCommand
        {
            BuyerId = Guid.Empty,
            OutputInfos =  { new() { ProductId = 1, Count = 1 } }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_046 - CreateOutput validation Products not empty")]
    public async Task CreateOutput_WithNoProducts_ShouldThrowException()
    {
        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputCommand
        {
            BuyerId = Guid.NewGuid(),
            OutputInfos = []
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_047 - UpdateOutputStatus validation StatusId not empty")]
    public async Task UpdateOutputStatus_WithEmptyStatusId_ShouldThrowException()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand
        {
            Id = 1,
            StatusId = "",
            CurrentUserId = Guid.NewGuid()
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_048 - UpdateOutputStatus validation OrderId exists")]
    public async Task UpdateOutputStatus_WithNonExistentOrderId_ShouldThrowException()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand
        {
            Id = 999,
            StatusId = "confirmed_cod",
            CurrentUserId = Guid.NewGuid()
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync((Output?)null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_049 - UpdateManyOutputStatus validation Ids not empty")]
    public async Task UpdateManyOutputStatus_WithEmptyIds_ShouldThrowException()
    {
        var handler = new UpdateManyOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateManyOutputStatusCommand
        {
            Ids = [],
            StatusId = "confirmed_cod"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_050 - DeleteManyOutputs validation Ids not empty")]
    public async Task DeleteManyOutputs_WithEmptyIds_ShouldThrowException()
    {
        var handler = new DeleteManyOutputsCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new DeleteManyOutputsCommand { Ids = [] };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_051 - RestoreManyOutputs validation Ids not empty")]
    public async Task RestoreManyOutputs_WithEmptyIds_ShouldThrowException()
    {
        var handler = new RestoreManyOutputsCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new RestoreManyOutputsCommand { Ids = [] };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_052 - GetOutputsByUserId với pagination")]
    public async Task GetOutputsByUserId_ShouldSupportPagination()
    {
        var handler = new GetOutputsByUserrIdQueryHandler(
            _readRepoMock.Object,
            _paginatorMock.Object);

        var userId = Guid.NewGuid();
        var query = new GetOutputsByUserIdQuery() { BuyerId = userId, SieveModel = new SieveModel { Page = 2, PageSize = 5 } };

        var outputs = new List<Output>
        {
            new () { Id = 1, BuyerId = userId },
            new () { Id = 2, BuyerId = userId }
        }.AsQueryable();

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(outputs);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        _paginatorMock.Verify(x => x.ApplyAsync<Output, OutputResponse>(It.IsAny<IQueryable<Output>>(), It.IsAny<SieveModel>(), It.IsAny<DataFetchMode?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_053 - GetOutputsByUserIdByManager kiểm tra quyền")]
    public async Task GetOutputsByUserIdByManager_ShouldRequireManagerPermission()
    {
        var handler = new GetOutputsByUserIdByManagerQueryHandler(
            _readRepoMock.Object,
            _paginatorMock.Object);

        var userId = Guid.NewGuid();
        var query = new GetOutputsByUserIdQuery() { BuyerId = userId, SieveModel = new SieveModel() };

        var outputs = new List<Output>
        {
            new () { Id = 1, BuyerId = userId }
        }.AsQueryable();

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(outputs);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        _paginatorMock.Verify(x => x.ApplyAsync<Output, OutputResponse>(It.IsAny<IQueryable<Output>>(), It.IsAny<SieveModel>(), It.IsAny<DataFetchMode?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_054 - UpdateOutput kiểm tra ProductId trong products")]
    public async Task UpdateOutput_WithInvalidProductId_ShouldThrowException()
    {
        var handler = new UpdateOutputCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _deleteRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputCommand
        {
            Id = 1,
            OutputInfos = 
            {
                new() { ProductId = 999, Count = 1 }
            }
        };

        var existingOutput = new Output { Id = 1, StatusId = "pending" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        _variantRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync([]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_055 - CreateOutput với product đã bị xóa")]
    public async Task CreateOutput_WithDeletedProduct_ShouldThrowException()
    {
        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputCommand
        {
            BuyerId = Guid.NewGuid(), 
            OutputInfos =
            [
                new() { ProductId = 1, Count = 1 }
            ]
        };

        var deletedVariant = new ProductVariant { Id = 1, DeletedAt = DateTime.UtcNow };
        _variantRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), DataFetchMode.All))
            .ReturnsAsync([deletedVariant]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_056 - UpdateOutputStatus từ ConfirmedCod -> Refund")]
    public async Task UpdateOutputStatus_ConfirmedCodToRefund_ShouldSucceed()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand
        {
            Id = 1,
            StatusId = "refund",
            CurrentUserId = Guid.NewGuid()
        };

        var existingOutput = new Output { Id = 1, StatusId = "confirmed_cod" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None);

        existingOutput.StatusId.Should().Be("refund");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_057 - CreateOutput tạo với Notes")]
    public async Task CreateOutput_WithNotes_ShouldSaveNotes()
    {
        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputCommand
        {
            BuyerId = Guid.NewGuid(),
            Notes = "Giao hàng trước 5pm",
            OutputInfos = [new() { ProductId = 1, Count = 1 }]
        };

        Output? capturedOutput = null;
        _insertRepoMock.Setup(x => x.Add(It.IsAny<Output>()))
            .Callback<Output>((output) => capturedOutput = output);

        var result = await handler.Handle(command, CancellationToken.None);

        capturedOutput.Should().NotBeNull();
        result.Value!.Notes.Should().Be("Giao hàng trước 5pm");
    }

    [Fact(DisplayName = "SO_058 - UpdateOutput cập nhật Notes")]
    public async Task UpdateOutput_ShouldAllowUpdatingNotes()
    {
        var handler = new UpdateOutputCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _deleteRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputCommand
        {
            Id = 1,
            Notes = "Cập nhật: Giao vào sáng mai",
            OutputInfos = [new() { ProductId = 1, Count = 2 }]
        };

        var existingOutput = new Output { Id = 1, StatusId = "pending", Notes = "Old notes" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None);

        existingOutput.Notes.Should().Be("Cập nhật: Giao vào sáng mai");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_059 - GetOutputsList search theo CustomerName")]
    public async Task GetOutputsList_ShouldSearchByCustomerName()
    {
        var handler = new GetOutputsListQueryHandler(
            _readRepoMock.Object,
            _paginatorMock.Object);

        var query = new GetOutputsListQuery() { SieveModel = new SieveModel { Filters = "CustomerName@=Nguyen" } };

        var outputs = new List<Output>
        {
            new () { Id = 1, CustomerName = "Nguyen Van A" },
            new () { Id = 2, CustomerName = "Nguyen Thi B" }
        }.AsQueryable();

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(outputs);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        _paginatorMock.Verify(x => x.ApplyAsync<Output, OutputResponse>(It.IsAny<IQueryable<Output>>(), It.IsAny<SieveModel>(), It.IsAny<DataFetchMode?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_060 - GetOutputsList filter theo date range")]
    public async Task GetOutputsList_ShouldFilterByDateRange()
    {
        var handler = new GetOutputsListQueryHandler(
            _readRepoMock.Object,
            _paginatorMock.Object);

        var query = new GetOutputsListQuery() { SieveModel = new SieveModel { Filters = "CreatedAt>=2024-01-01,CreatedAt<=2024-12-31" } };

        var outputs = new List<Output>
        {
            new () { Id = 1, CreatedAt = new DateTime(2024, 6, 1) },
            new () { Id = 2, CreatedAt = new DateTime(2024, 7, 1) }
        }.AsQueryable();

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(outputs);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        _paginatorMock.Verify(x => x.ApplyAsync<Output, OutputResponse>(It.IsAny<IQueryable<Output>>(), It.IsAny<SieveModel>(), It.IsAny<DataFetchMode?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
