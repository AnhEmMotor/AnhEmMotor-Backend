using Application.ApiContracts.Product.Common;
using Application.ApiContracts.Product.Create;
using Application.ApiContracts.Product.Delete;
using Application.ApiContracts.Product.Update;
using Application.Interfaces.Services.Product;
using Asp.Versioning;
using Domain.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý sản phẩm.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class ProductController(
    IProductSelectService selectService,
    IProductInsertService insertService,
    IProductUpdateService updateService,
    IProductDeleteService deleteService) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách sản phẩm đầy đủ (có phân trang, lọc, tìm kiếm).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Application.ApiContracts.Product.Select.ProductDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] Application.ApiContracts.Product.Select.ProductListRequest request, CancellationToken cancellationToken)
    {
        var result = await selectService.GetProductsAsync(request, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm đã bị xoá (có phân trang, lọc, tìm kiếm).
    /// </summary>
    [HttpGet("deleted")]
    [ProducesResponseType(typeof(PagedResult<Application.ApiContracts.Product.Select.ProductDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedProducts([FromQuery] Application.ApiContracts.Product.Select.ProductListRequest request, CancellationToken cancellationToken)
    {
        var result = await selectService.GetDeletedProductsAsync(request, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách biến thể sản phẩm của tất cả sản phẩm chưa xoá.
    /// </summary>
    [HttpGet("variants-lite/active")]
    [ProducesResponseType(typeof(PagedResult<ProductVariantLiteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveVariantLiteProducts([FromQuery] Application.ApiContracts.Product.Select.ProductListRequest request, CancellationToken cancellationToken = default)
    {
        var result = await selectService.GetActiveVariantLiteProductsAsync(request, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách biến thể sản phẩm của tất cả sản phẩm đã xoá.
    /// </summary>
    [HttpGet("variants-lite/deleted")]
    [ProducesResponseType(typeof(PagedResult<ProductVariantLiteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedVariantLiteProducts([FromQuery] Application.ApiContracts.Product.Select.ProductListRequest request, CancellationToken cancellationToken = default)
    {
        var result = await selectService.GetDeletedVariantLiteProductsAsync(request, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết sản phẩm theo Id.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Application.ApiContracts.Product.Select.ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(int id, [FromQuery] bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var (data, error) = await selectService.GetProductDetailsByIdAsync(id, includeDeleted, cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Lấy danh sách biến thể chưa xoá theo Id sản phẩm.
    /// </summary>
    [HttpGet("{id:int}/variants-lite/active")]
    [ProducesResponseType(typeof(List<ProductVariantLiteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVariantLiteByProductIdActive(int id, CancellationToken cancellationToken = default)
    {
        var (variants, error) = await selectService.GetVariantLiteByProductIdAsync(id, includeDeleted: false, cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return NotFound(error);
        }

        return Ok(variants);
    }

    /// <summary>
    /// Lấy danh sách biến thể đã xoá theo Id sản phẩm.
    /// </summary>
    [HttpGet("{id:int}/variants-lite/deleted")]
    [ProducesResponseType(typeof(List<ProductVariantLiteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVariantLiteByProductIdDeleted(int id, CancellationToken cancellationToken = default)
    {
        var (variants, error) = await selectService.GetVariantLiteByProductIdAsync(id, includeDeleted: true, cancellationToken).ConfigureAwait(true);
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
        var result = await selectService.CheckSlugAvailabilityAsync(slug, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Tạo mới sản phẩm với các biến thể.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Application.ApiContracts.Product.Select.ProductDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var (data, error) = await insertService.CreateProductAsync(request, cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return BadRequest(error);
        }

        return CreatedAtAction(nameof(GetProductById), new { id = data!.Id }, data);
    }

    /// <summary>
    /// Cập nhật sản phẩm theo Id.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Application.ApiContracts.Product.Select.ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var (data, error) = await updateService.UpdateProductAsync(id, request, cancellationToken).ConfigureAwait(true);
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
        var error = await deleteService.DeleteProductAsync(id, cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return NotFound(error);
        }

        return NoContent();
    }

    /// <summary>
    /// Xoá nhiều sản phẩm cùng lúc (soft delete) và cascade xoá ảnh.
    /// </summary>
    [HttpPost("delete-many")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProducts([FromBody] DeleteManyProductsRequest request, CancellationToken cancellationToken)
    {
        var error = await deleteService.DeleteProductsAsync(request, cancellationToken).ConfigureAwait(true);
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
    [ProducesResponseType(typeof(Application.ApiContracts.Product.Select.ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreProduct(int id, CancellationToken cancellationToken)
    {
        var (data, error) = await updateService.RestoreProductAsync(id, cancellationToken).ConfigureAwait(true);
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
        var (data, error) = await updateService.RestoreProductsAsync(request, cancellationToken).ConfigureAwait(true);
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
    [ProducesResponseType(typeof(Application.ApiContracts.Product.Select.ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductPrice(int id, [FromBody] UpdateProductPriceRequest request, CancellationToken cancellationToken)
    {
        var (data, error) = await updateService.UpdateProductPriceAsync(id, request, cancellationToken).ConfigureAwait(true);
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
        var (data, error) = await updateService.UpdateManyProductPricesAsync(request, cancellationToken).ConfigureAwait(true);
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
    [ProducesResponseType(typeof(Application.ApiContracts.Product.Select.ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductStatus(int id, [FromBody] UpdateProductStatusRequest request, CancellationToken cancellationToken)
    {
        var (data, error) = await updateService.UpdateProductStatusAsync(id, request, cancellationToken).ConfigureAwait(true);
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
        var (data, error) = await updateService.UpdateManyProductStatusesAsync(request, cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }
}