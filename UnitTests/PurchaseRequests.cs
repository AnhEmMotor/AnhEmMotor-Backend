using Application.ApiContracts.PurchaseRequest.Requests;
using Application.Features.PurchaseRequests.Commands.ApproveRejectPurchaseRequest;
using Application.Features.PurchaseRequests.Commands.CreatePurchaseRequest;
using Application.Features.PurchaseRequests.Commands.DeletePurchaseRequest;
using Application.Features.PurchaseRequests.Commands.SendPurchaseRequest;
using Application.Features.PurchaseRequests.Commands.UpdatePurchaseRequest;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.PurchaseRequest;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Moq;
using PurchaseRequestEntity = Domain.Entities.PurchaseRequest;

namespace UnitTests;

public class PurchaseRequests
{
    private readonly Mock<IPurchaseRequestInsertRepository> _insertRepoMock;
    private readonly Mock<IPurchaseRequestReadRepository> _readRepoMock;
    private readonly Mock<IPurchaseRequestUpdateRepository> _updateRepoMock;
    private readonly Mock<IPurchaseRequestDeleteRepository> _deleteRepoMock;
    private readonly Mock<IProductVariantReadRepository> _variantRepoMock;
    private readonly Mock<IPermissionReadRepository> _permissionRepoMock;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public PurchaseRequests()
    {
        _insertRepoMock = new Mock<IPurchaseRequestInsertRepository>();
        _readRepoMock = new Mock<IPurchaseRequestReadRepository>();
        _updateRepoMock = new Mock<IPurchaseRequestUpdateRepository>();
        _deleteRepoMock = new Mock<IPurchaseRequestDeleteRepository>();
        _variantRepoMock = new Mock<IProductVariantReadRepository>();
        _permissionRepoMock = new Mock<IPermissionReadRepository>();
        _currentUserContextMock = new Mock<ICurrentUserContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    #pragma warning disable IDE0079 
    #pragma warning disable CRR0035
    [Fact(DisplayName = "PR_001 - Tạo PR thành công với dữ liệu hợp lệ (Sản phẩm không có màu)")]
    public async Task PR_001_CreatePurchaseRequest_Success_NoColor()
    {
        var handler = new CreatePurchaseRequestCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _variantRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreatePurchaseRequestCommand
        {
            Note = "Test Note",
            Items = [new CreatePurchaseRequestItemRequest { ProductVariantId = 1, Quantity = 10 }]
        };
        var mockVariants = new List<ProductVariant>
        {
            new() { Id = 1, VariantName = "Variant 1", ProductVariantColors = [] }
        };
        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(mockVariants);
        var currentUserId = Guid.NewGuid();
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        var createdPR = new PurchaseRequestEntity
        {
            Id = 10,
            Note = "Test Note",
            Status = "draft",
            CreatedBy = currentUserId
        };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPR);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        string.Compare(result.Value!.Note, "Test Note").Should().Be(0);
        string.Compare(result.Value.Status, "draft").Should().Be(0);
    }

    [Fact(DisplayName = "PR_002 - Tạo PR thành công với dữ liệu hợp lệ (Sản phẩm có màu trùng khớp)")]
    public async Task PR_002_CreatePurchaseRequest_Success_WithColor()
    {
        var handler = new CreatePurchaseRequestCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _variantRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreatePurchaseRequestCommand
        {
            Note = "Test Note With Color",
            Items =
                [new CreatePurchaseRequestItemRequest { ProductVariantId = 1, ProductVariantColorId = 2, Quantity = 5 }]
        };
        var mockVariants = new List<ProductVariant>
        {
            new() { Id = 1, VariantName = "Variant 1", ProductVariantColors = [new ProductVariantColor { Id = 2 }] }
        };
        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(mockVariants);
        var currentUserId = Guid.NewGuid();
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        var createdPR = new PurchaseRequestEntity
        {
            Id = 11,
            Note = "Test Note With Color",
            Status = "draft",
            CreatedBy = currentUserId
        };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPR);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        string.Compare(result.Value!.Note, "Test Note With Color").Should().Be(0);
    }

    [Fact(DisplayName = "PR_003 - Tạo PR thất bại do danh sách sản phẩm trống")]
    public async Task PR_003_CreatePurchaseRequest_Failed_EmptyItems()
    {
        var handler = new CreatePurchaseRequestCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _variantRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreatePurchaseRequestCommand { Note = "Test Empty Items", Items = [] };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Fact(DisplayName = "PR_004 - Tạo PR thất bại do ProductVariantId trong Items bị trống")]
    public async Task PR_004_CreatePurchaseRequest_Failed_NullVariantId()
    {
        var handler = new CreatePurchaseRequestCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _variantRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreatePurchaseRequestCommand
        {
            Note = "Test Null Variant Id",
            Items = [new CreatePurchaseRequestItemRequest { ProductVariantId = null, Quantity = 10 }]
        };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Fact(DisplayName = "PR_005 - Tạo PR thất bại do biến thể sản phẩm không tồn tại")]
    public async Task PR_005_CreatePurchaseRequest_Failed_VariantNotFound()
    {
        var handler = new CreatePurchaseRequestCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _variantRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreatePurchaseRequestCommand
        {
            Note = "Test Variant Not Found",
            Items = [new CreatePurchaseRequestItemRequest { ProductVariantId = 99, Quantity = 10 }]
        };
        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync([]);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "NotFound").Should().Be(0);
    }

    [Fact(DisplayName = "PR_006 - Tạo PR thất bại do sản phẩm yêu cầu chọn màu sắc nhưng để trống")]
    public async Task PR_006_CreatePurchaseRequest_Failed_MissingColor()
    {
        var handler = new CreatePurchaseRequestCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _variantRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreatePurchaseRequestCommand
        {
            Note = "Test Missing Color",
            Items =
                [new CreatePurchaseRequestItemRequest
                {
                    ProductVariantId = 1,
                    ProductVariantColorId = null,
                    Quantity = 10
                }]
        };
        var mockVariants = new List<ProductVariant>
        {
            new() { Id = 1, VariantName = "Variant 1", ProductVariantColors = [new ProductVariantColor { Id = 2 }] }
        };
        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(mockVariants);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Fact(DisplayName = "PR_007 - Tạo PR thất bại do màu sắc đã chọn không thuộc về sản phẩm")]
    public async Task PR_007_CreatePurchaseRequest_Failed_WrongColor()
    {
        var handler = new CreatePurchaseRequestCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _variantRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreatePurchaseRequestCommand
        {
            Note = "Test Wrong Color",
            Items =
                [new CreatePurchaseRequestItemRequest
                {
                    ProductVariantId = 1,
                    ProductVariantColorId = 99,
                    Quantity = 10
                }]
        };
        var mockVariants = new List<ProductVariant>
        {
            new() { Id = 1, VariantName = "Variant 1", ProductVariantColors = [new ProductVariantColor { Id = 2 }] }
        };
        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(mockVariants);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Fact(DisplayName = "PR_008 - Tạo PR thất bại do sản phẩm không hỗ trợ màu sắc nhưng vẫn gửi kèm ColorId")]
    public async Task PR_008_CreatePurchaseRequest_Failed_ColorNotSupported()
    {
        var handler = new CreatePurchaseRequestCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _variantRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreatePurchaseRequestCommand
        {
            Note = "Test Color Not Supported",
            Items =
                [new CreatePurchaseRequestItemRequest { ProductVariantId = 1, ProductVariantColorId = 2, Quantity = 10 }]
        };
        var mockVariants = new List<ProductVariant>
        {
            new() { Id = 1, VariantName = "Variant 1", ProductVariantColors = [] }
        };
        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(mockVariants);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Theory(DisplayName = "PR_009 - Tạo PR thất bại do số lượng sản phẩm nhỏ hơn hoặc bằng 0")]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task PR_009_CreatePurchaseRequest_Failed_InvalidQuantity(int quantity)
    {
        var handler = new CreatePurchaseRequestCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _variantRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreatePurchaseRequestCommand
        {
            Note = "Test Invalid Quantity",
            Items = [new CreatePurchaseRequestItemRequest { ProductVariantId = 1, Quantity = quantity }]
        };
        var mockVariants = new List<ProductVariant>
        {
            new() { Id = 1, VariantName = "Variant 1", ProductVariantColors = [] }
        };
        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(mockVariants);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Fact(DisplayName = "PR_010 - Cập nhật PR thành công ở trạng thái draft")]
    public async Task PR_010_UpdatePurchaseRequest_Success_Draft()
    {
        var handler = new UpdatePurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _deleteRepoMock.Object,
            _variantRepoMock.Object,
            _permissionRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdatePurchaseRequestCommand
        {
            Id = 1,
            Note = "Updated Note",
            Items = [new UpdatePurchaseRequestItemRequest { ProductVariantId = 1, Quantity = 20 }]
        };
        var existingPR = new PurchaseRequestEntity
        {
            Id = 1,
            Status = "draft",
            Note = "Old Note",
            PurchaseRequestItems = []
        };
        var mockVariants = new List<ProductVariant>
        {
            new() { Id = 1, VariantName = "Variant 1", ProductVariantColors = [] }
        };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingPR);
        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(mockVariants);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        _updateRepoMock.Verify(x => x.Update(It.IsAny<PurchaseRequestEntity>()), Times.Once);
    }

    [Fact(DisplayName = "PR_011 - Cập nhật PR thất bại do PR không tồn tại")]
    public async Task PR_011_UpdatePurchaseRequest_Failed_NotFound()
    {
        var handler = new UpdatePurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _deleteRepoMock.Object,
            _variantRepoMock.Object,
            _permissionRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdatePurchaseRequestCommand { Id = 99, Note = "Note", Items = [] };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseRequestEntity?)null);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "NotFound").Should().Be(0);
    }

    [Fact(DisplayName = "PR_012 - Cập nhật PR ở trạng thái Sent thành công bởi người có quyền duyệt")]
    public async Task PR_012_UpdatePurchaseRequest_Success_Sent_WithPermission()
    {
        var handler = new UpdatePurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _deleteRepoMock.Object,
            _variantRepoMock.Object,
            _permissionRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdatePurchaseRequestCommand
        {
            Id = 1,
            Note = "Updated Note Sent",
            Items = [new UpdatePurchaseRequestItemRequest { ProductVariantId = 1, Quantity = 20 }]
        };
        var existingPR = new PurchaseRequestEntity
        {
            Id = 1,
            Status = "sent",
            Note = "Old Note",
            PurchaseRequestItems = []
        };
        var mockVariants = new List<ProductVariant>
        {
            new() { Id = 1, VariantName = "Variant 1", ProductVariantColors = [] }
        };
        var currentUserId = Guid.NewGuid();
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingPR);
        _permissionRepoMock.Setup(
            x => x.CheckUserPermissionsAsync(
                currentUserId,
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(mockVariants);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "PR_013 - Cập nhật PR ở trạng thái Sent thất bại bởi người không có quyền duyệt")]
    public async Task PR_013_UpdatePurchaseRequest_Failed_Sent_NoPermission()
    {
        var handler = new UpdatePurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _deleteRepoMock.Object,
            _variantRepoMock.Object,
            _permissionRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdatePurchaseRequestCommand { Id = 1, Note = "Updated Note Sent Fail", Items = [] };
        var existingPR = new PurchaseRequestEntity { Id = 1, Status = "sent", Note = "Old Note" };
        var currentUserId = Guid.NewGuid();
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingPR);
        _permissionRepoMock.Setup(
            x => x.CheckUserPermissionsAsync(
                currentUserId,
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Theory(DisplayName = "PR_014 - Cập nhật PR thất bại do PR đã được Phê duyệt hoặc Từ chối")]
    [InlineData("approve")]
    [InlineData("reject")]
    public async Task PR_014_UpdatePurchaseRequest_Failed_AlreadyFinalized(string status)
    {
        var handler = new UpdatePurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _deleteRepoMock.Object,
            _variantRepoMock.Object,
            _permissionRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdatePurchaseRequestCommand { Id = 1, Note = "Updated Note Finalized", Items = [] };
        var existingPR = new PurchaseRequestEntity { Id = 1, Status = status, Note = "Old Note" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingPR);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Fact(DisplayName = "PR_015 - Xóa PR ở trạng thái draft thành công")]
    public async Task PR_015_DeletePurchaseRequest_Success_Draft()
    {
        var handler = new DeletePurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _permissionRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new DeletePurchaseRequestCommand(1);
        var existingPR = new PurchaseRequestEntity { Id = 1, Status = "draft" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingPR);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        _deleteRepoMock.Verify(x => x.Delete(existingPR), Times.Once);
    }

    [Fact(DisplayName = "PR_016 - Xóa PR thất bại do PR không tồn tại")]
    public async Task PR_016_DeletePurchaseRequest_Failed_NotFound()
    {
        var handler = new DeletePurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _permissionRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new DeletePurchaseRequestCommand(99);
        _readRepoMock.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseRequestEntity?)null);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "NotFound").Should().Be(0);
    }

    [Fact(DisplayName = "PR_017 - Xóa PR ở trạng thái khác draft thành công bởi người có quyền duyệt")]
    public async Task PR_017_DeletePurchaseRequest_Success_NonDraft_WithPermission()
    {
        var handler = new DeletePurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _permissionRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new DeletePurchaseRequestCommand(1);
        var existingPR = new PurchaseRequestEntity { Id = 1, Status = "sent" };
        var currentUserId = Guid.NewGuid();
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingPR);
        _permissionRepoMock.Setup(
            x => x.CheckUserPermissionsAsync(
                currentUserId,
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        _deleteRepoMock.Verify(x => x.Delete(existingPR), Times.Once);
    }

    [Fact(DisplayName = "PR_018 - Xóa PR ở trạng thái khác draft thất bại bởi người không có quyền duyệt")]
    public async Task PR_018_DeletePurchaseRequest_Failed_NonDraft_NoPermission()
    {
        var handler = new DeletePurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _permissionRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new DeletePurchaseRequestCommand(1);
        var existingPR = new PurchaseRequestEntity { Id = 1, Status = "sent" };
        var currentUserId = Guid.NewGuid();
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingPR);
        _permissionRepoMock.Setup(
            x => x.CheckUserPermissionsAsync(
                currentUserId,
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Fact(DisplayName = "PR_019 - Gửi PR (Send PR) thành công")]
    public async Task PR_019_SendPurchaseRequest_Success()
    {
        var handler = new SendPurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new SendPurchaseRequestCommand(1);
        var existingPR = new PurchaseRequestEntity { Id = 1, Status = "draft" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingPR);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        string.Compare(existingPR.Status, "sent").Should().Be(0);
        _updateRepoMock.Verify(x => x.Update(existingPR), Times.Once);
    }

    [Fact(DisplayName = "PR_020 - Gửi PR thất bại do PR không ở trạng thái draft")]
    public async Task PR_020_SendPurchaseRequest_Failed_NotDraft()
    {
        var handler = new SendPurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new SendPurchaseRequestCommand(1);
        var existingPR = new PurchaseRequestEntity { Id = 1, Status = "approve" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingPR);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Fact(DisplayName = "PR_021 - Phê duyệt PR thành công")]
    public async Task PR_021_ApprovePurchaseRequest_Success()
    {
        var handler = new ApproveRejectPurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object,
            _currentUserContextMock.Object);
        var command = new ApproveRejectPurchaseRequestCommand(1, "approve");
        var existingPR = new PurchaseRequestEntity { Id = 1, Status = "sent" };
        var currentUserId = Guid.NewGuid();
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingPR);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        string.Compare(existingPR.Status, "approve").Should().Be(0);
        existingPR.ApprovedBy.Should().Be(currentUserId);
        _updateRepoMock.Verify(x => x.Update(existingPR), Times.Once);
    }

    [Fact(DisplayName = "PR_022 - Từ chối PR thành công")]
    public async Task PR_022_RejectPurchaseRequest_Success()
    {
        var handler = new ApproveRejectPurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object,
            _currentUserContextMock.Object);
        var command = new ApproveRejectPurchaseRequestCommand(1, "reject");
        var existingPR = new PurchaseRequestEntity { Id = 1, Status = "sent" };
        var currentUserId = Guid.NewGuid();
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingPR);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        string.Compare(existingPR.Status, "reject").Should().Be(0);
        existingPR.RejectedBy.Should().Be(currentUserId);
        _updateRepoMock.Verify(x => x.Update(existingPR), Times.Once);
    }

    [Fact(DisplayName = "PR_023 - Phê duyệt/từ chối PR thất bại do trạng thái mới không hợp lệ")]
    public async Task PR_023_ApproveRejectPurchaseRequest_Failed_InvalidNewStatus()
    {
        var handler = new ApproveRejectPurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object,
            _currentUserContextMock.Object);
        var command = new ApproveRejectPurchaseRequestCommand(1, "draft");
        var existingPR = new PurchaseRequestEntity { Id = 1, Status = "sent" };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingPR);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Theory(DisplayName = "PR_024 - Phê duyệt/từ chối PR thất bại do PR không ở trạng thái Sent")]
    [InlineData("draft")]
    [InlineData("approve")]
    public async Task PR_024_ApproveRejectPurchaseRequest_Failed_NotSent(string currentStatus)
    {
        var handler = new ApproveRejectPurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object,
            _currentUserContextMock.Object);
        var command = new ApproveRejectPurchaseRequestCommand(1, "approve");
        var existingPR = new PurchaseRequestEntity { Id = 1, Status = currentStatus };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingPR);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Fact(DisplayName = "PR_043 - Gửi PR thành công và lưu người gửi")]
    public async Task PR_043_SendPurchaseRequest_SaveSentBy_Success()
    {
        var handler = new SendPurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object,
            _currentUserContextMock.Object);
        var command = new SendPurchaseRequestCommand(1);
        var existingPR = new PurchaseRequestEntity { Id = 1, Status = "draft" };
        var currentUserId = Guid.NewGuid();
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingPR);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        string.Compare(existingPR.Status, "sent").Should().Be(0);
        existingPR.SentBy.Should().Be(currentUserId);
        _updateRepoMock.Verify(x => x.Update(existingPR), Times.Once);
    }

    [Fact(DisplayName = "PR_044 - Phê duyệt PR thành công và lưu người duyệt")]
    public async Task PR_044_ApprovePurchaseRequest_SaveApprovedBy_Success()
    {
        var handler = new ApproveRejectPurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object,
            _currentUserContextMock.Object);
        var command = new ApproveRejectPurchaseRequestCommand(1, "approve");
        var existingPR = new PurchaseRequestEntity { Id = 1, Status = "sent" };
        var currentUserId = Guid.NewGuid();
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingPR);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        string.Compare(existingPR.Status, "approve").Should().Be(0);
        existingPR.ApprovedBy.Should().Be(currentUserId);
        existingPR.RejectedBy.Should().BeNull();
        _updateRepoMock.Verify(x => x.Update(existingPR), Times.Once);
    }

    [Fact(DisplayName = "PR_045 - Từ chối PR thành công và lưu người từ chối")]
    public async Task PR_045_RejectPurchaseRequest_SaveRejectedBy_Success()
    {
        var handler = new ApproveRejectPurchaseRequestCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object,
            _currentUserContextMock.Object);
        var command = new ApproveRejectPurchaseRequestCommand(1, "reject");
        var existingPR = new PurchaseRequestEntity { Id = 1, Status = "sent" };
        var currentUserId = Guid.NewGuid();
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingPR);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        string.Compare(existingPR.Status, "reject").Should().Be(0);
        existingPR.RejectedBy.Should().Be(currentUserId);
        existingPR.ApprovedBy.Should().BeNull();
        _updateRepoMock.Verify(x => x.Update(existingPR), Times.Once);
    }
    #pragma warning restore CRR0035
    #pragma warning restore IDE0079
}
