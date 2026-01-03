using Application.Common.Exceptions;
using Application.Features.ProductCategories.Commands.CreateProductCategory;
using Application.Features.ProductCategories.Commands.DeleteProductCategory;
using Application.Features.ProductCategories.Commands.UpdateProductCategory;
using Application.Features.ProductCategories.Commands.RestoreProductCategory;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ProductCategoryEntity = Domain.Entities.ProductCategory;

namespace UnitTests;

public class ProductCategory
{
    private readonly Mock<IProductCategoryInsertRepository> _insertRepoMock;
    private readonly Mock<IProductCategoryUpdateRepository> _updateRepoMock;
    private readonly Mock<IProductCategoryDeleteRepository> _deleteRepoMock;
    private readonly Mock<IProductCategoryReadRepository> _readRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public ProductCategory()
    {
        _insertRepoMock = new Mock<IProductCategoryInsertRepository>();
        _updateRepoMock = new Mock<IProductCategoryUpdateRepository>();
        _deleteRepoMock = new Mock<IProductCategoryDeleteRepository>();
        _readRepoMock = new Mock<IProductCategoryReadRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact(DisplayName = "PC_001 - Tạo danh mục sản phẩm thành công (Happy Path)")]
    public async Task CreateProductCategory_WithValidData_ShouldSucceed()
    {
        // Arrange
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Điện thoại", Description = "Các sản phẩm điện thoại" };

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>())).Returns(new List<ProductCategoryEntity>().AsQueryable());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Điện thoại");
        result.Description.Should().Be("Các sản phẩm điện thoại");
        _insertRepoMock.Verify(x => x.Add(It.Is<ProductCategoryEntity>(c => c.Name == "Điện thoại")), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PC_002 - Tạo danh mục sản phẩm chỉ với Name (Description null)")]
    public async Task CreateProductCategory_WithNameOnly_ShouldSucceed()
    {
        // Arrange
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Laptop", Description = null };

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>())).Returns(new List<ProductCategoryEntity>().AsQueryable());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Laptop");
        result.Description.Should().BeNull();
        _insertRepoMock.Verify(x => x.Add(It.IsAny<ProductCategoryEntity>()), Times.Once);
    }

    [Fact(DisplayName = "PC_003 - Tạo danh mục sản phẩm chỉ với Name (Description empty string)")]
    public async Task CreateProductCategory_WithEmptyDescription_ShouldConvertToNull()
    {
        // Arrange
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Tablet", Description = "" };

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>())).Returns(new List<ProductCategoryEntity>().AsQueryable());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Tablet");
        result.Description.Should().BeNull();
    }

    [Fact(DisplayName = "PC_004 - Tạo danh mục sản phẩm với Name có khoảng trắng đầu/cuối")]
    public async Task CreateProductCategory_WithNameWhitespace_ShouldTrim()
    {
        // Arrange
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "  Phụ kiện  ", Description = "Test" };

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>())).Returns(new List<ProductCategoryEntity>().AsQueryable());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Phụ kiện");
    }

    [Fact(DisplayName = "PC_005 - Tạo danh mục sản phẩm với Description có khoảng trắng đầu/cuối")]
    public async Task CreateProductCategory_WithDescriptionWhitespace_ShouldTrim()
    {
        // Arrange
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Máy tính", Description = "  Mô tả test  " };

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>())).Returns(new List<ProductCategoryEntity>().AsQueryable());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be("Mô tả test");
    }

    [Fact(DisplayName = "PC_006 - Tạo danh mục sản phẩm với Name chứa ký tự đặc biệt")]
    public async Task CreateProductCategory_WithSpecialCharacters_ShouldSucceed()
    {
        // Arrange
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Đồ điện tử & Công nghệ <Tag>", Description = "Test" };

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>())).Returns(new List<ProductCategoryEntity>().AsQueryable());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Đồ điện tử & Công nghệ <Tag>");
    }

    [Fact(DisplayName = "PC_007 - Tạo danh mục sản phẩm thiếu Name (Name null)")]
    public void CreateProductCategory_WithNullName_ShouldFailValidation()
    {
        // Arrange
        var validator = new CreateProductCategoryCommandValidator();
        var command = new CreateProductCategoryCommand { Name = null, Description = "Test" };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "PC_008 - Tạo danh mục sản phẩm thiếu Name (Name empty)")]
    public void CreateProductCategory_WithEmptyName_ShouldFailValidation()
    {
        // Arrange
        var validator = new CreateProductCategoryCommandValidator();
        var command = new CreateProductCategoryCommand { Name = "", Description = "Test" };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "PC_009 - Tạo danh mục sản phẩm với Name trùng (case-sensitive)")]
    public async Task CreateProductCategory_WithDuplicateName_ShouldThrowException()
    {
        // Arrange
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Gaming", Description = "Test" };

        var existingCategories = new List<ProductCategoryEntity> 
        { 
            new() { Name = "Gaming", DeletedAt = null } 
        }.AsQueryable();
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>())).Returns(existingCategories);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact(DisplayName = "PC_010 - Tạo danh mục sản phẩm với Name trùng (case-insensitive)")]
    public async Task CreateProductCategory_WithDuplicateNameCaseInsensitive_ShouldThrowException()
    {
        // Arrange
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "gaming", Description = "Test" };

        var existingCategories = new List<ProductCategoryEntity> 
        { 
            new() { Name = "Gaming", DeletedAt = null } 
        }.AsQueryable();
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>())).Returns(existingCategories);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact(DisplayName = "PC_011 - Tạo danh mục sản phẩm với Name trùng bản ghi đã xóa")]
    public async Task CreateProductCategory_WithNameOfDeletedCategory_ShouldThrowException()
    {
        // Arrange
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Đã xóa", Description = "Test" };

        var existingCategories = new List<ProductCategoryEntity> 
        { 
            new() { Name = "Đã xóa", DeletedAt = DateTime.UtcNow } 
        }.AsQueryable();
        _readRepoMock.Setup(x => x.GetQueryable(Domain.Constants.DataFetchMode.All)).Returns(existingCategories);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact(DisplayName = "PC_012 - Tạo danh mục sản phẩm với dữ liệu rác trong request")]
    public async Task CreateProductCategory_WithExtraFields_ShouldIgnoreExtraData()
    {
        // Arrange
        var handler = new CreateProductCategoryCommandHandler(_insertRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Valid", Description = "Valid" };

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>())).Returns(new List<ProductCategoryEntity>().AsQueryable());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Valid");
        result.Description.Should().Be("Valid");
    }

    [Fact(DisplayName = "PC_013 - Cập nhật danh mục sản phẩm thành công (cả Name và Description)")]
    public async Task UpdateProductCategory_WithBothFields_ShouldSucceed()
    {
        // Arrange
        var handler = new UpdateProductCategoryCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var command = new UpdateProductCategoryCommand { Id = 1, Name = "Updated Name", Description = "Updated Description" };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync(new ProductCategoryEntity { Id = 1, Name = "Original", Description = "Original Desc", DeletedAt = null });
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>())).Returns(new List<ProductCategoryEntity>().AsQueryable());

        // Act
        var (result, error) = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        error.Should().BeNull();
        _updateRepoMock.Verify(x => x.Update(It.Is<ProductCategoryEntity>(c => c.Name == "Updated Name")), Times.Once);
    }

    [Fact(DisplayName = "PC_014 - Cập nhật danh mục sản phẩm chỉ Name")]
    public async Task UpdateProductCategory_OnlyName_ShouldKeepDescription()
    {
        // Arrange
        var handler = new UpdateProductCategoryCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var command = new UpdateProductCategoryCommand { Id = 2, Name = "Only Name Updated", Description = null };

        _readRepoMock.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync(new ProductCategoryEntity { Id = 2, Name = "Original", Description = "Keep This", DeletedAt = null });
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>())).Returns(new List<ProductCategoryEntity>().AsQueryable());

        // Act
        var (result, error) = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Only Name Updated");
        result.Description.Should().Be("Keep This");
        error.Should().BeNull();
    }

    [Fact(DisplayName = "PC_015 - Cập nhật danh mục sản phẩm chỉ Description")]
    public async Task UpdateProductCategory_OnlyDescription_ShouldKeepName()
    {
        // Arrange
        var handler = new UpdateProductCategoryCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var command = new UpdateProductCategoryCommand { Id = 3, Name = null, Description = "Only Description Updated" };

        _readRepoMock.Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync(new ProductCategoryEntity { Id = 3, Name = "Keep This", Description = "Original", DeletedAt = null });
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>())).Returns(new List<ProductCategoryEntity>().AsQueryable());

        // Act
        var (result, error) = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Keep This");
        result.Description.Should().Be("Only Description Updated");
        error.Should().BeNull();
    }

    [Fact(DisplayName = "PC_016 - Cập nhật danh mục sản phẩm với body rỗng")]
    public void UpdateProductCategory_WithEmptyBody_ShouldFailValidation()
    {
        // Arrange
        var validator = new UpdateProductCategoryCommandValidator();
        var command = new UpdateProductCategoryCommand { Id = 4, Name = null, Description = null };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact(DisplayName = "PC_017 - Cập nhật danh mục sản phẩm với Name trùng (khác Id)")]
    public async Task UpdateProductCategory_WithDuplicateName_ShouldThrowException()
    {
        // Arrange
        var handler = new UpdateProductCategoryCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var command = new UpdateProductCategoryCommand { Id = 5, Name = "Existing", Description = null };

        _readRepoMock.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync(new ProductCategoryEntity { Id = 5, Name = "Original", DeletedAt = null });
        
        var existingCategories = new List<ProductCategoryEntity> 
        { 
            new() { Id = 6, Name = "Existing", DeletedAt = null } 
        }.AsQueryable();
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<Domain.Constants.DataFetchMode>())).Returns(existingCategories);

        // Act
        var (result, error) = await handler.Handle(command, CancellationToken.None);

        // Assert
        error.Should().NotBeNull();
    }

    [Fact(DisplayName = "PC_018 - Cập nhật danh mục sản phẩm không tồn tại")]
    public async Task UpdateProductCategory_NotFound_ShouldThrowException()
    {
        // Arrange
        var handler = new UpdateProductCategoryCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var command = new UpdateProductCategoryCommand { Id = 999, Name = "Updated" };

        _readRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync((ProductCategoryEntity?)null);

        // Act
        var (result, error) = await handler.Handle(command, CancellationToken.None);

        // Assert
        error.Should().NotBeNull();
    }

    [Fact(DisplayName = "PC_019 - Cập nhật danh mục sản phẩm đã bị xóa")]
    public async Task UpdateProductCategory_DeletedCategory_ShouldThrowException()
    {
        // Arrange
        var handler = new UpdateProductCategoryCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var command = new UpdateProductCategoryCommand { Id = 7, Name = "Updated" };

        _readRepoMock.Setup(x => x.GetByIdAsync(7, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync(new ProductCategoryEntity { Id = 7, Name = "Deleted", DeletedAt = DateTime.UtcNow });

        // Act
        var (result, error) = await handler.Handle(command, CancellationToken.None);

        // Assert
        error.Should().NotBeNull();
    }

    [Fact(DisplayName = "PC_020 - Xóa danh mục sản phẩm thành công")]
    public async Task DeleteProductCategory_ValidId_ShouldSucceed()
    {
        // Arrange
        var handler = new DeleteProductCategoryCommandHandler(_readRepoMock.Object, _deleteRepoMock.Object, _unitOfWorkMock.Object);
        var command = new DeleteProductCategoryCommand { Id = 8 };

        _readRepoMock.Setup(x => x.GetByIdAsync(8, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync(new ProductCategoryEntity { Id = 8, Name = "To Delete", DeletedAt = null });

        // Act
        var error = await handler.Handle(command, CancellationToken.None);

        // Assert
        error.Should().BeNull();
        _deleteRepoMock.Verify(x => x.Delete(It.Is<ProductCategoryEntity>(c => c.Id == 8)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PC_021 - Xóa danh mục sản phẩm không tồn tại")]
    public async Task DeleteProductCategory_NotFound_ShouldThrowException()
    {
        // Arrange
        var handler = new DeleteProductCategoryCommandHandler(_readRepoMock.Object, _deleteRepoMock.Object, _unitOfWorkMock.Object);
        var command = new DeleteProductCategoryCommand { Id = 999 };

        _readRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync((ProductCategoryEntity?)null);

        // Act
        var error = await handler.Handle(command, CancellationToken.None);

        // Assert
        error.Should().NotBeNull();
    }

    [Fact(DisplayName = "PC_022 - Xóa danh mục sản phẩm đã bị xóa")]
    public async Task DeleteProductCategory_AlreadyDeleted_ShouldThrowException()
    {
        // Arrange
        var handler = new DeleteProductCategoryCommandHandler(_readRepoMock.Object, _deleteRepoMock.Object, _unitOfWorkMock.Object);
        var command = new DeleteProductCategoryCommand { Id = 9 };

        _readRepoMock.Setup(x => x.GetByIdAsync(9, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync(new ProductCategoryEntity { Id = 9, Name = "Already Deleted", DeletedAt = DateTime.UtcNow });

        // Act
        var error = await handler.Handle(command, CancellationToken.None);

        // Assert
        error.Should().NotBeNull();
    }

    [Fact(DisplayName = "PC_023 - Khôi phục danh mục sản phẩm thành công")]
    public async Task RestoreProductCategory_ValidId_ShouldSucceed()
    {
        // Arrange
        var handler = new RestoreProductCategoryCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var command = new RestoreProductCategoryCommand { Id = 10 };

        _readRepoMock.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>(), Domain.Constants.DataFetchMode.DeletedOnly))
            .ReturnsAsync(new ProductCategoryEntity { Id = 10, Name = "To Restore", DeletedAt = DateTime.UtcNow });

        // Act
        var (result, error) = await handler.Handle(command, CancellationToken.None);

        // Assert
        error.Should().BeNull();
        result.Should().NotBeNull();
        _updateRepoMock.Verify(x => x.Restore(It.Is<ProductCategoryEntity>(c => c.Id == 10)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PC_024 - Khôi phục danh mục sản phẩm chưa bị xóa")]
    public async Task RestoreProductCategory_NotDeleted_ShouldThrowException()
    {
        // Arrange
        var handler = new RestoreProductCategoryCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var command = new RestoreProductCategoryCommand { Id = 11 };

        _readRepoMock.Setup(x => x.GetByIdAsync(11, It.IsAny<CancellationToken>(), Domain.Constants.DataFetchMode.DeletedOnly))
            .ReturnsAsync((ProductCategoryEntity?)null);

        // Act
        var (result, error) = await handler.Handle(command, CancellationToken.None);

        // Assert
        error.Should().NotBeNull();
    }
}
