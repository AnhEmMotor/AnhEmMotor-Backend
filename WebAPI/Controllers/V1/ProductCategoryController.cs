using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Services.ProductCategory;
using Asp.Versioning;
using Domain.Helpers;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý danh mục sản phẩm.
/// </summary>
/// <param name="insertService"></param>
/// <param name="selectService"></param>
/// <param name="updateService"></param>
/// <param name="deleteService"></param>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class ProductCategoryController(
    IProductCategoryInsertService insertService,
    IProductCategorySelectService selectService,
    IProductCategoryUpdateService updateService,
    IProductCategoryDeleteService deleteService) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách danh mục sản phẩm (có phân trang, lọc, sắp xếp). Nếu không truyền tham số phân trang sẽ trả về tất cả.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductCategories([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var result = await selectService.GetProductCategoriesAsync(sieveModel, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách danh mục sản phẩm đã bị xoá (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet("deleted")]
    [ProducesResponseType(typeof(PagedResult<ProductCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedProductCategories([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var result = await selectService.GetDeletedProductCategoriesAsync(sieveModel, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin danh mục sản phẩm theo Id.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductCategoryById(int id, CancellationToken cancellationToken)
    {
        var (data, error) = await selectService.GetByIdAsync(id, cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Tạo mới danh mục sản phẩm.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateProductCategory([FromBody] CreateProductCategoryRequest request, CancellationToken cancellationToken)
    {
        var response = await insertService.CreateAsync(request, cancellationToken).ConfigureAwait(true);
        return CreatedAtAction(nameof(GetProductCategoryById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Cập nhật danh mục sản phẩm.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductCategory(int id, [FromBody] UpdateProductCategoryRequest request, CancellationToken cancellationToken)
    {
        var (data, error) = await updateService.UpdateAsync(id, request, cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Xoá danh mục sản phẩm (soft delete).
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductCategory(int id, CancellationToken cancellationToken)
    {
        var error = await deleteService.DeleteAsync(id, cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return NotFound(error);
        }

        return NoContent();
    }

    /// <summary>
    /// Xoá nhiều danh mục sản phẩm cùng lúc.
    /// </summary>
    [HttpPost("delete-many")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProductCategories([FromBody] DeleteManyProductCategoriesRequest request, CancellationToken cancellationToken)
    {
        var error = await deleteService.DeleteManyAsync(request, cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return BadRequest(error);
        }

        return NoContent();
    }

    /// <summary>
    /// Khôi phục danh mục sản phẩm đã bị xoá.
    /// </summary>
    [HttpPost("restore/{id:int}")]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreProductCategory(int id, CancellationToken cancellationToken)
    {
        var (data, error) = await updateService.RestoreAsync(id, cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Khôi phục nhiều danh mục sản phẩm cùng lúc.
    /// </summary>
    [HttpPost("restore-many")]
    [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreProductCategories([FromBody] RestoreManyProductCategoriesRequest request, CancellationToken cancellationToken)
    {
        var (data, error) = await updateService.RestoreManyAsync(request, cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }
}