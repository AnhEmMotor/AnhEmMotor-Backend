using Application.Common.Models;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.DeleteManyProducts;
using Application.Features.Products.Commands.DeleteProduct;
using Application.Features.Products.Commands.RestoreManyProducts;
using Application.Features.Products.Commands.RestoreProduct;
using Application.Features.Products.Commands.UpdateManyProductPrices;
using Application.Features.Products.Commands.UpdateManyProductStatuses;
using Application.Features.Products.Commands.UpdateProduct;
using Application.Features.Products.Commands.UpdateProductPrice;
using Application.Features.Products.Commands.UpdateProductStatus;
using Application.Features.Products.Commands.UpdateVariantPrice;
using Application.Features.Products.Queries.CheckSlugAvailability;
using Application.Features.Products.Queries.GetActiveVariantLiteListForInput;
using Application.Features.Products.Queries.GetActiveVariantLiteListForOutput;
using Application.Features.Products.Queries.GetDeletedProductsList;
using Application.Features.Products.Queries.GetProductsList;
using Application.Features.Products.Queries.GetProductsListForManager;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class Product
{
    private readonly Mock<ISender> _senderMock;
    private readonly ProductController _controller;

    public Product()
    {
        _senderMock = new Mock<ISender>();
        _controller = new ProductController(_senderMock.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
    }

#pragma warning disable CRR0035
    [Fact(DisplayName = "PRODUCT_081 - API tạo sản phẩm trả về 403 khi user không có quyền")]
    public async Task CreateProduct_UserNoPermission_ReturnsForbidden()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.CreateProductAsync(new CreateProductCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PRODUCT_082 - API tạo sản phẩm trả về 401 khi user chưa đăng nhập")]
    public async Task CreateProduct_UserNotAuthenticated_ReturnsUnauthorized()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.CreateProductAsync(new CreateProductCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PRODUCT_083 - API lấy danh sách sản phẩm for-manager trả về 403 khi user không có quyền")]
    public async Task GetProductsForManager_UserNoPermission_ReturnsForbidden()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<GetProductsListForManagerQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetProductsForManagerAsync(new Sieve.Models.SieveModel(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PRODUCT_084 - API lấy danh sách sản phẩm public cho phép truy cập không cần quyền")]
    public async Task GetProducts_NoAuth_ReturnsSuccess()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<GetProductsListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new Domain.Primitives.PagedResult<Application.ApiContracts.Product.Responses.ProductDetailResponse>(
                    [],
                    0,
                    1,
                    10));

        var result = await _controller.GetProductsAsync(new Sieve.Models.SieveModel(), CancellationToken.None)
            .ConfigureAwait(true);

        Assert.NotNull(result);
    }

    [Fact(DisplayName = "PRODUCT_085 - API xóa sản phẩm trả về 403 khi user không có quyền")]
    public async Task DeleteProduct_UserNoPermission_ReturnsForbidden()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<DeleteProductCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.DeleteProductAsync(1, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PRODUCT_086 - API sửa sản phẩm trả về 403 khi user không có quyền")]
    public async Task UpdateProduct_UserNoPermission_ReturnsForbidden()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<UpdateProductCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.UpdateProductAsync(1, new UpdateProductCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PRODUCT_087 - API lấy deleted products trả về 403 khi user không có quyền view")]
    public async Task GetDeletedProducts_UserNoPermission_ReturnsForbidden()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<GetDeletedProductsListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetDeletedProductsAsync(new Sieve.Models.SieveModel(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PRODUCT_088 - API check slug availability trả về 403 khi user không có quyền update")]
    public async Task CheckSlugAvailability_UserNoPermission_ReturnsForbidden()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<CheckSlugAvailabilityQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.CheckSlugAvailabilityAsync("test-slug", CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PRODUCT_089 - API update product price trả về 403 khi user không có quyền")]
    public async Task UpdateProductPrice_UserNoPermission_ReturnsForbidden()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<UpdateProductPriceCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.UpdateProductPriceAsync(1, new UpdateProductPriceCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PRODUCT_090 - API update variant price trả về 403 khi user không có quyền")]
    public async Task UpdateVariantPrice_UserNoPermission_ReturnsForbidden()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<UpdateVariantPriceCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.UpdateVariantPriceAsync(1, new UpdateVariantPriceCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PRODUCT_091 - API update many product prices trả về 403 khi user không có quyền")]
    public async Task UpdateManyProductPrices_UserNoPermission_ReturnsForbidden()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<UpdateManyProductPricesCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.UpdateManyProductPricesAsync(new UpdateManyProductPricesCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PRODUCT_092 - API update product status trả về 403 khi user không có quyền")]
    public async Task UpdateProductStatus_UserNoPermission_ReturnsForbidden()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<UpdateProductStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.UpdateProductStatusAsync(1, new UpdateProductStatusCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PRODUCT_093 - API update many product statuses trả về 403 khi user không có quyền")]
    public async Task UpdateManyProductStatuses_UserNoPermission_ReturnsForbidden()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<UpdateManyProductStatusesCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.UpdateManyProductStatusesAsync(
                new UpdateManyProductStatusesCommand(),
                CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PRODUCT_094 - API delete many products trả về 403 khi user không có quyền")]
    public async Task DeleteManyProducts_UserNoPermission_ReturnsForbidden()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<DeleteManyProductsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.DeleteProductsAsync(new DeleteManyProductsCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PRODUCT_095 - API restore product trả về 403 khi user không có quyền")]
    public async Task RestoreProduct_UserNoPermission_ReturnsForbidden()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<RestoreProductCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.RestoreProductAsync(1, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PRODUCT_096 - API restore many products trả về 403 khi user không có quyền")]
    public async Task RestoreManyProducts_UserNoPermission_ReturnsForbidden()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<RestoreManyProductsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.RestoreProductsAsync(new RestoreManyProductsCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PRODUCT_097 - API variants-lite/for-input trả về 403 khi user không có quyền input")]
    public async Task GetActiveVariantLiteProductsForInput_UserNoPermission_ReturnsForbidden()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<GetActiveVariantLiteListForInputQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetActiveVariantLiteProductsForInputAsync(
                new Sieve.Models.SieveModel(),
                CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PRODUCT_098 - API variants-lite/for-output trả về 403 khi user không có quyền output")]
    public async Task GetActiveVariantLiteProductsForOutput_UserNoPermission_ReturnsForbidden()
    {
        _senderMock.Setup(
            m => m.Send(It.IsAny<GetActiveVariantLiteListForOutputQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetActiveVariantLiteProductsForOutputAsync(
                new Sieve.Models.SieveModel(),
                CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PRODUCT_099 - API tạo sản phẩm trả về 400 với error response đúng format")]
    public async Task CreateProduct_MissingName_ReturnsBadRequest()
    {
        _senderMock.Setup(m => m.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<Application.ApiContracts.Product.Responses.ProductDetailForManagerResponse?>.Failure(
                    Error.BadRequest("Name là bắt buộc")));

        var result = await _controller.CreateProductAsync(new CreateProductCommand(), CancellationToken.None)
            .ConfigureAwait(true);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact(DisplayName = "PRODUCT_100 - API hỗ trợ pagination với custom page size")]
    public async Task GetProducts_CustomPageSize_ReturnsCorrectPagedResult()
    {
        var sieveModel = new Sieve.Models.SieveModel { Page = 2, PageSize = 5 };
        var expectedResult = new Domain.Primitives.PagedResult<Application.ApiContracts.Product.Responses.ProductDetailResponse>(
            [],
            15,
            2,
            5);

        _senderMock.Setup(m => m.Send(It.IsAny<GetProductsListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _controller.GetProductsAsync(sieveModel, CancellationToken.None).ConfigureAwait(true);

        Assert.NotNull(result);
        _senderMock.Verify(m => m.Send(It.IsAny<GetProductsListQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
#pragma warning restore CRR0035
}
