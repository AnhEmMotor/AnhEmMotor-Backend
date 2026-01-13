using Application.Features.ProductCategories.Commands.CreateProductCategory;
using Application.Features.ProductCategories.Commands.DeleteProductCategory;
using Application.Features.ProductCategories.Commands.RestoreProductCategory;
using Application.Features.ProductCategories.Commands.UpdateProductCategory;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Services;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using ProductCategoryEntity = Domain.Entities.ProductCategory;

namespace UnitTests;

public class ProductCategory
{
    private readonly Mock<IProductCategoryInsertRepository> _insertRepoMock;
    private readonly Mock<IProductCategoryUpdateRepository> _updateRepoMock;
    private readonly Mock<IProductCategoryDeleteRepository> _deleteRepoMock;
    private readonly Mock<IProductCategoryReadRepository> _readRepoMock;
    private readonly Mock<IProtectedProductCategoryService> _protectedCategoryServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public ProductCategory()
    {
        _insertRepoMock = new Mock<IProductCategoryInsertRepository>();
        _updateRepoMock = new Mock<IProductCategoryUpdateRepository>();
        _deleteRepoMock = new Mock<IProductCategoryDeleteRepository>();
        _readRepoMock = new Mock<IProductCategoryReadRepository>();
        _protectedCategoryServiceMock = new Mock<IProtectedProductCategoryService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact(DisplayName = "PC_001 - Tạo danh mục sản phẩm thành công (Happy Path)")]
    public async Task CreateProductCategory_WithValidData_ShouldSucceed()
    {
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Điện thoại", Description = "Các sản phẩm điện thoại" };

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>()))
            .Returns(new List<ProductCategoryEntity>().AsQueryable());

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.Value.Should().NotBeNull();
        resultObj.Value.Name.Should().Be("Điện thoại");
        resultObj.Value.Description.Should().Be("Các sản phẩm điện thoại");
        _insertRepoMock.Verify(
            x => x.Add(It.Is<ProductCategoryEntity>(c => string.Compare(c.Name, "Điện thoại") == 0)),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

#pragma warning disable CRR0035
    [Fact(DisplayName = "PC_002 - Tạo danh mục sản phẩm chỉ với Name (Description null)")]
    public async Task CreateProductCategory_WithNameOnly_ShouldSucceed()
    {
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Laptop", Description = null };

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>()))
            .Returns(new List<ProductCategoryEntity>().AsQueryable());

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.Value.Should().NotBeNull();
        resultObj.Value.Name.Should().Be("Laptop");
        resultObj.Value.Description.Should().BeNull();
        _insertRepoMock.Verify(x => x.Add(It.IsAny<ProductCategoryEntity>()), Times.Once);
    }

    [Fact(DisplayName = "PC_003 - Tạo danh mục sản phẩm chỉ với Name (Description empty string)")]
    public async Task CreateProductCategory_WithEmptyDescription_ShouldConvertToNull()
    {
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Tablet", Description = string.Empty };

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>()))
            .Returns(new List<ProductCategoryEntity>().AsQueryable());

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.Value.Should().NotBeNull();
        resultObj.Value.Name.Should().Be("Tablet");
        resultObj.Value.Description.Should().BeNull();
    }

    [Fact(DisplayName = "PC_004 - Tạo danh mục sản phẩm với Name có khoảng trắng đầu/cuối")]
    public async Task CreateProductCategory_WithNameWhitespace_ShouldTrim()
    {
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "  Phụ kiện  ", Description = "Test" };

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>()))
            .Returns(new List<ProductCategoryEntity>().AsQueryable());

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.Value.Should().NotBeNull();
        resultObj.Value.Name.Should().Be("Phụ kiện");
    }

    [Fact(DisplayName = "PC_005 - Tạo danh mục sản phẩm với Description có khoảng trắng đầu/cuối")]
    public async Task CreateProductCategory_WithDescriptionWhitespace_ShouldTrim()
    {
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Máy tính", Description = "  Mô tả test  " };

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>()))
            .Returns(new List<ProductCategoryEntity>().AsQueryable());

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.Value.Should().NotBeNull();
        resultObj.Value.Description.Should().Be("Mô tả test");
    }

    [Fact(DisplayName = "PC_006 - Tạo danh mục sản phẩm với Name chứa ký tự đặc biệt")]
    public async Task CreateProductCategory_WithSpecialCharacters_ShouldSucceed()
    {
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Đồ điện tử & Công nghệ <Tag>", Description = "Test" };

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>()))
            .Returns(new List<ProductCategoryEntity>().AsQueryable());

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.Value.Should().NotBeNull();
        resultObj.Value.Name.Should().Be("Đồ điện tử & Công nghệ <Tag>");
    }

    [Fact(DisplayName = "PC_007 - Tạo danh mục sản phẩm thiếu Name (Name null)")]
    public void CreateProductCategory_WithNullName_ShouldFailValidation()
    {
        var validator = new CreateProductCategoryCommandValidator();
        var command = new CreateProductCategoryCommand { Name = null, Description = "Test" };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "PC_008 - Tạo danh mục sản phẩm thiếu Name (Name empty)")]
    public void CreateProductCategory_WithEmptyName_ShouldFailValidation()
    {
        var validator = new CreateProductCategoryCommandValidator();
        var command = new CreateProductCategoryCommand { Name = string.Empty, Description = "Test" };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "PC_009 - Tạo danh mục sản phẩm với Name trùng (case-sensitive)")]
    public async Task CreateProductCategory_WithDuplicateName_ShouldThrowException()
    {
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Gaming", Description = "Test" };

        var existingCategories = new List<ProductCategoryEntity> { new() { Name = "Gaming", DeletedAt = null } }.AsQueryable(
            );
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>())).Returns(existingCategories);

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_010 - Tạo danh mục sản phẩm với Name trùng (case-insensitive)")]
    public async Task CreateProductCategory_WithDuplicateNameCaseInsensitive_ShouldThrowException()
    {
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "gaming", Description = "Test" };

        var existingCategories = new List<ProductCategoryEntity> { new() { Name = "Gaming", DeletedAt = null } }.AsQueryable(
            );
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>())).Returns(existingCategories);

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_011 - Tạo danh mục sản phẩm với Name trùng bản ghi đã xóa")]
    public async Task CreateProductCategory_WithNameOfDeletedCategory_ShouldThrowException()
    {
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Đã xóa", Description = "Test" };

        var existingCategories = new List<ProductCategoryEntity>
        {
            new() { Name = "Đã xóa", DeletedAt = DateTime.UtcNow }
        }.AsQueryable();
        _readRepoMock.Setup(x => x.GetQueryable(Domain.Constants.DataFetchMode.All)).Returns(existingCategories);

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_012 - Tạo danh mục sản phẩm với dữ liệu rác trong request")]
    public async Task CreateProductCategory_WithExtraFields_ShouldIgnoreExtraData()
    {
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Valid", Description = "Valid" };

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>()))
            .Returns(new List<ProductCategoryEntity>().AsQueryable());

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.Value.Should().NotBeNull();
        resultObj.Value.Name.Should().Be("Valid");
        resultObj.Value.Description.Should().Be("Valid");
    }

    [Fact(DisplayName = "PC_013 - Cập nhật danh mục sản phẩm thành công (cả Name và Description)")]
    public async Task UpdateProductCategory_WithBothFields_ShouldSucceed()
    {
        var handler = new UpdateProductCategoryCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateProductCategoryCommand
        {
            Id = 1,
            Name = "Updated Name",
            Description = "Updated Description"
        };

        _readRepoMock.Setup(
            x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync(
                new ProductCategoryEntity { Id = 1, Name = "Original", Description = "Original Desc", DeletedAt = null });
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>()))
            .Returns(new List<ProductCategoryEntity>().AsQueryable());

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.Value.Should().NotBeNull();
        resultObj.Value!.Name.Should().Be("Updated Name");
        resultObj.Value.Description.Should().Be("Updated Description");
        resultObj.IsSuccess.Should().BeTrue();
        _updateRepoMock.Verify(
            x => x.Update(It.Is<ProductCategoryEntity>(c => string.Compare(c.Name, "Updated Name") == 0)),
            Times.Once);
    }

    [Fact(DisplayName = "PC_014 - Cập nhật danh mục sản phẩm chỉ Name")]
    public async Task UpdateProductCategory_OnlyName_ShouldKeepDescription()
    {
        var handler = new UpdateProductCategoryCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateProductCategoryCommand { Id = 2, Name = "Only Name Updated", Description = null };

        _readRepoMock.Setup(
            x => x.GetByIdAsync(2, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync(
                new ProductCategoryEntity { Id = 2, Name = "Original", Description = "Keep This", DeletedAt = null });
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>()))
            .Returns(new List<ProductCategoryEntity>().AsQueryable());

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.Value.Should().NotBeNull();
        resultObj.Value!.Name.Should().Be("Only Name Updated");
        resultObj.Value.Description.Should().Be("Keep This");
        resultObj.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_015 - Cập nhật danh mục sản phẩm chỉ Description")]
    public async Task UpdateProductCategory_OnlyDescription_ShouldKeepName()
    {
        var handler = new UpdateProductCategoryCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateProductCategoryCommand { Id = 3, Name = null, Description = "Only Description Updated" };

        _readRepoMock.Setup(
            x => x.GetByIdAsync(3, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync(
                new ProductCategoryEntity { Id = 3, Name = "Keep This", Description = "Original", DeletedAt = null });
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>()))
            .Returns(new List<ProductCategoryEntity>().AsQueryable());

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.Value.Should().NotBeNull();
        resultObj.Value!.Name.Should().Be("Keep This");
        resultObj.Value.Description.Should().Be("Only Description Updated");
        resultObj.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_016 - Cập nhật danh mục sản phẩm với body rỗng")]
    public void UpdateProductCategory_WithEmptyBody_ShouldFailValidation()
    {
        var validator = new UpdateProductCategoryCommandValidator();
        var command = new UpdateProductCategoryCommand { Id = 4, Name = null, Description = null };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact(DisplayName = "PC_017 - Cập nhật danh mục sản phẩm với Name trùng (khác Id)")]
    public async Task UpdateProductCategory_WithDuplicateName_ShouldThrowException()
    {
        var handler = new UpdateProductCategoryCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateProductCategoryCommand { Id = 5, Name = "Existing", Description = null };

        _readRepoMock.Setup(
            x => x.GetByIdAsync(5, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync(new ProductCategoryEntity { Id = 5, Name = "Original", DeletedAt = null });

        var existingCategories = new List<ProductCategoryEntity>
        {
            new() { Id = 6, Name = "Existing", DeletedAt = null }
        }.AsQueryable();
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>())).Returns(existingCategories);

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_018 - Cập nhật danh mục sản phẩm không tồn tại")]
    public async Task UpdateProductCategory_NotFound_ShouldThrowException()
    {
        var handler = new UpdateProductCategoryCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateProductCategoryCommand { Id = 999, Name = "Updated" };

        _readRepoMock.Setup(
            x => x.GetByIdAsync(999, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync((ProductCategoryEntity?)null);

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_019 - Cập nhật danh mục sản phẩm đã bị xóa")]
    public async Task UpdateProductCategory_DeletedCategory_ShouldThrowException()
    {
        var handler = new UpdateProductCategoryCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateProductCategoryCommand { Id = 7, Name = "Updated" };

        _readRepoMock.Setup(
            x => x.GetByIdAsync(7, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync(new ProductCategoryEntity { Id = 7, Name = "Deleted", DeletedAt = DateTime.UtcNow });

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_020 - Xóa danh mục sản phẩm thành công")]
    public async Task DeleteProductCategory_ValidId_ShouldSucceed()
    {
        var handler = new DeleteProductCategoryCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _protectedCategoryServiceMock.Object,
            _unitOfWorkMock.Object);
        var command = new DeleteProductCategoryCommand { Id = 8 };

        _readRepoMock.Setup(
            x => x.GetByIdAsync(8, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync(new ProductCategoryEntity { Id = 8, Name = "To Delete", DeletedAt = null });
        _protectedCategoryServiceMock.Setup(x => x.IsProtectedAsync(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.IsSuccess.Should().BeTrue();
        _deleteRepoMock.Verify(x => x.Delete(It.Is<ProductCategoryEntity>(c => c.Id == 8)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PC_021 - Xóa danh mục sản phẩm không tồn tại")]
    public async Task DeleteProductCategory_NotFound_ShouldThrowException()
    {
        var handler = new DeleteProductCategoryCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _protectedCategoryServiceMock.Object,
            _unitOfWorkMock.Object);
        var command = new DeleteProductCategoryCommand { Id = 999 };

        _readRepoMock.Setup(
            x => x.GetByIdAsync(999, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync((ProductCategoryEntity?)null);

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_022 - Xóa danh mục sản phẩm đã bị xóa")]
    public async Task DeleteProductCategory_AlreadyDeleted_ShouldThrowException()
    {
        var handler = new DeleteProductCategoryCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _protectedCategoryServiceMock.Object,
            _unitOfWorkMock.Object);
        var command = new DeleteProductCategoryCommand { Id = 9 };

        _readRepoMock.Setup(
            x => x.GetByIdAsync(9, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync(new ProductCategoryEntity { Id = 9, Name = "Already Deleted", DeletedAt = DateTime.UtcNow });

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_023 - Khôi phục danh mục sản phẩm thành công")]
    public async Task RestoreProductCategory_ValidId_ShouldSucceed()
    {
        var handler = new RestoreProductCategoryCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new RestoreProductCategoryCommand { Id = 10 };

        _readRepoMock.Setup(
            x => x.GetByIdAsync(10, It.IsAny<CancellationToken>(), Domain.Constants.DataFetchMode.DeletedOnly))
            .ReturnsAsync(new ProductCategoryEntity { Id = 10, Name = "To Restore", DeletedAt = DateTime.UtcNow });

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.IsSuccess.Should().BeTrue();
        resultObj.Value.Should().NotBeNull();
        _updateRepoMock.Verify(x => x.Restore(It.Is<ProductCategoryEntity>(c => c.Id == 10)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PC_024 - Khôi phục danh mục sản phẩm chưa bị xóa")]
    public async Task RestoreProductCategory_NotDeleted_ShouldThrowException()
    {
        var handler = new RestoreProductCategoryCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new RestoreProductCategoryCommand { Id = 11 };

        _readRepoMock.Setup(
            x => x.GetByIdAsync(11, It.IsAny<CancellationToken>(), Domain.Constants.DataFetchMode.DeletedOnly))
            .ReturnsAsync((ProductCategoryEntity?)null);

        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        resultObj.IsFailure.Should().BeTrue();
    }
#pragma warning restore CRR0035
}
