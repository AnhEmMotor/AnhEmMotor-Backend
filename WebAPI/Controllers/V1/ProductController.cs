using Application.ApiContracts.Product.Common;
using Application.ApiContracts.Product.Create;
using Application.ApiContracts.Product.Delete;
using Application.ApiContracts.Product.Select;
using Application.ApiContracts.Product.Update;
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
using Application.Features.Products.Queries.GetActiveVariantLiteList;
using Application.Features.Products.Queries.GetDeletedProductsList;
using Application.Features.Products.Queries.GetDeletedVariantLiteList;
using Application.Features.Products.Queries.GetProductById;
using Application.Features.Products.Queries.GetProductsList;
using Application.Features.Products.Queries.GetVariantLiteByProductId;
using Asp.Versioning;
using Domain.Helpers;
using Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý sản phẩm.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class ProductController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách sản phẩm đầy đủ (có phân trang, lọc, tìm kiếm).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] SieveModel request, CancellationToken cancellationToken)
    {
        var query = GetProductsListQuery.FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm đã bị xoá (có phân trang, lọc, tìm kiếm).
    /// </summary>
    [HttpGet("deleted")]
    [ProducesResponseType(typeof(PagedResult<ProductDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedProducts([FromQuery] SieveModel request, CancellationToken cancellationToken)
    {
        var query = GetDeletedProductsListQuery.FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách biến thể sản phẩm của tất cả sản phẩm (chưa xoá - có phân trang, lọc, tìm kiếm).
    /// </summary>
    [HttpGet("variants-lite")]
    [ProducesResponseType(typeof(PagedResult<ProductVariantLiteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveVariantLiteProducts([FromQuery] SieveModel request, CancellationToken cancellationToken = default)
    {
        var query = GetActiveVariantLiteListQuery.FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết sản phẩm theo Id.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVarientById(int id, CancellationToken cancellationToken = default)
    {
        var (data, error) = await sender.Send(new GetProductByIdQuery(id, false), cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return NotFound(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Lấy danh sách biến thể theo Id sản phẩm.
    /// </summary>
    [HttpGet("{id:int}/variants-lite")]
    [ProducesResponseType(typeof(List<ProductVariantLiteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVariantLiteByProductIdActive(int id, CancellationToken cancellationToken = default)
    {
        var (variants, error) = await sender.Send(new GetVariantLiteByProductIdQuery(id, IncludeDeleted: false), cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return NotFound(error);
        }

        return Ok(variants);
    }

    /// <summary>
    /// Kiểm tra slug có sẵn sàng sử dụng hay không.
    /// </summary>
    [HttpGet("check-slug")]
    [ProducesResponseType(typeof(SlugAvailabilityResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckSlugAvailability([FromQuery] string slug, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CheckSlugAvailabilityQuery(slug), cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Tạo mới sản phẩm với các biến thể.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var command = CreateProductCommand.FromRequest(request);
        var (data, error) = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return BadRequest(error);
        }
        return StatusCode(StatusCodes.Status201Created, data);
    }

    /// <summary>
    /// Cập nhật sản phẩm theo Id.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(new UpdateProductCommand(id, request), cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Xoá sản phẩm (soft delete) và cascade xoá ảnh của các biến thể.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        var error = await sender.Send(new DeleteProductCommand(id), cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return NotFound(error);
        }

        return NoContent();
    }

    /// <summary>
    /// Xoá nhiều sản phẩm cùng lúc (soft delete) và cascade xoá ảnh.
    /// </summary>
    [HttpDelete("delete-many")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProducts([FromBody] DeleteManyProductsRequest request, CancellationToken cancellationToken)
    {
        var error = await sender.Send(new DeleteManyProductsCommand(request.Ids!), cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return BadRequest(error);
        }

        return NoContent();
    }

    /// <summary>
    /// Khôi phục sản phẩm đã bị xoá.
    /// </summary>
    [HttpPost("restore/{id:int}")]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreProduct(int id, CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(new RestoreProductCommand(id), cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Khôi phục nhiều sản phẩm cùng lúc.
    /// </summary>
    [HttpPost("restore-many")]
    [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreProducts([FromBody] RestoreManyProductsRequest request, CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(new RestoreManyProductsCommand(request.Ids!), cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Chỉnh giá cho tất cả các biến thể của 1 sản phẩm.
    /// </summary>
    [HttpPatch("{id:int}/price")]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductPrice(int id, [FromBody] UpdateProductPriceRequest request, CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(new UpdateProductPriceCommand(id, request.Price ?? 0), cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Chỉnh giá cho nhiều sản phẩm cùng lúc.
    /// </summary>
    [HttpPatch("prices")]
    [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateManyProductPrices([FromBody] UpdateManyProductPricesRequest request, CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(new UpdateManyProductPricesCommand(request.Ids!, request.Price), cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Chỉnh giá cho 1 biến thể sản phẩm.
    /// </summary>
    [HttpPatch("variant/{variantId:int}/price")]
    [ProducesResponseType(typeof(ProductVariantLiteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVariantPrice(int variantId, [FromBody] UpdateVariantPriceRequest request, CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(new UpdateVariantPriceCommand(variantId, request.Price ?? 0), cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Chỉnh giá cho nhiều biến thể sản phẩm cùng lúc.
    /// </summary>
    [HttpPatch("variant/prices")]
    [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateManyVariantPrices([FromBody] UpdateManyVariantPricesRequest request, CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(new UpdateManyVariantPricesCommand(request.Ids!, request.Price), cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Chỉnh trạng thái của 1 sản phẩm.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductStatus(int id, [FromBody] UpdateProductStatusRequest request, CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(new UpdateProductStatusCommand(id, request.StatusId!), cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Chỉnh trạng thái nhiều sản phẩm cùng lúc.
    /// </summary>
    [HttpPatch("statuses")]
    [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateManyProductStatuses([FromBody] UpdateManyProductStatusesRequest request, CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(new UpdateManyProductStatusesCommand(request.Ids!, request.StatusId!), cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }
}