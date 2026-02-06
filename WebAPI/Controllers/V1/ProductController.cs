using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.DeleteManyProducts;
using Application.Features.Products.Commands.DeleteProduct;
using Application.Features.Products.Commands.RestoreManyProducts;
using Application.Features.Products.Commands.RestoreProduct;
using Application.Features.Products.Commands.UpdateManyProductPrices;
using Application.Features.Products.Commands.UpdateManyProductStatuses;
using Application.Features.Products.Commands.UpdateManyVariantPrices;
using Application.Features.Products.Commands.UpdateProduct;
using Application.Features.Products.Commands.UpdateProductPrice;
using Application.Features.Products.Commands.UpdateProductStatus;
using Application.Features.Products.Commands.UpdateVariantPrice;
using Application.Features.Products.Queries.CheckSlugAvailability;
using Application.Features.Products.Queries.GetActiveVariantLiteListForOutput;
using Application.Features.Products.Queries.GetDeletedProductsList;
using Application.Features.Products.Queries.GetProductById;
using Application.Features.Products.Queries.GetProductsList;
using Application.Features.Products.Queries.GetVariantLiteByProductId;
using Asp.Versioning;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;
using static Domain.Constants.Permission.PermissionsList;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý sản phẩm.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý sản phẩm")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class ProductController(ISender sender) : ApiController
{
    /// <summary>
    /// Lấy danh sách sản phẩm đầy đủ dành cho khách hàng (có phân trang, lọc, tìm kiếm).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsAsync(
        [FromQuery] SieveModel request,
        CancellationToken cancellationToken)
    {
        var query = GetProductsListQuery.FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm đầy đủ dành cho người quản lý (có phân trang, lọc, tìm kiếm).
    /// </summary>
    [HttpGet("for-manager")]
    [HasPermission(Products.View)]
    [ProducesResponseType(typeof(PagedResult<ProductDetailForManagerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsForManagerAsync(
        [FromQuery] SieveModel request,
        CancellationToken cancellationToken)
    {
        var query = Application.Features.Products.Queries.GetProductsListForManager.GetProductsListForManagerQuery
            .FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm đã bị xoá (có phân trang, lọc, tìm kiếm).
    /// </summary>
    [HttpGet("deleted")]
    [HasPermission(Products.View)]
    [ProducesResponseType(typeof(PagedResult<ProductDetailForManagerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedProductsAsync(
        [FromQuery] SieveModel request,
        CancellationToken cancellationToken)
    {
        var query = GetDeletedProductsListQuery.FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách biến thể sản phẩm của tất cả sản phẩm (có phân trang, lọc, tìm kiếm - chỉ có thể vào khi và chỉ
    /// khi có quyền xem danh sách sản phẩm).
    /// </summary>
    [HttpGet("variants-lite/for-manager")]
    [RequiresAnyPermissions(Products.View)]
    [ProducesResponseType(typeof(PagedResult<ProductVariantLiteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveVariantLiteProductsAsync(
        [FromQuery] SieveModel request,
        CancellationToken cancellationToken = default)
    {
        var query = Application.Features.Products.Queries.GetActiveVariantLiteListForManager.GetActiveVariantLiteListForManagerQuery
            .FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách biến thể sản phẩm của tất cả sản phẩm (có phân trang, lọc, tìm kiếm - cho phép vào với mọi user).
    /// </summary>
    [HttpGet("variants-lite")]
    [ProducesResponseType(typeof(PagedResult<ProductVariantLiteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveVariantLiteProductsPublicAsync(
        [FromQuery] SieveModel request,
        CancellationToken cancellationToken = default)
    {
        var query = Application.Features.Products.Queries.GetActiveVariantLiteListForManager.GetActiveVariantLiteListForManagerQuery
            .FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// `Lấy danh sách biến thể sản phẩm của tất cả sản phẩm (có phân trang, lọc, tìm kiếm - chỉ có thể vào khi và chỉ
    /// khi có quyền thêm hoặc sửa phiếu nhập).
    /// </summary>
    [HttpGet("variants-lite/for-input")]
    [RequiresAnyPermissions(Inputs.Edit, Inputs.Create)]
    [ProducesResponseType(typeof(PagedResult<ProductVariantLiteResponseForInput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveVariantLiteProductsForInputAsync(
        [FromQuery] SieveModel request,
        CancellationToken cancellationToken = default)
    {
        var query = Application.Features.Products.Queries.GetActiveVariantLiteListForInput.GetActiveVariantLiteListForInputQuery
            .FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách biến thể sản phẩm của tất cả sản phẩm (có phân trang, lọc, tìm kiếm - chỉ có thể vào khi và chỉ
    /// khi có quyền thêm hoặc sửa phiếu bán hàng).
    /// </summary>
    [HttpGet("variants-lite/for-output")]
    [RequiresAnyPermissions(Outputs.Edit, Outputs.Create)]
    [ProducesResponseType(typeof(PagedResult<ProductVariantLiteResponseForOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveVariantLiteProductsForOutputAsync(
        [FromQuery] SieveModel request,
        CancellationToken cancellationToken = default)
    {
        var query = GetActiveVariantLiteListForOutputQuery.FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết sản phẩm theo Id (dành cho toàn bộ người dùng khách)
    /// </summary>
    [HttpGet("{id:int}", Name = RouteNames.Product.GetVarientById)]
    [ProducesResponseType(typeof(ProductDetailForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVarientByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetProductByIdQuery() { Id = id }, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết sản phẩm theo Id (dành cho người quản lý)
    /// </summary>
    [HttpGet("{id:int}/for-manager")]
    [HasPermission(Products.View)]
    [ProducesResponseType(typeof(ProductDetailForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVarientByIdForManagerAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetProductByIdQuery() { Id = id }, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách biến thể theo Id sản phẩm.
    /// </summary>
    [HttpGet("{id:int}/variants-lite")]
    [HasPermission(Products.View)]
    [ProducesResponseType(typeof(List<ProductVariantLiteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVariantLiteByProductIdActiveAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetVariantLiteByProductIdQuery() { IncludeDeleted = false, ProductId = id },
            cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Kiểm tra slug có sẵn sàng sử dụng hay không.
    /// </summary>
    [HttpGet("check-slug")]
    [RequiresAnyPermissions(Products.Create, Products.Edit)]
    [ProducesResponseType(typeof(SlugAvailabilityResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckSlugAvailabilityAsync(
        [FromQuery] string slug,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CheckSlugAvailabilityQuery() { Slug = slug }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo mới sản phẩm với các biến thể.
    /// </summary>
    [HttpPost]
    [HasPermission(Products.Create)]
    [ProducesResponseType(typeof(ProductDetailForManagerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProductAsync(
        [FromBody] CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateProductCommand>();
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(
            result,
            RouteNames.Product.GetVarientById,
            new { id = result.IsSuccess ? result.Value?.Id : 0 });
    }

    /// <summary>
    /// Cập nhật sản phẩm theo Id.
    /// </summary>
    [HttpPut("{id:int}")]
    [HasPermission(Products.Edit)]
    [ProducesResponseType(typeof(ProductDetailForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProductAsync(
        int id,
        [FromBody] UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var command = request with { Id = id };
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xoá sản phẩm (soft delete) và cascade xoá ảnh của các biến thể.
    /// </summary>
    [HttpDelete("{id:int}")]
    [HasPermission(Products.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductAsync(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteProductCommand() { Id = id }, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xoá nhiều sản phẩm cùng lúc (soft delete) và cascade xoá ảnh.
    /// </summary>
    [HttpDelete("delete-many")]
    [HasPermission(Products.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProductsAsync(
        [FromBody] DeleteManyProductsCommand request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteManyProductsCommand() { Ids = request.Ids }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục sản phẩm đã bị xoá.
    /// </summary>
    [HttpPost("restore/{id:int}")]
    [HasPermission(Products.Delete)]
    [ProducesResponseType(typeof(ProductDetailForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreProductAsync(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RestoreProductCommand() { Id = id }, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục nhiều sản phẩm cùng lúc.
    /// </summary>
    [HttpPost("restore-many")]
    [HasPermission(Products.Delete)]
    [ProducesResponseType(typeof(List<ProductDetailForManagerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreProductsAsync(
        [FromBody] RestoreManyProductsCommand request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RestoreManyProductsCommand() { Ids = request.Ids }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Chỉnh giá cho tất cả các biến thể của 1 sản phẩm.
    /// </summary>
    [HttpPatch("{id:int}/price")]
    [HasPermission(Products.EditPrice)]
    [ProducesResponseType(typeof(ProductDetailForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductPriceAsync(
        int id,
        [FromBody] UpdateProductPriceCommand request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateProductPriceCommand() { Id = id, Price = request.Price },
            cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Chỉnh giá cho nhiều sản phẩm cùng lúc.
    /// </summary>
    [HttpPatch("prices")]
    [HasPermission(Products.EditPrice)]
    [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateManyProductPricesAsync(
        [FromBody] UpdateManyProductPricesCommand request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateManyProductPricesCommand() { Ids = request.Ids, Price = request.Price },
            cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Chỉnh giá cho 1 biến thể sản phẩm.
    /// </summary>
    [HasPermission(Products.EditPrice)]
    [HttpPatch("variant/{variantId:int}/price")]
    [ProducesResponseType(typeof(ProductVariantLiteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVariantPriceAsync(
        int variantId,
        [FromBody] UpdateVariantPriceCommand request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateVariantPriceCommand() { Price = request.Price, VariantId = variantId },
            cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Chỉnh giá cho nhiều biến thể sản phẩm cùng lúc.
    /// </summary>
    [HttpPatch("variant/prices")]
    [HasPermission(Products.EditPrice)]
    [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateManyVariantPricesAsync(
        [FromBody] UpdateManyVariantPricesCommand request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateManyVariantPricesCommand() { Ids = request.Ids, Price = request.Price },
            cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Chỉnh trạng thái của 1 sản phẩm.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [HasPermission(Products.ChangeStatus)]
    [ProducesResponseType(typeof(ProductDetailForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductStatusAsync(
        int id,
        [FromBody] UpdateProductStatusCommand request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateProductStatusCommand() { Id = id, StatusId = request.StatusId },
            cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Chỉnh trạng thái nhiều sản phẩm cùng lúc.
    /// </summary>
    [HttpPatch("statuses")]
    [HasPermission(Products.ChangeStatus)]
    [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateManyProductStatusesAsync(
        [FromBody] UpdateManyProductStatusesCommand request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateManyProductStatusesCommand() { Ids = request.Ids, StatusId = request.StatusId },
            cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }
}