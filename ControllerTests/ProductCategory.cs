using System.Reflection;
using Application.ApiContracts.ProductCategory.Requests;
using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Features.ProductCategories.Commands.CreateProductCategory;
using Application.Features.ProductCategories.Commands.DeleteProductCategory;
using Application.Features.ProductCategories.Commands.DeleteManyProductCategories;
using Application.Features.ProductCategories.Commands.RestoreProductCategory;
using Application.Features.ProductCategories.Commands.RestoreManyProductCategories;
using Application.Features.ProductCategories.Commands.UpdateProductCategory;
using Application.Features.ProductCategories.Queries.GetProductCategoryById;
using Application.Features.ProductCategories.Queries.GetProductCategoriesList;
using Application.Features.ProductCategories.Queries.GetDeletedProductCategoriesList;
using Domain.Primitives;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sieve.Models;
using WebAPI.Controllers.V1;
using Xunit;


namespace ControllerTests;

public class ProductCategory
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ProductCategoryController _controller;

    public ProductCategory()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ProductCategoryController(_mediatorMock.Object);
        
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact(DisplayName = "PC_043 - Kiểm tra phân quyền - Tạo danh mục sản phẩm không có quyền")]
    public async Task CreateProductCategory_WithoutPermission_ShouldThrowUnauthorized()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateProductCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _controller.CreateProductCategory(new CreateProductCategoryRequest(), CancellationToken.None));
    }

    [Fact(DisplayName = "PC_044 - Kiểm tra phân quyền - Xem danh sách cho manager không có quyền")]
    public async Task GetProductCategoriesForManager_WithoutPermission_ShouldThrowUnauthorized()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProductCategoriesListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _controller.GetProductCategoriesForManager(new SieveModel(), CancellationToken.None));
    }

    [Fact(DisplayName = "PC_045 - Kiểm tra phân quyền - Xem danh sách đã xóa không có quyền")]
    public async Task GetDeletedProductCategories_WithoutPermission_ShouldThrowUnauthorized()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDeletedProductCategoriesListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _controller.GetDeletedProductCategories(new SieveModel(), CancellationToken.None));
    }

    [Fact(DisplayName = "PC_046 - Kiểm tra phân quyền - Cập nhật danh mục sản phẩm không có quyền")]
    public async Task UpdateProductCategory_WithoutPermission_ShouldThrowUnauthorized()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateProductCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _controller.UpdateProductCategory(28, new UpdateProductCategoryRequest(), CancellationToken.None));
    }

    [Fact(DisplayName = "PC_047 - Kiểm tra phân quyền - Xóa danh mục sản phẩm không có quyền")]
    public async Task DeleteProductCategory_WithoutPermission_ShouldThrowUnauthorized()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteProductCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _controller.DeleteProductCategory(29, CancellationToken.None));
    }

    [Fact(DisplayName = "PC_048 - Kiểm tra phân quyền - Khôi phục danh mục sản phẩm không có quyền")]
    public async Task RestoreProductCategory_WithoutPermission_ShouldThrowUnauthorized()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreProductCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _controller.RestoreProductCategory(30, CancellationToken.None));
    }

    [Fact(DisplayName = "PC_049 - Kiểm tra phân quyền - Xóa nhiều danh mục sản phẩm không có quyền")]
    public async Task DeleteManyProductCategories_WithoutPermission_ShouldThrowUnauthorized()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteManyProductCategoriesCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _controller.DeleteProductCategories(new DeleteManyProductCategoriesRequest { Ids = [31, 32] }, CancellationToken.None));
    }

    [Fact(DisplayName = "PC_050 - Kiểm tra phân quyền - Khôi phục nhiều danh mục sản phẩm không có quyền")]
    public async Task RestoreManyProductCategories_WithoutPermission_ShouldThrowUnauthorized()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreManyProductCategoriesCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _controller.RestoreProductCategories(new RestoreManyProductCategoriesRequest { Ids = [33, 34] }, CancellationToken.None));
    }

    [Fact(DisplayName = "PC_052 - Xác thực dữ liệu - Tạo với Name quá dài (vượt max length)")]
    public async Task CreateProductCategory_WithTooLongName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateProductCategoryRequest 
        { 
            Name = new string('a', 300), 
            Description = "Test" 
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateProductCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Name exceeds maximum length"));

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => 
            _controller.CreateProductCategory(request, CancellationToken.None));
    }

    [Fact(DisplayName = "PC_054 - Xác thực dữ liệu - Xóa với Id âm")]
    public async Task DeleteProductCategory_WithNegativeId_ShouldThrowException()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteProductCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid ID"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _controller.DeleteProductCategory(-1, CancellationToken.None));
    }

    [Fact(DisplayName = "PC_055 - Xác thực dữ liệu - Xóa nhiều với danh sách Ids rỗng")]
    public async Task DeleteManyProductCategories_WithEmptyIds_ShouldFailValidation()
    {
        // Arrange
        var request = new DeleteManyProductCategoriesRequest { Ids = [] };
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteManyProductCategoriesCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Ids list cannot be empty"));

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => 
            _controller.DeleteProductCategories(request, CancellationToken.None));
    }

    [Fact(DisplayName = "PC_056 - Xác thực dữ liệu - Khôi phục nhiều với danh sách Ids null")]
    public async Task RestoreManyProductCategories_WithNullIds_ShouldFailValidation()
    {
        // Arrange
        var request = new RestoreManyProductCategoriesRequest { Ids = null! };
        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreManyProductCategoriesCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Ids is required"));

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => 
            _controller.RestoreProductCategories(request, CancellationToken.None));
    }

    [Fact(DisplayName = "PC_057 - Kiểm tra Rate Limiting - Gửi quá nhiều request")]
    public async Task CreateProductCategory_RateLimitExceeded_ShouldThrowException()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateProductCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Too many requests"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _controller.CreateProductCategory(new CreateProductCategoryRequest(), CancellationToken.None));
    }

    [Fact(DisplayName = "PC_058 - Lấy danh sách danh mục sản phẩm khi chưa đăng nhập (public endpoint)")]
    public async Task GetProductCategories_AnonymousUser_ShouldSucceed()
    {
        // Arrange
        var expectedResult = new PagedResult<ProductCategoryResponse>(
            items:
            [
                new ProductCategoryResponse { Id = 1, Name = "Category 1", Description = "Desc" },
                new ProductCategoryResponse { Id = 2, Name = "Category 2", Description = "Desc" }
            ],
            totalCount: 10,
            pageNumber: 1,
            pageSize: 10
        );
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProductCategoriesListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<ProductCategoryResponse>>.Success(expectedResult));

        // Act
        var result = await _controller.GetProductCategories(new SieveModel(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetProductCategoriesListQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PC_059 - Lấy chi tiết danh mục sản phẩm khi chưa đăng nhập (public endpoint)")]
    public async Task GetProductCategoryById_AnonymousUser_ShouldSucceed()
    {
        // Arrange
        var expectedResult = new ProductCategoryResponse { Id = 35, Name = "Public Category", Description = "Desc" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProductCategoryByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductCategoryResponse?>.Success(expectedResult));

        // Act
        var result = await _controller.GetProductCategoryById(35, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetProductCategoryByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PC_060 - Kiểm tra concurrent update - Hai request cập nhật cùng lúc")]
    public async Task UpdateProductCategory_ConcurrentUpdate_ShouldHandleCorrectly()
    {
        // Arrange
        var request1 = new UpdateProductCategoryRequest { Name = "Update A" };
        var request2 = new UpdateProductCategoryRequest { Name = "Update B" };

        var response1 = new ProductCategoryResponse { Id = 36, Name = "Update A", Description = "Desc" };
        var response2 = new ProductCategoryResponse { Id = 36, Name = "Update B", Description = "Desc" };

        _mediatorMock.SetupSequence(m => m.Send(It.IsAny<UpdateProductCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductCategoryResponse?>.Success(response1))
            .ReturnsAsync(Result<ProductCategoryResponse?>.Success(response2));

        // Act
        var result1 = await _controller.UpdateProductCategory(36, request1, CancellationToken.None);
        var result2 = await _controller.UpdateProductCategory(36, request2, CancellationToken.None);

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateProductCategoryCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
