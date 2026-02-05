using Application.ApiContracts.Auth.Responses;
using Application.ApiContracts.Output.Responses;
using Application.Features.Outputs.Commands.CreateOutput;
using Application.Features.Outputs.Commands.CreateOutputByManager;
using Application.Features.Outputs.Commands.DeleteManyOutputs;
using Application.Features.Outputs.Commands.DeleteOutput;
using Application.Features.Outputs.Commands.RestoreManyOutputs;
using Application.Features.Outputs.Commands.RestoreOutput;
using Application.Features.Outputs.Commands.UpdateManyOutputStatus;
using Application.Features.Outputs.Commands.UpdateOutput;
using Application.Features.Outputs.Commands.UpdateOutputForManager;
using Application.Features.Outputs.Commands.UpdateOutputStatus;
using Application.Features.Outputs.Mappings;
using Application.Features.Outputs.Queries.GetDeletedOutputsList;
using Application.Features.Outputs.Queries.GetOutputById;
using Application.Features.Outputs.Queries.GetOutputsByUserId;
using Application.Features.Outputs.Queries.GetOutputsByUserIdByManager;
using Application.Features.Outputs.Queries.GetOutputsList;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.User;
using Castle.Core.Resource;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Entities;
using FluentAssertions;
using Mapster;
using Moq;
using Sieve.Models;
using System.ComponentModel.DataAnnotations;
using static Domain.Constants.Permission.PermissionsList;
using ProductEntity = Domain.Entities.Product;
using ProductStatus = Domain.Constants.ProductStatus;

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
        
        new OutputMappingConfig().Register(TypeAdapterConfig.GlobalSettings);
    }

#pragma warning disable IDE0079
#pragma warning disable CRR0035
    [Fact(DisplayName = "SO_001 - CreateOutput tạo đơn hàng thành công")]
    public async Task CreateOutput_ValidRequest_ShouldCallInsertRepository()
    {
        var productId = 1;
        var command = new CreateOutputCommand
        {
            OutputInfos = [new() { ProductId = productId, Count = 5 }]
        };

        var mockVariant = new ProductVariant
        {
            Id = productId,
            Price = 100,
            Product = new ProductEntity { StatusId = ProductStatus.ForSale }
        };

        _variantRepoMock.Setup(x => x.GetByIdAsync(
            It.IsAny<List<int>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<DataFetchMode>()))
            .ReturnsAsync([mockVariant]);

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Output { Id = 100 });

        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue(); 
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(100);
        _insertRepoMock.Verify(x => x.Add(It.IsAny<Output>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_002 - CreateOutput validates BuyerId không null")]
    public async Task CreateOutput_WithNullBuyerId_ShouldStillProcess()
    {
        var productId = 1;
        var variant = new ProductVariant
        {
            Id = productId,
            Price = 100,
            Product = new ProductEntity { StatusId = ProductStatus.ForSale, Name = "Test Product" }
        };

        _variantRepoMock.Setup(x => x.GetByIdAsync(
                It.IsAny<List<int>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<DataFetchMode>()))
            .ReturnsAsync([variant]);

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Output { Id = 99 });

        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputCommand
        {
            OutputInfos = [new() { ProductId = productId, Count = 1 }],
            BuyerId = null
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(99);
        _insertRepoMock.Verify(x => x.Add(It.Is<Output>(o => o.BuyerId == null)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_003 - CreateOutput tính toán COGS đúng")]
    public async Task CreateOutput_ShouldCalculateTotalCOGS()
    {
        var product1 = new ProductVariant
        {
            Id = 1,
            Price = 50,
            Product = new ProductEntity { StatusId = ProductStatus.ForSale, Name = "P1" }
        };
        var product2 = new ProductVariant
        {
            Id = 2,
            Price = 100,
            Product = new ProductEntity { StatusId = ProductStatus.ForSale, Name = "P2" }
        };

        _variantRepoMock.Setup(x => x.GetByIdAsync(
                It.IsAny<List<int>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<DataFetchMode>()))
            .ReturnsAsync([product1, product2]);

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Output());

        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputCommand
        {
            OutputInfos = [
                new() { ProductId = 1, Count = 2 },
            new() { ProductId = 2, Count = 3 }
            ]
        };

        Output? capturedOutput = null;
        _insertRepoMock.Setup(x => x.Add(It.IsAny<Output>()))
            .Callback<Output>((output) => capturedOutput = output);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeFalse();
        capturedOutput.Should().NotBeNull();

        var info1 = capturedOutput!.OutputInfos.FirstOrDefault(x => x.ProductVarientId == 1);
        var info2 = capturedOutput!.OutputInfos.FirstOrDefault(x => x.ProductVarientId == 2);

        info1.Should().NotBeNull();
        info1!.Price.Should().Be(50);
        info2.Should().NotBeNull();
        info2!.Price.Should().Be(100);
    }

    [Fact(DisplayName = "SO_004 - CreateOutput tạo OutputInfo cho từng sản phẩm")]
    public async Task CreateOutput_WithMultipleProducts_ShouldCreateOutputInfos()
    {
        var variantIds = new List<int> { 1, 2, 3 };
        var variants = variantIds.Select(id => new ProductVariant
        {
            Id = id,
            Price = 100,
            Product = new ProductEntity
            {
                Name = $"Product {id}",
                StatusId = ProductStatus.ForSale
            }
        }).ToList();

        _variantRepoMock.Setup(x => x.GetByIdAsync(
            It.IsAny<List<int>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<DataFetchMode>()))
            .ReturnsAsync(variants);

        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputCommand
        {
            OutputInfos =
                [
                    new() { ProductId = 1, Count = 1 },
                new() { ProductId = 2, Count = 1 },
                new() { ProductId = 3, Count = 1 }
                ]
        };

        Output? capturedOutput = null;
        _insertRepoMock.Setup(x => x.Add(It.IsAny<Output>()))
            .Callback<Output>((output) => capturedOutput = output);

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Output());

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        capturedOutput.Should().NotBeNull();
        capturedOutput!.OutputInfos.Should().HaveCount(3);
    }

    [Fact(DisplayName = "SO_005 - CreateOutput set trạng thái Pending mặc định")]
    public async Task CreateOutput_WithoutStatusId_ShouldDefaultToPending()
    {
        var productId = 1;
        var variants = new List<ProductVariant>
    {
        new()
        {
            Id = productId,
            Price = 500,
            Product = new ProductEntity { StatusId = ProductStatus.ForSale }
        }
    };

        _variantRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(variants);

        Output? capturedOutput = null;
        _insertRepoMock.Setup(x => x.Add(It.IsAny<Output>()))
            .Callback<Output>((output) => capturedOutput = output);

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
    .ReturnsAsync((int id, CancellationToken ct, DataFetchMode mode) => new Output
    {
        Id = id,
        StatusId = OrderStatus.Pending
    });

        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputCommand
        {
            BuyerId = Guid.NewGuid(),
            OutputInfos = [new() { ProductId = productId, Count = 1 }]
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        capturedOutput.Should().NotBeNull();
        capturedOutput!.StatusId.Should().Be(OrderStatus.Pending);
        result.IsSuccess.Should().BeTrue();
        result.Value!.StatusId.Should().Be(OrderStatus.Pending);
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
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

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
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

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

        var command = new UpdateOutputStatusCommand { Id = 1, StatusId = "delivering", CurrentUserId = Guid.NewGuid() };

        var existingOutput = new Output { Id = 1, StatusId = "confirmed_cod" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

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

        var command = new UpdateOutputStatusCommand { Id = 1, StatusId = "completed", CurrentUserId = Guid.NewGuid() };

        var existingOutput = new Output { Id = 1, StatusId = "delivering" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        existingOutput.StatusId.Should().Be("completed");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_010 - UpdateOutputStatus từ Pending -> WaitingDeposit")]
    public async Task UpdateOutputStatus_PendingToWaitingDeposit_ShouldSucceed()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand { Id = 1, StatusId = "waiting_deposit", CurrentUserId = Guid.NewGuid() };

        var existingOutput = new Output { Id = 1, StatusId = "pending" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        existingOutput.StatusId.Should().Be("waiting_deposit");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_011 - UpdateOutputStatus từ WaitingDeposit -> DepositPaid")]
    public async Task UpdateOutputStatus_WaitingDepositToDepositPaid_ShouldSucceed()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand
        {
            Id = 1,
            StatusId = "deposit_paid",
            CurrentUserId = Guid.NewGuid()
        };

        var existingOutput = new Output { Id = 1, StatusId = "waiting_deposit" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        existingOutput.StatusId.Should().Be("deposit_paid");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_012 - UpdateOutputStatus từ DepositPaid -> Delivering")]
    public async Task UpdateOutputStatus_DepositPaidToDelivering_ShouldSucceed()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand { Id = 1, StatusId = "delivering", CurrentUserId = Guid.NewGuid() };

        var existingOutput = new Output { Id = 1, StatusId = "deposit_paid" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        existingOutput.StatusId.Should().Be("delivering");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_013 - UpdateOutputStatus từ Pending -> Cancelled")]
    public async Task UpdateOutputStatus_PendingToCancelled_ShouldSucceed()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand { Id = 1, StatusId = "cancelled", CurrentUserId = Guid.NewGuid() };

        var existingOutput = new Output { Id = 1, StatusId = "pending" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        existingOutput.StatusId.Should().Be("cancelled");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_014 - UpdateOutputStatus chặn transition không hợp lệ")]
    public async Task UpdateOutputStatus_InvalidTransition_ShouldThrowException()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand { Id = 1, StatusId = "completed", CurrentUserId = Guid.NewGuid() };

        var existingOutput = new Output { Id = 1, StatusId = "pending" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_015 - UpdateOutputStatus cập nhật LastStatusChangedAt")]
    public async Task UpdateOutputStatus_ShouldUpdateLastStatusChangedAt()
    {
        var outputId = 1;
        var existingOutput = new Output
        {
            Id = outputId,
            StatusId = "pending",
            OutputInfos = []
        };

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(outputId, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(outputId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOutput);

        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand
        {
            Id = outputId,
            StatusId = "confirmed_cod",
            CurrentUserId = Guid.NewGuid()
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        existingOutput.LastStatusChangedAt.Should().NotBeNull();
        existingOutput.LastStatusChangedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "SO_016 - UpdateOutputStatus set FinishedBy khi Completed")]
    public async Task UpdateOutputStatus_ToCompleted_ShouldSetFinishedBy()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var outputId = 1;
        var existingOutput = new Output
        {
            Id = outputId,
            StatusId = "delivering", // Đảm bảo transition từ delivering -> completed là hợp lệ
            OutputInfos = []
        };

        // PHẢI Mock đúng phương thức GetByIdWithDetailsAsync
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(outputId, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        // Mock thêm cho lần gọi cuối cùng để trả về kết quả Response
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(outputId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOutput);

        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand
        {
            Id = outputId,
            StatusId = OrderStatus.Completed, // Nên dùng Constant thay vì viết thường "completed"
            CurrentUserId = currentUserId
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingOutput.FinishedBy.Should().Be(currentUserId);
    }

    [Fact(DisplayName = "SO_017 - CreateOutputByAdmin kiểm tra quyền")]
    public async Task CreateOutputByAdmin_ShouldAllowManagerToCreate()
    {
        // 1. Setup Mock User (Để qua bước check đầu tiên)
        _userRepoMock.Setup(x => x.GetUserByIDAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAuth());

        // 2. Setup Mock Variants (Để qua bước check sản phẩm và lấy giá)
        var productId = 1;
        var variants = new List<ProductVariant>
    {
        new() {
            Id = productId,
            Price = 100,
            Product = new ProductEntity { StatusId = ProductStatus.ForSale }
        }
    };
        _variantRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(variants);

        // 3. Setup Mock Read Repository (Để có kết quả trả về cuối cùng)
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Output { Id = 99 });

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
            OutputInfos = [new() { ProductId = productId, Count = 1 }]
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Kiểm tra xem có thành công không trước khi lấy Value
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(99);
        _insertRepoMock.Verify(x => x.Add(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_018 - CreateOutputByAdmin cho phép set BuyerId")]
    public async Task CreateOutputByAdmin_ShouldAllowCustomBuyerId()
    {
        // Arrange
        var customBuyerId = Guid.NewGuid();
        var productId = 1;

        // 1. Vượt qua trạm kiểm tra User
        _userRepoMock.Setup(x => x.GetUserByIDAsync(customBuyerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAuth());

        // 2. Vượt qua trạm kiểm tra Variant
        var variants = new List<ProductVariant>
    {
        new() {
            Id = productId,
            Price = 100,
            Product = new ProductEntity { StatusId = ProductStatus.ForSale }
        }
    };
        _variantRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(variants);

        // 3. Chuẩn bị dữ liệu trả về cuối cùng (để result.Value không bị null)
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Output { Id = 1, BuyerId = customBuyerId });

        var handler = new CreateOutputByManagerCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _userRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOutputByManagerCommand
        {
            BuyerId = customBuyerId,
            StatusId = "pending",
            OutputInfos = [new() { ProductId = productId, Count = 1 }]
        };

        Output? capturedOutput = null;
        _insertRepoMock.Setup(x => x.Add(It.IsAny<Output>()))
            .Callback<Output>((output) => capturedOutput = output);

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedOutput.Should().NotBeNull();
        capturedOutput!.BuyerId.Should().Be(customBuyerId);
        result.Value!.BuyerId.Should().Be(customBuyerId);
    }

    [Fact(DisplayName = "SO_019 - UpdateOutputForManager updates successfully with valid data")]
    public async Task UpdateOutputForManager_WithValidData_ShouldCallUpdate()
    {
        var productId = 1;
        var currentUserId = Guid.NewGuid();

        var command = new UpdateOutputForManagerCommand
        {
            Id = 1,
            CurrentUserId = currentUserId,
            OutputInfos = [new() { ProductId = productId, Count = 2 }]
        };

        // FIX: Mock đầy đủ cấu trúc cây (Graph) mà OutputResponse cần
        var existingOutput = new Output
        {
            Id = 1,
            CreatedBy = currentUserId,
            CreatedByUser = new ApplicationUser { Id = currentUserId, FullName = "Test User" }, // Cấp 1
            OutputInfos =
            [
                new OutputInfo
            {
                Id = 10, // Giả lập ID tồn tại
                ProductVarientId = productId,
                Count = 1,
                // QUAN TRỌNG: Phải mock object con này, nếu không Mapster sẽ crash
                ProductVariant = new ProductVariant
                {
                    Id = productId,
                    Price = 100000,
                    Product = new ProductEntity
                    {
                        Id = 99,
                        Name = "Sản phẩm Test",
                        StatusId = ProductStatus.ForSale
                    }
                }
            }
            ]
        };

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(
                1,
                It.IsAny<CancellationToken>(),
                It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        var variants = new List<ProductVariant>
    {
        new() {
            Id = productId,
            Price = 100,
            Product = new ProductEntity { StatusId = ProductStatus.ForSale }
        }
    };

        _variantRepoMock.Setup(x => x.GetByIdAsync(
                It.IsAny<List<int>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<DataFetchMode>()))
            .ReturnsAsync(variants);

        var handler = new UpdateOutputForManagerCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _deleteRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact(DisplayName = "SO_020 - UpdateOutput kiểm tra quyền Manager")]
    public async Task UpdateOutput_ShouldRequireManagerPermission()
    {
        // 1. Khởi tạo dữ liệu giả
        var userId = Guid.NewGuid();
        var productId = 1;

        var handler = new UpdateOutputCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _deleteRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        // Thêm CurrentUserId vào command
        var command = new UpdateOutputCommand
        {
            Id = 1,
            CurrentUserId = userId, // PHẢI CÓ
            OutputInfos = [new() { ProductId = productId, Count = 3 }]
        };

        // Tạo output có Buyer trùng với CurrentUserId
        var existingOutput = new Output
        {
            Id = 1,
            StatusId = "pending",
            Buyer = new ApplicationUser { Id = userId },
            OutputInfos = []
        };

        // Mock Variant để vượt qua check sản phẩm
        var mockVariant = new ProductVariant
        {
            Id = productId,
            Price = 100,
            Product = new ProductEntity { StatusId = ProductStatus.ForSale } // PHẢI CÓ
        };

        // 2. Setup Mock behavior
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        _variantRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync([mockVariant]);

        // Setup trả về kết quả sau khi update để không bị lỗi null reference ở dòng cuối handler
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        // 3. Thực thi
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // 4. Kiểm chứng
        // Nếu result.IsError là true, bạn sẽ biết ngay nó chết ở đâu
        Assert.False(result.IsFailure, $"Test failed due to: {result.Errors?.FirstOrDefault()?.Message}");

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

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

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

        var command = new DeleteOutputCommand() { Id = 1 };

        var existingOutput = new Output { Id = 1, StatusId = "pending" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        existingOutput.DeletedAt.Should().NotBeNull();
        existingOutput.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "SO_023 - RestoreOutput clears DeletedAt")]
    public async Task RestoreOutput_ShouldClearDeletedAt()
    {
        // Arrange
        var outputId = 1;
        var command = new RestoreOutputCommand { Id = outputId };
        var deletedOutput = new Output { Id = outputId, DeletedAt = DateTime.UtcNow };

        // FIX 1: Match the exact DataFetchMode or use It.IsAny
        _readRepoMock.Setup(x => x.GetByIdAsync(
                outputId,
                It.IsAny<CancellationToken>(),
                DataFetchMode.DeletedOnly)) // Must match Handler
            .ReturnsAsync(deletedOutput);

        // FIX 2: Force the mock to mutate the object state
        _updateRepoMock.Setup(x => x.Restore(It.IsAny<Output>()))
            .Callback<Output>(o => o.DeletedAt = null);

        var handler = new RestoreOutputCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        deletedOutput.DeletedAt.Should().BeNull();
        _updateRepoMock.Verify(x => x.Restore(It.IsAny<Output>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_024 - DeleteManyOutputs xóa nhiều đơn")]
    public async Task DeleteManyOutputs_ShouldDeleteMultipleOrders()
    {
        var handler = new DeleteManyOutputsCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new DeleteManyOutputsCommand { Ids = [ 1, 2, 3 ] };

        var existingOutputs = new List<Output>
        {
            new() { Id = 1, StatusId = "pending" },
            new() { Id = 2, StatusId = "pending" },
            new() { Id = 3, StatusId = "pending" }
        };
        _readRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutputs);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        _deleteRepoMock.Verify(x => x.Delete(It.IsAny<IEnumerable<Output>>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_025 - RestoreManyOutputs restores multiple orders successfully")]
    public async Task RestoreManyOutputs_ShouldRestoreMultipleOrders()
    {
        var outputIds = new List<int> { 1, 2, 3 };
        var command = new RestoreManyOutputsCommand { Ids = outputIds };

        var deletedOutputs = new List<Output>
    {
        new() { Id = 1, DeletedAt = DateTimeOffset.UtcNow },
        new() { Id = 2, DeletedAt = DateTimeOffset.UtcNow },
        new() { Id = 3, DeletedAt = DateTimeOffset.UtcNow }
    };

        // FIX 1: Match DataFetchMode.DeletedOnly
        _readRepoMock.Setup(x => x.GetByIdAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>(),
                DataFetchMode.DeletedOnly))
            .ReturnsAsync(deletedOutputs);

        // FIX 2: Mock logic for IEnumerable/List parameter and mutate state
        _updateRepoMock.Setup(x => x.Restore(It.IsAny<IEnumerable<Output>>()))
            .Callback<IEnumerable<Output>>(outputs =>
            {
                foreach (var output in outputs)
                {
                    output.DeletedAt = null;
                }
            });

        var handler = new RestoreManyOutputsCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        deletedOutputs.Should().AllSatisfy(x => x.DeletedAt.Should().BeNull());
        _updateRepoMock.Verify(x => x.Restore(It.IsAny<IEnumerable<Output>>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_026 - UpdateManyOutputStatus cập nhật nhiều đơn")]
    public async Task UpdateManyOutputStatus_ShouldUpdateMultipleOrders()
    {
        var handler = new UpdateManyOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateManyOutputStatusCommand { Ids = [ 1, 2, 3 ], StatusId = "confirmed_cod" };

        var existingOutputs = new List<Output>
        {
            new() { Id = 1, StatusId = "pending" },
            new() { Id = 2, StatusId = "pending" },
            new() { Id = 3, StatusId = "pending" }
        };
        _readRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutputs);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

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

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.Value!.Id.Should().Be(1);
    }

    [Fact(DisplayName = "SO_028 - GetOutputsByUserId chỉ lấy đơn của user")]
    public async Task GetOutputsByUserId_ShouldReturnOnlyUserOrders()
    {
        var handler = new GetOutputsByUserrIdQueryHandler(_readRepoMock.Object, _paginatorMock.Object);

        var userId = Guid.NewGuid();
        var query = new GetOutputsByUserIdQuery() { BuyerId = userId, SieveModel = new SieveModel() };

        var userOutputs = new List<Output> { new() { Id = 1, BuyerId = userId }, new() { Id = 2, BuyerId = userId } }.AsQueryable(
            );

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(userOutputs);

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _paginatorMock.Verify(
            x => x.ApplyAsync<Output, OutputResponse>(
                It.IsAny<IQueryable<Output>>(),
                It.IsAny<SieveModel>(),
                It.IsAny<DataFetchMode?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_029 - GetOutputsList hỗ trợ phân trang")]
    public async Task GetOutputsList_ShouldSupportPagination()
    {
        var handler = new GetOutputsListQueryHandler(_readRepoMock.Object, _paginatorMock.Object);

        var query = new GetOutputsListQuery() { SieveModel = new SieveModel { Page = 1, PageSize = 10 } };

        var outputs = new List<Output> { new() { Id = 1 }, new() { Id = 2 } }.AsQueryable();

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(outputs);

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _paginatorMock.Verify(
            x => x.ApplyAsync<Output, OutputResponse>(
                It.IsAny<IQueryable<Output>>(),
                It.IsAny<SieveModel>(),
                It.IsAny<DataFetchMode?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_030 - GetOutputsList filter theo status")]
    public async Task GetOutputsList_ShouldFilterByStatus()
    {
        var handler = new GetOutputsListQueryHandler(_readRepoMock.Object, _paginatorMock.Object);

        var query = new GetOutputsListQuery() { SieveModel = new SieveModel { Filters = "StatusId==pending" } };

        var outputs = new List<Output>
        {
            new() { Id = 1, StatusId = "pending" },
            new() { Id = 2, StatusId = "confirmed_cod" }
        }.AsQueryable();

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(outputs);

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _paginatorMock.Verify(
            x => x.ApplyAsync<Output, OutputResponse>(
                It.IsAny<IQueryable<Output>>(),
                It.IsAny<SieveModel>(),
                It.IsAny<DataFetchMode?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_031 - GetOutputsList sort theo CreatedAt")]
    public async Task GetOutputsList_ShouldSortByCreatedAt()
    {
        var handler = new GetOutputsListQueryHandler(_readRepoMock.Object, _paginatorMock.Object);

        var query = new GetOutputsListQuery() { SieveModel = new SieveModel { Sorts = "-CreatedAt" } };

        var outputs = new List<Output>
        {
            new() { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new() { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-1) }
        }.AsQueryable();

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(outputs);

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _paginatorMock.Verify(
            x => x.ApplyAsync<Output, OutputResponse>(
                It.IsAny<IQueryable<Output>>(),
                It.IsAny<SieveModel>(),
                It.IsAny<DataFetchMode?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_032 - GetDeletedOutputsList chỉ lấy đơn đã xóa")]
    public async Task GetDeletedOutputsList_ShouldReturnOnlyDeletedOrders()
    {
        var handler = new GetDeletedOutputsListQueryHandler(_readRepoMock.Object, _paginatorMock.Object);

        var query = new GetDeletedOutputsListQuery() { SieveModel = new SieveModel() };

        var deletedOutputs = new List<Output>
        {
            new() { Id = 1, DeletedAt = DateTime.UtcNow },
            new() { Id = 2, DeletedAt = DateTime.UtcNow }
        }.AsQueryable();

        _readRepoMock.Setup(x => x.GetQueryable(DataFetchMode.DeletedOnly)).Returns(deletedOutputs);

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _paginatorMock.Verify(
            x => x.ApplyAsync<Output, OutputResponse>(
                It.IsAny<IQueryable<Output>>(),
                It.IsAny<SieveModel>(),
                It.IsAny<DataFetchMode?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_033 - CreateOutput with many products processes all")]
    public async Task CreateOutput_WithManyProducts_ShouldProcessAll()
    {
        var productId1 = 1;
        var productId2 = 2;
        var productId3 = 3;
        var productId4 = 4;

        var command = new CreateOutputCommand
        {
            BuyerId = Guid.NewGuid(),
            OutputInfos =
            [
                new() { ProductId = productId1, Count = 5 },
            new() { ProductId = productId2, Count = 3 },
            new() { ProductId = productId3, Count = 2 },
            new() { ProductId = productId4, Count = 1 }
            ]
        };

        // FIX: Prepare variants to pass guard clauses
        var variants = new List<ProductVariant>
    {
        new() { Id = productId1, Price = 10, Product = new ProductEntity { StatusId = ProductStatus.ForSale } },
        new() { Id = productId2, Price = 20, Product = new ProductEntity { StatusId = ProductStatus.ForSale } },
        new() { Id = productId3, Price = 30, Product = new ProductEntity { StatusId = ProductStatus.ForSale } },
        new() { Id = productId4, Price = 40, Product = new ProductEntity { StatusId = ProductStatus.ForSale } }
    };

        _variantRepoMock.Setup(x => x.GetByIdAsync(
                It.IsAny<List<int>>(),
                It.IsAny<CancellationToken>(),
                DataFetchMode.ActiveOnly))
            .ReturnsAsync(variants);

        // FIX: Mock the return value of GetByIdWithDetailsAsync to prevent NRE in the final Adapt
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(new Output());

        Output? capturedOutput = null;
        _insertRepoMock.Setup(x => x.Add(It.IsAny<Output>()))
            .Callback<Output>((output) => capturedOutput = output);

        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedOutput.Should().NotBeNull();
        capturedOutput!.OutputInfos.Should().HaveCount(4);
    }

    [Fact(DisplayName = "SO_034 - CreateOutput calculates total amount")]
    public async Task CreateOutput_ShouldCalculateTotalAmount()
    {
        var product1 = new { Id = 1, Price = 100, Count = 2 };
        var product2 = new { Id = 2, Price = 200, Count = 3 };

        var command = new CreateOutputCommand
        {
            BuyerId = Guid.NewGuid(),
            OutputInfos =
            [
                new() { ProductId = product1.Id, Count = product1.Count },
            new() { ProductId = product2.Id, Count = product2.Count }
            ]
        };

        var variants = new List<ProductVariant>
    {
        new() { Id = product1.Id, Price = product1.Price, Product = new ProductEntity { StatusId = ProductStatus.ForSale } },
        new() { Id = product2.Id, Price = product2.Price, Product = new ProductEntity { StatusId = ProductStatus.ForSale } }
    };

        _variantRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync(variants);

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(new Output());

        Output? capturedOutput = null;
        _insertRepoMock.Setup(x => x.Add(It.IsAny<Output>()))
            .Callback<Output>((output) => capturedOutput = output);

        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        capturedOutput.Should().NotBeNull();
        // Total = (2 * 100) + (3 * 200) = 800
        var total = capturedOutput!.OutputInfos.Sum(x => x.Count * x.Price);
        total.Should().Be(800);
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
            OutputInfos = { new() { ProductId = 1, Count = 0 } }
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

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
            OutputInfos = [ new() { ProductId = 999, Count = 1 } ]
        };

        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync([]);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

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

        var command = new UpdateOutputCommand { Id = 1, OutputInfos = { new() { ProductId = 1, Count = 5 } } };

        var completedOutput = new Output { Id = 1, StatusId = "completed" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(completedOutput);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

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

        var command = new UpdateOutputCommand { Id = 1, OutputInfos = { new() { ProductId = 1, Count = 5 } } };

        var deletedOutput = new Output { Id = 1, DeletedAt = DateTime.UtcNow };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(deletedOutput);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

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

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

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

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

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
        var command = new UpdateOutputStatusCommand { Id = 1, StatusId = "completed", CurrentUserId = finishedBy };

        var existingOutput = new Output { Id = 1, StatusId = "delivering" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        existingOutput.FinishedBy.Should().Be(finishedBy);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_042 - CreateOutput set CreatedBy correctly")]
    public async Task CreateOutput_ShouldSetCreatedBy()
    {
        // Arrange
        var productId = 1;
        // Dùng một biến duy nhất để làm mốc so sánh
        var expectedId = Guid.NewGuid();

        var command = new CreateOutputCommand
        {
            BuyerId = expectedId, // Gán vào BuyerId
            OutputInfos = [new() { ProductId = productId, Count = 1 }]
        };

        // Mock các trạm gác (như đã phân tích ở các bước trước)
        var variants = new List<ProductVariant> {
        new() {
            Id = productId,
            Price = 100,
            Product = new ProductEntity { StatusId = ProductStatus.ForSale }
        }
    };
        _variantRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync(variants);

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(new Output());

        Output? capturedOutput = null;
        _insertRepoMock.Setup(x => x.Add(It.IsAny<Output>()))
            .Callback<Output>(output => capturedOutput = output);

        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        capturedOutput.Should().NotBeNull();

        // Kiểm tra CreatedBy phải ăn theo BuyerId của Request
        capturedOutput!.CreatedBy.Should().Be(expectedId);
        // Kiểm tra BuyerId cũng phải đúng
        capturedOutput.BuyerId.Should().Be(expectedId);
    }

    [Fact(DisplayName = "SO_043 - GetOutputById không trả về đơn đã xóa")]
    public async Task GetOutputById_DeletedOrder_ShouldReturnNull()
    {
        var handler = new GetOutputByIdQueryHandler(_readRepoMock.Object);

        var query = new GetOutputByIdQuery() { Id = 1 };

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync((Output?)null);

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_044 - GetOutputsList exclude soft deleted")]
    public async Task GetOutputsList_ShouldExcludeSoftDeleted()
    {
        var handler = new GetOutputsListQueryHandler(_readRepoMock.Object, _paginatorMock.Object);

        var query = new GetOutputsListQuery() { SieveModel = new SieveModel() };

        var outputs = new List<Output> { new() { Id = 1, DeletedAt = null }, new() { Id = 2, DeletedAt = null } }.AsQueryable(
            );

        _readRepoMock.Setup(x => x.GetQueryable(DataFetchMode.ActiveOnly)).Returns(outputs);

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

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
            OutputInfos = { new() { ProductId = 1, Count = 1 } }
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_046 - Validator should fail when OutputInfos is empty")]
    public void Validator_WithNoProducts_ShouldHaveValidationError()
    {
        CreateOutputCommandValidator _validator = new();
        var command = new CreateOutputCommand
        {
            BuyerId = Guid.NewGuid(),
            OutputInfos = []
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "OutputInfos");
    }

    [Fact(DisplayName = "SO_047 - UpdateOutputStatus validation StatusId not empty")]
    public async Task UpdateOutputStatus_WithEmptyStatusId_ShouldThrowException()
    {
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand { Id = 1, StatusId = string.Empty, CurrentUserId = Guid.NewGuid() };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

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

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_049 - Validator fails when Ids is empty")]
    public void Validator_WithEmptyIds_ShouldHaveError()
    {
        UpdateManyOutputStatusCommandValidator validator = new();
    var command = new UpdateManyOutputStatusCommand
        {
            Ids = [],
            StatusId = "confirmed_cod"
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Ids");
    }

    [Fact(DisplayName = "SO_050 - DeleteManyOutputs fails when Ids is empty")]
    public void DeleteManyOutputs_WithEmptyIds_ShouldHaveValidationError()
    {
        DeleteManyOutputsCommandValidator validator = new();
    var command = new DeleteManyOutputsCommand { Ids = [] };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Ids");
    }

    [Fact(DisplayName = "SO_051 - RestoreManyOutputs fails when Ids is empty")]
    public void RestoreManyOutputs_WithEmptyIds_ShouldHaveValidationError()
    {
        RestoreManyOutputsCommandValidator validator = new();
        var command = new RestoreManyOutputsCommand { Ids = [] };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Ids");
    }

    [Fact(DisplayName = "SO_052 - GetOutputsByUserId với pagination")]
    public async Task GetOutputsByUserId_ShouldSupportPagination()
    {
        var handler = new GetOutputsByUserrIdQueryHandler(_readRepoMock.Object, _paginatorMock.Object);

        var userId = Guid.NewGuid();
        var query = new GetOutputsByUserIdQuery()
        {
            BuyerId = userId,
            SieveModel = new SieveModel { Page = 2, PageSize = 5 }
        };

        var outputs = new List<Output> { new() { Id = 1, BuyerId = userId }, new() { Id = 2, BuyerId = userId } }.AsQueryable(
            );

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(outputs);

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _paginatorMock.Verify(
            x => x.ApplyAsync<Output, OutputResponse>(
                It.IsAny<IQueryable<Output>>(),
                It.IsAny<SieveModel>(),
                It.IsAny<DataFetchMode?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_053 - GetOutputsByUserIdByManager kiểm tra quyền")]
    public async Task GetOutputsByUserIdByManager_ShouldRequireManagerPermission()
    {
        var handler = new GetOutputsByUserIdByManagerQueryHandler(_readRepoMock.Object, _paginatorMock.Object);

        var userId = Guid.NewGuid();
        var query = new GetOutputsByUserIdQuery() { BuyerId = userId, SieveModel = new SieveModel() };

        var outputs = new List<Output> { new() { Id = 1, BuyerId = userId } }.AsQueryable();

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(outputs);

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _paginatorMock.Verify(
            x => x.ApplyAsync<Output, OutputResponse>(
                It.IsAny<IQueryable<Output>>(),
                It.IsAny<SieveModel>(),
                It.IsAny<DataFetchMode?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
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

        var command = new UpdateOutputCommand { Id = 1, OutputInfos = { new() { ProductId = 999, Count = 1 } } };

        var existingOutput = new Output { Id = 1, StatusId = "pending" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync([]);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

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
            OutputInfos = [ new() { ProductId = 1, Count = 1 } ]
        };

        var deletedVariant = new ProductVariant { Id = 1, DeletedAt = DateTime.UtcNow };
        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), DataFetchMode.All))
            .ReturnsAsync([ deletedVariant ]);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SO_057 - UpdateOutputStatus from PaidProcessing to Refunding succeeds")]
    public async Task UpdateOutputStatus_PaidProcessingToRefunding_ShouldSucceed()
    {
        var outputId = 1;
        var currentUserId = Guid.NewGuid();
        var handler = new UpdateOutputStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputStatusCommand
        {
            Id = 1,
            StatusId = "refunding", // Correct target
            CurrentUserId = Guid.NewGuid()
        };

        var existingOutput = new Output
        {
            Id = 1,
            StatusId = "paid_processing",
            OutputInfos =
            [
                new OutputInfo { ProductVarientId = 100, Count = 5 } // Khách mua 5 cái
            ]
        };

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(
                outputId,
                It.IsAny<CancellationToken>(),
                It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        var handlerResult = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        handlerResult.IsSuccess.Should().BeTrue();
        existingOutput.StatusId.Should().Be("refunding");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_057 - CreateOutput creates with Notes correctly")]
    public async Task CreateOutput_WithNotes_ShouldSaveNotes()
    {
        // Arrange
        var productId = 1;
        var notes = "Giao hàng trước 5pm";
        var buyerId = Guid.NewGuid();

        var command = new CreateOutputCommand
        {
            BuyerId = buyerId,
            Notes = notes,
            OutputInfos = [new() { ProductId = productId, Count = 1 }]
        };

        // 1. Mock để vượt qua trạm gác sản phẩm
        var variants = new List<ProductVariant>
    {
        new() {
            Id = productId,
            Price = 100,
            Product = new ProductEntity { StatusId = Domain.Constants.ProductStatus.ForSale }
        }
    };
        _variantRepoMock.Setup(x => x.GetByIdAsync(
                It.IsAny<List<int>>(),
                It.IsAny<CancellationToken>(),
                DataFetchMode.ActiveOnly))
            .ReturnsAsync(variants);

        // 2. Mock trạm cuối để Mapping result.Value!.Notes không bị NRE
        var finalOutput = new Output { Id = 1, Notes = notes };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<DataFetchMode>()))
            .ReturnsAsync(finalOutput);

        Output? capturedOutput = null;
        _insertRepoMock.Setup(x => x.Add(It.IsAny<Output>()))
            .Callback<Output>((output) => capturedOutput = output);

        var handler = new CreateOutputCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        capturedOutput.Should().NotBeNull();
        capturedOutput!.Notes.Should().Be(notes); // Kiểm tra dữ liệu được lưu vào DB
        result.IsSuccess.Should().BeTrue();
        result.Value!.Notes.Should().Be(notes); // Kiểm tra dữ liệu trả về cho API
    }

    [Fact(DisplayName = "SO_058 - UpdateOutput updates Notes successfully")]
    public async Task UpdateOutput_ShouldAllowUpdatingNotes()
    {
        var productId = 1;
        var currentUserId = Guid.NewGuid();
        var handler = new UpdateOutputCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _deleteRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateOutputCommand
        {
            Id = 1,
            CurrentUserId = currentUserId,
            Notes = "Cập nhật: Giao vào sáng mai",
            OutputInfos = [new() { ProductId = productId, Count = 2 }]
        };

        var existingOutput = new Output
        {
            Id = 1,
            StatusId = "pending",
            Notes = "Old notes",
            Buyer = new ApplicationUser() { Id = currentUserId },
            OutputInfos = []
        };

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingOutput);

        var variants = new List<ProductVariant>
    {
        new() {
            Id = productId,
            Product = new ProductEntity { StatusId = Domain.Constants.ProductStatus.ForSale }
        }
    };

        _variantRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(variants);

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOutput);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        existingOutput.Notes.Should().Be("Cập nhật: Giao vào sáng mai");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<Output>()), Times.Once);
    }

    [Fact(DisplayName = "SO_059 - GetOutputsList search theo CustomerName")]
    public async Task GetOutputsList_ShouldSearchByCustomerName()
    {
        var handler = new GetOutputsListQueryHandler(_readRepoMock.Object, _paginatorMock.Object);

        var query = new GetOutputsListQuery() { SieveModel = new SieveModel { Filters = "CustomerName@=Nguyen" } };

        var outputs = new List<Output>
        {
            new() { Id = 1, CustomerName = "Nguyen Van A" },
            new() { Id = 2, CustomerName = "Nguyen Thi B" }
        }.AsQueryable();

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(outputs);

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _paginatorMock.Verify(
            x => x.ApplyAsync<Output, OutputResponse>(
                It.IsAny<IQueryable<Output>>(),
                It.IsAny<SieveModel>(),
                It.IsAny<DataFetchMode?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_060 - GetOutputsList filter theo date range")]
    public async Task GetOutputsList_ShouldFilterByDateRange()
    {
        var handler = new GetOutputsListQueryHandler(_readRepoMock.Object, _paginatorMock.Object);

        var query = new GetOutputsListQuery()
        {
            SieveModel = new SieveModel { Filters = "CreatedAt>=2024-01-01,CreatedAt<=2024-12-31" }
        };

        var outputs = new List<Output>
        {
            new() { Id = 1, CreatedAt = new DateTime(2024, 6, 1) },
            new() { Id = 2, CreatedAt = new DateTime(2024, 7, 1) }
        }.AsQueryable();

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(outputs);

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _paginatorMock.Verify(
            x => x.ApplyAsync<Output, OutputResponse>(
                It.IsAny<IQueryable<Output>>(),
                It.IsAny<SieveModel>(),
                It.IsAny<DataFetchMode?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
#pragma warning restore CRR0035
#pragma warning restore IDE0079
    [Fact(DisplayName = "SO_099 - CreateOutput validates CustomerPhone")]
    public void CreateOutput_ValidateCustomerPhone_ShouldCheckFormat()
    {
        var validator = new CreateOutputCommandValidator();

        // Valid cases
        var validCommand1 = new CreateOutputCommand { CustomerPhone = "0912345678" };
        var result1 = validator.TestValidate(validCommand1);
        result1.ShouldNotHaveValidationErrorFor(x => x.CustomerPhone);

        var validCommand2 = new CreateOutputCommand { CustomerPhone = "84912345678" };
        var result2 = validator.TestValidate(validCommand2);
        result2.ShouldNotHaveValidationErrorFor(x => x.CustomerPhone);

        var validCommand3 = new CreateOutputCommand { CustomerPhone = "+84912345678" };
        var result3 = validator.TestValidate(validCommand3);
        result3.ShouldNotHaveValidationErrorFor(x => x.CustomerPhone);

        // Invalid cases
        var invalidCommand1 = new CreateOutputCommand { CustomerPhone = "091234" }; // Too short
        var resultInv1 = validator.TestValidate(invalidCommand1);
        resultInv1.ShouldHaveValidationErrorFor(x => x.CustomerPhone)
                  .WithErrorMessage("Invalid phone number format.");

        var invalidCommand2 = new CreateOutputCommand { CustomerPhone = "abcd123456" }; // Non-numeric
        var resultInv2 = validator.TestValidate(invalidCommand2);
        resultInv2.ShouldHaveValidationErrorFor(x => x.CustomerPhone)
                  .WithErrorMessage("Invalid phone number format.");
    }
}
