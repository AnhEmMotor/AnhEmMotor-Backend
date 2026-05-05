using Application.ApiContracts.Option.Responses;
using Application.ApiContracts.Product.Requests;
using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Features.Options.Queries.GetOptionsList;
using Application.Features.OptionValues.Commands.CreateOptionValue;
using Application.Features.OptionValues.Commands.DeleteOptionValue;
using Application.Features.OptionValues.Commands.UpdateOptionValue;
using Application.Features.PredefinedOptions.Queries.GetPredefinedOptionsList;
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
using Application.Features.Products.Queries.GetActiveVariantLiteListForInput;
using Application.Features.Products.Queries.GetActiveVariantLiteListForManager;
using Application.Features.Products.Queries.GetActiveVariantLiteListForOutput;
using Application.Features.Products.Queries.GetDeletedProductsList;
using Application.Features.Products.Queries.GetProductAttributeLabels;
using Application.Features.Products.Queries.GetProductById;
using Application.Features.Products.Queries.GetProductsList;
using Application.Features.Products.Queries.GetProductsListForManager;
using Application.Features.Products.Queries.GetProductsListForPriceManagement;
using Application.Features.Products.Queries.GetProductStoreDetailBySlug;
using Application.Features.Products.Queries.GetSitemapSlugs;
using Application.Features.Products.Queries.GetVariantCartDetailsBatch;
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
using System.Net;
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
    /// Lấy danh sách toàn bộ Slug của sản phẩm phục vụ cho việc tạo Sitemap. Chỉ cho phép gọi từ Localhost để đảm bảo
    /// bảo mật.
    /// </summary>
    [HttpGet("sitemap-slugs")]
    [ProducesResponseType(typeof(SitemapSlugsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSitemapSlugsAsync(CancellationToken cancellationToken)
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress;
        if (remoteIp != null && !IPAddress.IsLoopback(remoteIp))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
        var result = await sender.Send(new GetSitemapSlugsQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm đầy đủ dành cho khách hàng (có phân trang, lọc, tìm kiếm).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductListStoreResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsAsync(
        [FromQuery] GetProductsRequest request,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        CancellationToken cancellationToken)
    {
        var query = GetProductsListQuery.FromRequest(request, minPrice, maxPrice);
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
        var query = GetProductsListForManagerQuery
            .FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm rút gọn dành cho việc thiết lập giá (chỉ có Tên SP, Tên biến thể và Giá).
    /// </summary>
    [HttpGet("for-price-management")]
    [HasPermission(Products.View)]
    [ProducesResponseType(typeof(PagedResult<ProductPriceLiteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsForPriceManagementAsync(
        [FromQuery] SieveModel request,
        CancellationToken cancellationToken)
    {
        var query = GetProductsListForPriceManagementQuery.FromRequest(request);
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
        var query = GetActiveVariantLiteListForManagerQuery
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
        var query = GetActiveVariantLiteListForInputQuery
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
    /// Lấy danh sách các thuộc tính được định nghĩa sẵn dưới dạng từ điển key-value. Chỉ người dùng có quyền tạo hoặc
    /// chỉnh sửa sản phẩm mới có thể truy cập.
    /// </summary>
    [HttpGet("predefined-options")]
    [RequiresAnyPermissions(Products.View, Products.Create, Products.Edit, Products.Delete)]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPredefinedOptionsAsync(CancellationToken cancellationToken)
    {
        var query = new GetPredefinedOptionsListQuery();
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách ánh xạ trạng thái tồn kho (Key -> Tên tiếng Việt).
    /// </summary>
    [HttpGet("inventory-statuses")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public IActionResult GetInventoryStatuses()
    {
        var statuses = new[]
        {
            new { key = nameof(InventoryStatus.OutOfStock), label = "Hết hàng" },
            new { key = nameof(InventoryStatus.LowStock), label = "Sắp hết hàng" },
            new { key = nameof(InventoryStatus.InStock), label = "Còn hàng" }
        };
        return Ok(statuses);
    }

    /// <summary>
    /// Lấy thông tin chi tiết sản phẩm theo Id (dành cho người quản lý)
    /// </summary>
    [HttpGet("{id:int}/for-manager", Name = RouteNames.Product.GetVarientByIdForManager)]
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
            RouteNames.Product.GetVarientByIdForManager,
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

    /// <summary>
    /// Lấy chi tiết thông tin sản phẩm và biến thể dựa trên URL Slug dành cho cửa hàng.
    /// </summary>
    [HttpGet("store/{slug}")]
    [ProducesResponseType(typeof(ProductStoreDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductDetailBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductStoreDetailBySlugQuery(slug), cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách nhãn hiển thị cho các thuộc tính kỹ thuật sản phẩm dành cho cửa hàng.
    /// </summary>
    [HttpGet("store/attribute-labels")]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttributeLabelsAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductAttributeLabelsQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách thông tin của các mã biến thể được truyền vào để trả ra các thông tin phục vụ cho giỏ hàng
    /// </summary>
    [HttpPost("variants-cart-details-batch")]
    [ProducesResponseType(typeof(List<VariantCartDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVariantCartDetailsBatchAsync(
        [FromBody] List<int> variantIds,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetVariantCartDetailsBatchQuery { VariantIds = variantIds },
            cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo mới giá trị thuộc tính.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("option-values")]
    [HasPermission(Products.Create)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateOptionValueAsync(
        [FromBody] CreateOptionValueCommand request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật giá trị thuộc tính.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("option-values/{id:int}")]
    [HasPermission(Products.Edit)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateOptionValueAsync(
        int id,
        [FromBody] UpdateOptionValueCommand request,
        CancellationToken cancellationToken)
    {
        var command = request with { Id = id };
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xoá giá trị thuộc tính.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("option-values/{id:int}")]
    [HasPermission(Products.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteOptionValueAsync(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteOptionValueCommand(id), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}