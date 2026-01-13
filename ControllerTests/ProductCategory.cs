using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Features.ProductCategories.Commands.CreateProductCategory;
using Application.Features.ProductCategories.Commands.DeleteManyProductCategories;
using Application.Features.ProductCategories.Commands.DeleteProductCategory;
using Application.Features.ProductCategories.Commands.RestoreManyProductCategories;
using Application.Features.ProductCategories.Commands.RestoreProductCategory;
using Application.Features.ProductCategories.Commands.UpdateProductCategory;
using Application.Features.ProductCategories.Queries.GetDeletedProductCategoriesList;
using Application.Features.ProductCategories.Queries.GetProductCategoriesList;
using Application.Features.ProductCategories.Queries.GetProductCategoryById;
using Domain.Primitives;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sieve.Models;
using WebAPI.Controllers.V1;


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
        _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
    }

#pragma warning disable CRR0035
    [Fact(DisplayName = "PC_043 - Kiểm tra phân quyền - Tạo danh mục sản phẩm không có quyền")]
    public async Task CreateProductCategory_WithoutPermission_ShouldThrowUnauthorized()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateProductCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.CreateProductCategoryAsync(new CreateProductCategoryCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PC_044 - Kiểm tra phân quyền - Xem danh sách cho manager không có quyền")]
    public async Task GetProductCategoriesForManager_WithoutPermission_ShouldThrowUnauthorized()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProductCategoriesListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetProductCategoriesForManagerAsync(new SieveModel(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PC_045 - Kiểm tra phân quyền - Xem danh sách đã xóa không có quyền")]
    public async Task GetDeletedProductCategories_WithoutPermission_ShouldThrowUnauthorized()
    {
        _mediatorMock.Setup(
            m => m.Send(It.IsAny<GetDeletedProductCategoriesListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetDeletedProductCategoriesAsync(new SieveModel(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PC_046 - Kiểm tra phân quyền - Cập nhật danh mục sản phẩm không có quyền")]
    public async Task UpdateProductCategory_WithoutPermission_ShouldThrowUnauthorized()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateProductCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.UpdateProductCategoryAsync(28, new UpdateProductCategoryCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PC_047 - Kiểm tra phân quyền - Xóa danh mục sản phẩm không có quyền")]
    public async Task DeleteProductCategory_WithoutPermission_ShouldThrowUnauthorized()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteProductCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.DeleteProductCategoryAsync(29, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PC_048 - Kiểm tra phân quyền - Khôi phục danh mục sản phẩm không có quyền")]
    public async Task RestoreProductCategory_WithoutPermission_ShouldThrowUnauthorized()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreProductCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.RestoreProductCategoryAsync(30, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PC_049 - Kiểm tra phân quyền - Xóa nhiều danh mục sản phẩm không có quyền")]
    public async Task DeleteManyProductCategories_WithoutPermission_ShouldThrowUnauthorized()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteManyProductCategoriesCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.DeleteProductCategoriesAsync(
                new DeleteManyProductCategoriesCommand { Ids = [ 31, 32 ] },
                CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PC_050 - Kiểm tra phân quyền - Khôi phục nhiều danh mục sản phẩm không có quyền")]
    public async Task RestoreManyProductCategories_WithoutPermission_ShouldThrowUnauthorized()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreManyProductCategoriesCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.RestoreProductCategoriesAsync(
                new RestoreManyProductCategoriesCommand { Ids = [ 33, 34 ] },
                CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PC_052 - Xác thực dữ liệu - Tạo với Name quá dài (vượt max length)")]
    public async Task CreateProductCategory_WithTooLongName_ShouldFailValidation()
    {
        var request = new CreateProductCategoryCommand { Name = new string('a', 300), Description = "Test" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateProductCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Name exceeds maximum length"));

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _controller.CreateProductCategoryAsync(request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PC_054 - Xác thực dữ liệu - Xóa với Id âm")]
    public async Task DeleteProductCategory_WithNegativeId_ShouldThrowException()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteProductCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid ID"));

        await Assert.ThrowsAsync<ArgumentException>(
            () => _controller.DeleteProductCategoryAsync(-1, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PC_055 - Xác thực dữ liệu - Xóa nhiều với danh sách Ids rỗng")]
    public async Task DeleteManyProductCategories_WithEmptyIds_ShouldFailValidation()
    {
        var request = new DeleteManyProductCategoriesCommand { Ids = [] };
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteManyProductCategoriesCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Ids list cannot be empty"));

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _controller.DeleteProductCategoriesAsync(request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PC_056 - Xác thực dữ liệu - Khôi phục nhiều với danh sách Ids null")]
    public async Task RestoreManyProductCategories_WithNullIds_ShouldFailValidation()
    {
        var request = new RestoreManyProductCategoriesCommand { Ids = null! };
        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreManyProductCategoriesCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Ids is required"));

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _controller.RestoreProductCategoriesAsync(request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PC_057 - Kiểm tra Rate Limiting - Gửi quá nhiều request")]
    public async Task CreateProductCategory_RateLimitExceeded_ShouldThrowException()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateProductCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Too many requests"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.CreateProductCategoryAsync(new CreateProductCategoryCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PC_058 - Lấy danh sách danh mục sản phẩm khi chưa đăng nhập (public endpoint)")]
    public async Task GetProductCategories_AnonymousUser_ShouldSucceed()
    {
        var expectedResult = new PagedResult<ProductCategoryResponse>(
            items:[ new ProductCategoryResponse { Id = 1, Name = "Category 1", Description = "Desc" }, new ProductCategoryResponse
            {
                Id = 2,
                Name = "Category 2",
                Description = "Desc"
            } ],
            totalCount: 10,
            pageNumber: 1,
            pageSize: 10);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProductCategoriesListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<ProductCategoryResponse>>.Success(expectedResult));

        var result = await _controller.GetProductCategoriesAsync(new SieveModel(), CancellationToken.None)
            .ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetProductCategoriesListQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "PC_059 - Lấy chi tiết danh mục sản phẩm khi chưa đăng nhập (public endpoint)")]
    public async Task GetProductCategoryById_AnonymousUser_ShouldSucceed()
    {
        var expectedResult = new ProductCategoryResponse { Id = 35, Name = "Public Category", Description = "Desc" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProductCategoryByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductCategoryResponse?>.Success(expectedResult));

        var result = await _controller.GetProductCategoryByIdAsync(35, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetProductCategoryByIdQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "PC_060 - Kiểm tra concurrent update - Hai request cập nhật cùng lúc")]
    public async Task UpdateProductCategory_ConcurrentUpdate_ShouldHandleCorrectly()
    {
        var request1 = new UpdateProductCategoryCommand { Name = "Update A" };
        var request2 = new UpdateProductCategoryCommand { Name = "Update B" };

        var response1 = new ProductCategoryResponse { Id = 36, Name = "Update A", Description = "Desc" };
        var response2 = new ProductCategoryResponse { Id = 36, Name = "Update B", Description = "Desc" };

        _mediatorMock.SetupSequence(
            m => m.Send(It.IsAny<UpdateProductCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductCategoryResponse?>.Success(response1))
            .ReturnsAsync(Result<ProductCategoryResponse?>.Success(response2));

        var result1 = await _controller.UpdateProductCategoryAsync(36, request1, CancellationToken.None)
            .ConfigureAwait(true);
        var result2 = await _controller.UpdateProductCategoryAsync(36, request2, CancellationToken.None)
            .ConfigureAwait(true);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<UpdateProductCategoryCommand>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
#pragma warning restore CRR0035
}
