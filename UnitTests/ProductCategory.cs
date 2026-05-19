using Application.Features.ProductCategories.Commands.CreateProductCategory;
using Application.Features.ProductCategories.Commands.DeleteProductCategory;
using Application.Features.ProductCategories.Commands.RestoreProductCategory;
using Application.Features.ProductCategories.Commands.UpdateProductCategory;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Features.ProductCategories.Queries.GetProductCategoryStats;
using Application.Interfaces.Services;
using Application.ApiContracts.ProductCategory.Responses;
using Domain.Constants;
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

    [Fact(DisplayName = "PC_001 - T?o danh m?c s?n ph?m th�nh c�ng (Happy Path)")]
    public async Task CreateProductCategory_WithValidData_ShouldSucceed()
    {
        var handler = new CreateProductCategoryCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "�i?n tho?i", Description = "C�c s?n ph?m di?n tho?i" };
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.Value.Should().NotBeNull();
        resultObj.Value.Name.Should().Be("�i?n tho?i");
        resultObj.Value.Description.Should().Be("C�c s?n ph?m di?n tho?i");
        _insertRepoMock.Verify(
            x => x.Add(It.Is<ProductCategoryEntity>(c => string.Compare(c.Name, "�i?n tho?i") == 0)),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #pragma warning disable IDE0079 
    #pragma warning disable CRR0035
    [Fact(DisplayName = "PC_002 - T?o danh m?c s?n ph?m ch? v?i Name (Description null)")]
    public async Task CreateProductCategory_WithNameOnly_ShouldSucceed()
    {
        var handler = new CreateProductCategoryCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Laptop", Description = null };
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.Value.Should().NotBeNull();
        resultObj.Value.Name.Should().Be("Laptop");
        resultObj.Value.Description.Should().BeNull();
        _insertRepoMock.Verify(x => x.Add(It.IsAny<ProductCategoryEntity>()), Times.Once);
    }

    [Fact(DisplayName = "PC_003 - T?o danh m?c s?n ph?m ch? v?i Name (Description empty string)")]
    public async Task CreateProductCategory_WithEmptyDescription_ShouldConvertToNull()
    {
        var handler = new CreateProductCategoryCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Tablet", Description = string.Empty };
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.Value.Should().NotBeNull();
        resultObj.Value.Name.Should().Be("Tablet");
        resultObj.Value.Description.Should().BeNull();
    }

    [Fact(DisplayName = "PC_004 - T?o danh m?c s?n ph?m v?i Name c� kho?ng tr?ng d?u/cu?i")]
    public async Task CreateProductCategory_WithNameWhitespace_ShouldTrim()
    {
        var handler = new CreateProductCategoryCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "  Ph? ki?n  ", Description = "Test" };
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.Value.Should().NotBeNull();
        resultObj.Value.Name.Should().Be("Ph? ki?n");
    }

    [Fact(DisplayName = "PC_005 - T?o danh m?c s?n ph?m v?i Description c� kho?ng tr?ng d?u/cu?i")]
    public async Task CreateProductCategory_WithDescriptionWhitespace_ShouldTrim()
    {
        var handler = new CreateProductCategoryCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "M�y t�nh", Description = "  M� t? test  " };
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.Value.Should().NotBeNull();
        resultObj.Value.Description.Should().Be("M� t? test");
    }

    [Fact(DisplayName = "PC_006 - T?o danh m?c s?n ph?m v?i Name ch?a k� t? d?c bi?t")]
    public async Task CreateProductCategory_WithSpecialCharacters_ShouldSucceed()
    {
        var handler = new CreateProductCategoryCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "�? di?n t? & C�ng ngh? <Tag>", Description = "Test" };
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.Value.Should().NotBeNull();
        resultObj.Value.Name.Should().Be("�? di?n t? & C�ng ngh? <Tag>");
    }

    [Fact(DisplayName = "PC_007 - T?o danh m?c s?n ph?m thi?u Name (Name null)")]
    public void CreateProductCategory_WithNullName_ShouldFailValidation()
    {
        var validator = new CreateProductCategoryCommandValidator();
        var command = new CreateProductCategoryCommand { Name = null, Description = "Test" };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "PC_008 - T?o danh m?c s?n ph?m thi?u Name (Name empty)")]
    public void CreateProductCategory_WithEmptyName_ShouldFailValidation()
    {
        var validator = new CreateProductCategoryCommandValidator();
        var command = new CreateProductCategoryCommand { Name = string.Empty, Description = "Test" };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "PC_009 - T?o danh m?c s?n ph?m v?i Name tr�ng (case-sensitive)")]
    public async Task CreateProductCategory_WithDuplicateName_ShouldThrowException()
    {
        var handler = new CreateProductCategoryCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Gaming", Description = "Test" };
        _readRepoMock.Setup(
            x => x.ExistsByNameAsync(
                It.Is<string>(s => s.Equals("gaming", StringComparison.OrdinalIgnoreCase)),
                It.IsAny<CancellationToken>(),
                DataFetchMode.All))
            .ReturnsAsync(true);
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_010 - T?o danh m?c s?n ph?m v?i Name tr�ng (case-insensitive)")]
    public async Task CreateProductCategory_WithDuplicateNameCaseInsensitive_ShouldThrowException()
    {
        var handler = new CreateProductCategoryCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "gaming", Description = "Test" };
        _readRepoMock.Setup(
            x => x.ExistsByNameAsync(
                It.Is<string>(s => s.Equals("gaming", StringComparison.OrdinalIgnoreCase)),
                It.IsAny<CancellationToken>(),
                DataFetchMode.All))
            .ReturnsAsync(true);
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_011 - T?o danh m?c s?n ph?m v?i Name tr�ng b?n ghi d� x�a")]
    public async Task CreateProductCategory_WithNameOfDeletedCategory_ShouldThrowException()
    {
        var handler = new CreateProductCategoryCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "�� x�a", Description = "Test" };
        _readRepoMock.Setup(x => x.ExistsByNameAsync("�� x�a", It.IsAny<CancellationToken>(), DataFetchMode.All))
            .ReturnsAsync(true);
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_012 - T?o danh m?c s?n ph?m v?i d? li?u r�c trong request")]
    public async Task CreateProductCategory_WithExtraFields_ShouldIgnoreExtraData()
    {
        var handler = new CreateProductCategoryCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateProductCategoryCommand { Name = "Valid", Description = "Valid" };
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.Value.Should().NotBeNull();
        resultObj.Value.Name.Should().Be("Valid");
        resultObj.Value.Description.Should().Be("Valid");
    }

    [Fact(DisplayName = "PC_013 - C?p nh?t danh m?c s?n ph?m th�nh c�ng (c? Name v� Description)")]
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
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(
                new ProductCategoryEntity { Id = 1, Name = "Original", Description = "Original Desc", DeletedAt = null });
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.Value.Should().NotBeNull();
        resultObj.Value!.Name.Should().Be("Updated Name");
        resultObj.Value.Description.Should().Be("Updated Description");
        resultObj.IsSuccess.Should().BeTrue();
        _updateRepoMock.Verify(
            x => x.Update(It.Is<ProductCategoryEntity>(c => string.Compare(c.Name, "Updated Name") == 0)),
            Times.Once);
    }

    [Fact(DisplayName = "PC_014 - C?p nh?t danh m?c s?n ph?m ch? Name")]
    public async Task UpdateProductCategory_OnlyName_ShouldKeepDescription()
    {
        var handler = new UpdateProductCategoryCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateProductCategoryCommand { Id = 2, Name = "Only Name Updated", Description = null };
        _readRepoMock.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(
                new ProductCategoryEntity { Id = 2, Name = "Original", Description = "Keep This", DeletedAt = null });
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.Value.Should().NotBeNull();
        resultObj.Value!.Name.Should().Be("Only Name Updated");
        resultObj.Value.Description.Should().Be("Keep This");
        resultObj.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_015 - C?p nh?t danh m?c s?n ph?m ch? Description")]
    public async Task UpdateProductCategory_OnlyDescription_ShouldKeepName()
    {
        var handler = new UpdateProductCategoryCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateProductCategoryCommand { Id = 3, Name = null, Description = "Only Description Updated" };
        _readRepoMock.Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(
                new ProductCategoryEntity { Id = 3, Name = "Keep This", Description = "Original", DeletedAt = null });
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.Value.Should().NotBeNull();
        resultObj.Value!.Name.Should().Be("Keep This");
        resultObj.Value.Description.Should().Be("Only Description Updated");
        resultObj.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_016 - C?p nh?t danh m?c s?n ph?m v?i body r?ng")]
    public void UpdateProductCategory_WithEmptyBody_ShouldFailValidation()
    {
        var validator = new UpdateProductCategoryCommandValidator();
        var command = new UpdateProductCategoryCommand { Id = 4, Name = null, Description = null };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact(DisplayName = "PC_017 - C?p nh?t danh m?c s?n ph?m v?i Name tr�ng (kh�c Id)")]
    public async Task UpdateProductCategory_WithDuplicateName_ShouldThrowException()
    {
        var handler = new UpdateProductCategoryCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateProductCategoryCommand { Id = 5, Name = "Existing", Description = null };
        _readRepoMock.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(new ProductCategoryEntity { Id = 5, Name = "Original", DeletedAt = null });
        _readRepoMock.Setup(
            x => x.ExistsByNameExceptIdAsync("Existing", 5, It.IsAny<CancellationToken>(), DataFetchMode.All))
            .ReturnsAsync(true);
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_018 - C?p nh?t danh m?c s?n ph?m kh�ng t?n t?i")]
    public async Task UpdateProductCategory_NotFound_ShouldThrowException()
    {
        var handler = new UpdateProductCategoryCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateProductCategoryCommand { Id = 999, Name = "Updated" };
        _readRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync((ProductCategoryEntity?)null);
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_019 - C?p nh?t danh m?c s?n ph?m d� b? x�a")]
    public async Task UpdateProductCategory_DeletedCategory_ShouldThrowException()
    {
        var handler = new UpdateProductCategoryCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateProductCategoryCommand { Id = 7, Name = "Updated" };
        _readRepoMock.Setup(x => x.GetByIdAsync(7, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(new ProductCategoryEntity { Id = 7, Name = "Deleted", DeletedAt = DateTime.UtcNow });
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_021 - X�a danh m?c s?n ph?m kh�ng t?n t?i")]
    public async Task DeleteProductCategory_NotFound_ShouldThrowException()
    {
        var handler = new DeleteProductCategoryCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _protectedCategoryServiceMock.Object,
            _unitOfWorkMock.Object);
        var command = new DeleteProductCategoryCommand { Id = 999 };
        _readRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync((ProductCategoryEntity?)null);
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_022 - X�a danh m?c s?n ph?m d� b? x�a")]
    public async Task DeleteProductCategory_AlreadyDeleted_ShouldThrowException()
    {
        var handler = new DeleteProductCategoryCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _protectedCategoryServiceMock.Object,
            _unitOfWorkMock.Object);
        var command = new DeleteProductCategoryCommand { Id = 9 };
        _readRepoMock.Setup(x => x.GetByIdAsync(9, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(new ProductCategoryEntity { Id = 9, Name = "Already Deleted", DeletedAt = DateTime.UtcNow });
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_023 - Kh�i ph?c danh m?c s?n ph?m th�nh c�ng")]
    public async Task RestoreProductCategory_ValidId_ShouldSucceed()
    {
        var handler = new RestoreProductCategoryCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new RestoreProductCategoryCommand { Id = 10 };
        _readRepoMock.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>(), DataFetchMode.DeletedOnly))
            .ReturnsAsync(new ProductCategoryEntity { Id = 10, Name = "To Restore", DeletedAt = DateTime.UtcNow });
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.IsSuccess.Should().BeTrue();
        resultObj.Value.Should().NotBeNull();
        _updateRepoMock.Verify(x => x.Restore(It.Is<ProductCategoryEntity>(c => c.Id == 10)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PC_024 - Kh�i ph?c danh m?c s?n ph?m chua b? x�a")]
    public async Task RestoreProductCategory_NotDeleted_ShouldThrowException()
    {
        var handler = new RestoreProductCategoryCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new RestoreProductCategoryCommand { Id = 11 };
        _readRepoMock.Setup(x => x.GetByIdAsync(11, It.IsAny<CancellationToken>(), DataFetchMode.DeletedOnly))
            .ReturnsAsync((ProductCategoryEntity?)null);
        var resultObj = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        resultObj.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PC_071 - Lấy thống kê danh mục sản phẩm thành công")]
    public async Task GetProductCategoryStats_ShouldSucceed()
    {
        var handler = new GetProductCategoryStatsQueryHandler(_readRepoMock.Object);

        var statsResponse = new ProductCategoryStatsResponse
        {
            TotalCategories = 2,
            ProductCategoriesCount = 2,
            LatestUpdatedCategoryName = "Danh mục 2",
            LatestUpdatedAt = DateTimeOffset.UtcNow.AddDays(-5)
        };

        _readRepoMock.Setup(r => r.GetStatisticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(statsResponse);

        var query = new GetProductCategoryStatsQuery();
        var resultObj = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        resultObj.IsSuccess.Should().BeTrue();
        resultObj.Value.Should().NotBeNull();
        resultObj.Value.TotalCategories.Should().Be(2);
        resultObj.Value.ProductCategoriesCount.Should().Be(2);
        resultObj.Value.LatestUpdatedCategoryName.Should().Be("Danh mục 2");
    }
    #pragma warning restore CRR0035
    #pragma warning restore IDE0079
}
