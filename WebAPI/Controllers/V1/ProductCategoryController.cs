using Application.ApiContracts.ProductCategory.Requests;
using Application.ApiContracts.ProductCategory.Responses;
using Application.Features.ProductCategories.Commands.CreateProductCategory;
using Application.Features.ProductCategories.Commands.DeleteManyProductCategories;
using Application.Features.ProductCategories.Commands.DeleteProductCategory;
using Application.Features.ProductCategories.Commands.RestoreManyProductCategories;
using Application.Features.ProductCategories.Commands.RestoreProductCategory;
using Application.Features.ProductCategories.Commands.UpdateProductCategory;
using Application.Features.ProductCategories.Queries.GetDeletedProductCategoriesList;
using Application.Features.ProductCategories.Queries.GetProductCategoriesList;
using Application.Features.ProductCategories.Queries.GetProductCategoryById;
using Asp.Versioning;
using Domain.Helpers;
using Domain.Shared;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý danh mục sản phẩm.
/// </summary>
/// <param name="mediator"></param>
[ApiVersion("1.0")]
[SwaggerTag(" Quản lý danh mục sản phẩm")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class ProductCategoryController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách danh mục sản phẩm (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductCategories(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetProductCategoriesListQuery(sieveModel);
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách danh mục sản phẩm đã bị xoá (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet("deleted")]
    [ProducesResponseType(typeof(PagedResult<ProductCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedProductCategories(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedProductCategoriesListQuery(sieveModel);
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
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
        var query = new GetProductCategoryByIdQuery(id);
        var (data, error) = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Tạo mới danh mục sản phẩm.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateProductCategory(
        [FromBody] CreateProductCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateProductCategoryCommand>();
        var response = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Cập nhật danh mục sản phẩm.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductCategory(
        int id,
        [FromBody] UpdateProductCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateProductCategoryCommand>() with { Id = id };
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
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
        var command = new DeleteProductCategoryCommand() with { Id = id };
        var error = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }

        return NoContent();
    }

    /// <summary>
    /// Xoá nhiều danh mục sản phẩm cùng lúc.
    /// </summary>
    [HttpDelete("delete-many")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProductCategories(
        [FromBody] DeleteManyProductCategoriesRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<DeleteManyProductCategoriesCommand>();
        var error = await mediator.Send(command, cancellationToken).ConfigureAwait(true);

        if(error != null)
        {
            return BadRequest(error);
        }

        return NoContent();
    }

    /// <summary>
    /// Khôi phục 1 danh mục sản phẩm đã bị xoá.
    /// </summary>
    [HttpPatch("restore/{id:int}")]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreProductCategory(int id, CancellationToken cancellationToken)
    {
        var command = new RestoreProductCategoryCommand() with { Id = id };
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);

        if(error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Khôi phục nhiều danh mục sản phẩm cùng lúc.
    /// </summary>
    [HttpPost("restore-many")]
    [ProducesResponseType(typeof(List<ProductCategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreProductCategories(
        [FromBody] RestoreManyProductCategoriesRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManyProductCategoriesCommand>();
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);

        if(error != null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }
}
