using Application.ApiContracts.Quotation.Requests;
using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using Application.Features.Quotations.Commands.ApproveQuotation;
using Application.Features.Quotations.Commands.CreateQuotation;
using Application.Features.Quotations.Commands.DeleteQuotation;
using Application.Features.Quotations.Commands.RejectQuotation;
using Application.Features.Quotations.Commands.SendQuotation;
using Application.Features.Quotations.Commands.UpdateQuotation;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Quotation;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QuotationEntity = Domain.Entities.Quotation;
using SupplierEntity = Domain.Entities.Supplier;

namespace UnitTests;

public class Quotation
{
    private readonly Mock<IQuotationInsertRepository> _insertRepoMock;
    private readonly Mock<IQuotationReadRepository> _readRepoMock;
    private readonly Mock<IQuotationUpdateRepository> _updateRepoMock;
    private readonly Mock<IQuotationDeleteRepository> _deleteRepoMock;
    private readonly Mock<ISupplierReadRepository> _supplierRepoMock;
    private readonly Mock<IProductVariantReadRepository> _variantRepoMock;
    private readonly Mock<IPermissionReadRepository> _permissionRepoMock;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public Quotation()
    {
        _insertRepoMock = new Mock<IQuotationInsertRepository>();
        _readRepoMock = new Mock<IQuotationReadRepository>();
        _updateRepoMock = new Mock<IQuotationUpdateRepository>();
        _deleteRepoMock = new Mock<IQuotationDeleteRepository>();
        _supplierRepoMock = new Mock<ISupplierReadRepository>();
        _variantRepoMock = new Mock<IProductVariantReadRepository>();
        _permissionRepoMock = new Mock<IPermissionReadRepository>();
        _currentUserContextMock = new Mock<ICurrentUserContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact(DisplayName = "QUO_001 - Tạo Quotation thành công với đầy đủ thông tin hợp lệ")]
    public async Task CreateQuotation_ValidData_Success()
    {
        var handler = new CreateQuotationCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _supplierRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateQuotationCommand
        {
            SupplierId = 1,
            Notes = "Valid notes",
            Products =
            [
                new CreateQuotationItemRequest
                {
                    ProductVariantId = "1",
                    ProductVarientColorId = "2",
                    QuotePrice = 100,
                    Note = "Item note"
                }
            ]
        };

        var supplier = new SupplierEntity { Id = 1, StatusId = Domain.Constants.SupplierStatus.Active, Name = "Active Supplier" };
        var variant = new ProductVariant
        {
            Id = 1,
            ProductVariantColors = [new ProductVariantColor { Id = 2 }]
        };

        _supplierRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync(supplier);
        _variantRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync([variant]);
        _insertRepoMock.Setup(x => x.Add(It.IsAny<QuotationEntity>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuotationEntity { Id = 1, Status = "draft" });

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be("draft");
        _insertRepoMock.Verify(x => x.Add(It.IsAny<QuotationEntity>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "QUO_002 - Tạo Quotation thất bại khi nhà cung cấp không tồn tại")]
    public async Task CreateQuotation_SupplierNotFound_ReturnsNotFound()
    {
        var handler = new CreateQuotationCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _supplierRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateQuotationCommand
        {
            SupplierId = 999,
            Products = [new CreateQuotationItemRequest { ProductVariantId = "1", QuotePrice = 100 }]
        };

        _supplierRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync((SupplierEntity?)null);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("không tồn tại");
    }

    [Fact(DisplayName = "QUO_003 - Tạo Quotation thất bại khi nhà cung cấp không hoạt động")]
    public async Task CreateQuotation_InactiveSupplier_ReturnsBadRequest()
    {
        var handler = new CreateQuotationCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _supplierRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateQuotationCommand
        {
            SupplierId = 1,
            Products = [new CreateQuotationItemRequest { ProductVariantId = "1", QuotePrice = 100 }]
        };

        var supplier = new SupplierEntity { Id = 1, StatusId = "inactive", Name = "Inactive Supplier" };

        _supplierRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync(supplier);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("không ở trạng thái 'active'");
    }

    [Fact(DisplayName = "QUO_004 - Tạo Quotation thất bại khi biến thể sản phẩm không tồn tại")]
    public async Task CreateQuotation_ProductVariantNotFound_ReturnsBadRequest()
    {
        var handler = new CreateQuotationCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _supplierRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateQuotationCommand
        {
            SupplierId = 1,
            Products = [new CreateQuotationItemRequest { ProductVariantId = "999", QuotePrice = 100 }]
        };

        var supplier = new SupplierEntity { Id = 1, StatusId = Domain.Constants.SupplierStatus.Active, Name = "Active Supplier" };

        _supplierRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync(supplier);
        _variantRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync([]);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("không tồn tại");
    }

    [Fact(DisplayName = "QUO_005 - Tạo Quotation thất bại khi mã màu không thuộc biến thể sản phẩm")]
    public async Task CreateQuotation_InvalidColor_ReturnsBadRequest()
    {
        var handler = new CreateQuotationCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _supplierRepoMock.Object,
            _variantRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateQuotationCommand
        {
            SupplierId = 1,
            Products =
            [
                new CreateQuotationItemRequest
                {
                    ProductVariantId = "1",
                    ProductVarientColorId = "99",
                    QuotePrice = 100
                }
            ]
        };

        var supplier = new SupplierEntity { Id = 1, StatusId = Domain.Constants.SupplierStatus.Active, Name = "Active Supplier" };
        var variant = new ProductVariant
        {
            Id = 1,
            ProductVariantColors = [new ProductVariantColor { Id = 2 }]
        };

        _supplierRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync(supplier);
        _variantRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync([variant]);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("không thuộc biến thể sản phẩm");
    }

    [Fact(DisplayName = "QUO_006 - Gửi báo giá thành công khi đang ở trạng thái Nháp")]
    public async Task SendQuotation_FromDraft_Success()
    {
        var handler = new SendQuotationCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new SendQuotationCommand(1);
        var quotation = new QuotationEntity { Id = 1, Status = "draft" };

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quotation);
        _updateRepoMock.Setup(x => x.Update(It.IsAny<QuotationEntity>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        quotation.Status.Should().Be("sent");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<QuotationEntity>()), Times.Once);
    }

    [Fact(DisplayName = "QUO_007 - Gửi báo giá thất bại khi không ở trạng thái Nháp")]
    public async Task SendQuotation_NotFromDraft_ReturnsBadRequest()
    {
        var handler = new SendQuotationCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new SendQuotationCommand(1);
        var quotation = new QuotationEntity { Id = 1, Status = "sent" };

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quotation);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Chỉ cho phép gửi báo giá ở trạng thái Nháp");
    }

    [Fact(DisplayName = "QUO_008 - Xác nhận báo giá thành công khi ở trạng thái Đã gửi")]
    public async Task ApproveQuotation_FromSent_Success()
    {
        var handler = new ApproveQuotationCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new ApproveQuotationCommand(1);
        var quotation = new QuotationEntity { Id = 1, Status = "sent" };

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quotation);
        _updateRepoMock.Setup(x => x.Update(It.IsAny<QuotationEntity>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        quotation.Status.Should().Be("approved");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<QuotationEntity>()), Times.Once);
    }

    [Fact(DisplayName = "QUO_009 - Xác nhận báo giá thất bại khi không ở trạng thái Đã gửi")]
    public async Task ApproveQuotation_NotFromSent_ReturnsBadRequest()
    {
        var handler = new ApproveQuotationCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new ApproveQuotationCommand(1);
        var quotation = new QuotationEntity { Id = 1, Status = "draft" };

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quotation);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Chỉ cho phép xác nhận báo giá ở trạng thái Đã gửi");
    }

    [Fact(DisplayName = "QUO_010 - Hủy báo giá thành công khi ở trạng thái Đã gửi")]
    public async Task RejectQuotation_FromSent_Success()
    {
        var handler = new RejectQuotationCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new RejectQuotationCommand(1);
        var quotation = new QuotationEntity { Id = 1, Status = "sent" };

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quotation);
        _updateRepoMock.Setup(x => x.Update(It.IsAny<QuotationEntity>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        quotation.Status.Should().Be("rejected");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<QuotationEntity>()), Times.Once);
    }

    [Fact(DisplayName = "QUO_011 - Hủy báo giá thất bại khi không ở trạng thái Đã gửi")]
    public async Task RejectQuotation_NotFromSent_ReturnsBadRequest()
    {
        var handler = new RejectQuotationCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new RejectQuotationCommand(1);
        var quotation = new QuotationEntity { Id = 1, Status = "approved" };

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quotation);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Chỉ cho phép hủy báo giá ở trạng thái Đã gửi");
    }

    [Fact(DisplayName = "QUO_012 - Cập nhật toàn bộ thông tin báo giá Nháp thành công")]
    public async Task UpdateQuotation_FromDraft_Success()
    {
        var handler = new UpdateQuotationCommandHandler(
            _updateRepoMock.Object,
            _readRepoMock.Object,
            _supplierRepoMock.Object,
            _variantRepoMock.Object, 
            _permissionRepoMock.Object, 
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateQuotationCommand
        {
            Id = 1,
            SupplierId = 2,
            Notes = "Updated Notes",
            Products = [new UpdateQuotationItemRequest { ProductVariantId = "1", QuotePrice = 200 }]
        };

        var quotation = new QuotationEntity { Id = 1, SupplierId = 1, Status = "draft" };
        var supplier = new SupplierEntity { Id = 2, StatusId = Domain.Constants.SupplierStatus.Active, Name = "Active Supplier" };
        var variant = new ProductVariant { Id = 1 };

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quotation);
        _supplierRepoMock.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync(supplier);
        _variantRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync([variant]);
        _updateRepoMock.Setup(x => x.Update(It.IsAny<QuotationEntity>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        quotation.SupplierId.Should().Be(2);
        quotation.Note.Should().Be("Updated Notes");
        _updateRepoMock.Verify(x => x.Update(It.IsAny<QuotationEntity>()), Times.Once);
    }

    [Fact(DisplayName = "QUO_013 - Chỉ cho phép cập nhật ghi chú cho báo giá đã được Duyệt hoặc Hủy")]
    public async Task UpdateQuotation_ApprovedNotesOnly_Success()
    {
        var handler = new UpdateQuotationCommandHandler(
            _updateRepoMock.Object,
            _readRepoMock.Object,
            _supplierRepoMock.Object,
            _variantRepoMock.Object,
            _permissionRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateQuotationCommand
        {
            Id = 1,
            SupplierId = 1,
            Notes = "Only Update Notes",
            Products = [new UpdateQuotationItemRequest { Id = 10, ProductVariantId = "1", QuotePrice = 100, Note = "Row note" }]
        };

        var quotation = new QuotationEntity
        {
            Id = 1,
            SupplierId = 1,
            Status = "approved",
            QuotationProductRows =
            [
                new QuotationProductRow { Id = 10, ProductVariantId = 1, QuotePrice = 100, Note = "Row note" }
            ]
        };

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quotation);
        _updateRepoMock.Setup(x => x.Update(It.IsAny<QuotationEntity>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        quotation.Note.Should().Be("Only Update Notes");
    }

    [Fact(DisplayName = "QUO_014 - Cập nhật báo giá đã duyệt thất bại nếu thay đổi sản phẩm hoặc nhà cung cấp")]
    public async Task UpdateQuotation_ApprovedModifyOther_ReturnsBadRequest()
    {
        var handler = new UpdateQuotationCommandHandler(
            _updateRepoMock.Object,
            _readRepoMock.Object,
            _supplierRepoMock.Object,
            _variantRepoMock.Object,
            _permissionRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateQuotationCommand
        {
            Id = 1,
            SupplierId = 2,
            Notes = "Update Notes",
            Products = [new UpdateQuotationItemRequest { Id = 10, ProductVariantId = "1", QuotePrice = 100 }]
        };

        var quotation = new QuotationEntity
        {
            Id = 1,
            SupplierId = 1,
            Status = "approved",
            QuotationProductRows =
            [
                new QuotationProductRow { Id = 10, ProductVariantId = 1, QuotePrice = 100 }
            ]
        };

        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quotation);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Chỉ cho phép cập nhật ghi chú");
    }

    [Fact(DisplayName = "QUO_015 - Xóa báo giá thành công cho báo giá Nháp hoặc Đã gửi")]
    public async Task DeleteQuotation_Draft_Success()
    {
        var handler = new DeleteQuotationCommandHandler(
            _deleteRepoMock.Object,
            _readRepoMock.Object,
            _permissionRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);

        var command = new DeleteQuotationCommand { Id = 1 };
        var quotation = new QuotationEntity { Id = 1, Status = "draft" };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync(quotation);
        _deleteRepoMock.Setup(x => x.Delete(It.IsAny<QuotationEntity>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        _deleteRepoMock.Verify(x => x.Delete(It.IsAny<QuotationEntity>()), Times.Once);
    }

    [Fact(DisplayName = "QUO_016 - Xóa báo giá đã duyệt thành công nếu người dùng có quyền duyệt")]
    public async Task DeleteQuotation_ApprovedWithPermission_Success()
    {
        var handler = new DeleteQuotationCommandHandler(
            _deleteRepoMock.Object,
            _readRepoMock.Object,
            _permissionRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);

        var command = new DeleteQuotationCommand { Id = 1 };
        var quotation = new QuotationEntity { Id = 1, Status = "approved" };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync(quotation);
        _deleteRepoMock.Setup(x => x.Delete(It.IsAny<QuotationEntity>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _permissionRepoMock.Setup(x => x.CheckUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        _deleteRepoMock.Verify(x => x.Delete(It.IsAny<QuotationEntity>()), Times.Once);
    }

    [Fact(DisplayName = "QUO_017 - Xóa báo giá đã duyệt thất bại nếu người dùng không có quyền duyệt")]
    public async Task DeleteQuotation_ApprovedNoPermission_ReturnsBadRequest()
    {
        var handler = new DeleteQuotationCommandHandler(
            _deleteRepoMock.Object,
            _readRepoMock.Object,
            _permissionRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);

        var command = new DeleteQuotationCommand { Id = 1 };
        var quotation = new QuotationEntity { Id = 1, Status = "approved" };

        _permissionRepoMock.Setup(x => x.CheckUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync(quotation);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Báo giá ở trạng thái đã gửi hoặc đã duyệt chỉ có thể xóa bởi người dùng có quyền duyệt/hủy báo giá");
    }

    [Fact(DisplayName = "QUO_018 - Lỗi validation khi gửi yêu cầu không có sản phẩm")]
    public void CreateQuotation_EmptyProducts_FailsValidation()
    {
        var validator = new CreateQuotationCommandValidator();
        var command = new CreateQuotationCommand
        {
            SupplierId = 1,
            Products = []
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Products)
            .WithErrorMessage("Quotation must contain at least one product.");
    }

    [Fact(DisplayName = "QUO_019 - Lỗi validation khi trùng biến thể sản phẩm và màu")]
    public void CreateQuotation_DuplicateProducts_FailsValidation()
    {
        var validator = new CreateQuotationCommandValidator();
        var command = new CreateQuotationCommand
        {
            SupplierId = 1,
            Products =
            [
                new CreateQuotationItemRequest { ProductVariantId = "1", ProductVarientColorId = "2", QuotePrice = 100 },
                new CreateQuotationItemRequest { ProductVariantId = "1", ProductVarientColorId = "2", QuotePrice = 200 }
            ]
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Products)
            .WithErrorMessage("Product Variant and Color combination cannot be duplicated in a single quotation.");
    }

    [Fact(DisplayName = "QUO_020 - Lỗi validation khi giá báo của sản phẩm là số âm")]
    public void CreateQuotation_NegativePrice_FailsValidation()
    {
        var validator = new CreateQuotationCommandValidator();
        var command = new CreateQuotationCommand
        {
            SupplierId = 1,
            Products =
            [
                new CreateQuotationItemRequest { ProductVariantId = "1", QuotePrice = -100 }
            ]
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Products[0].QuotePrice")
            .WithErrorMessage("QuotePrice must be greater than or equal to 0.");
    }

    [Fact(DisplayName = "QUO_021 - Lỗi validation khi độ dài ghi chú vượt quá giới hạn")]
    public void CreateQuotation_NotesTooLong_FailsValidation()
    {
        var validator = new CreateQuotationCommandValidator();
        var command = new CreateQuotationCommand
        {
            SupplierId = 1,
            Notes = new string('A', 1001),
            Products = [new CreateQuotationItemRequest { ProductVariantId = "1", QuotePrice = 100 }]
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage("Notes cannot exceed 1000 characters.");
    }
}
