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
using Infrastructure.Authorization.Attribute;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using static Domain.Constants.Permission.PermissionsList;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý danh mục sản phẩm.
/// </summary>
/// <param name="mediator"></param>
[ApiVersion("1.0")]
[SwaggerTag(" Quản lý danh mục sản phẩm")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status500InternalServerError)]
public class ProductCategoryController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách danh mục sản phẩm (có phân trang, lọc, sắp xếp - vào được cho mọi người dùng).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<ProductCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductCategories(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetProductCategoriesListQuery(sieveModel);
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách danh mục sản phẩm (có phân trang, lọc, sắp xếp - chỉ được vào khi có quyền xem danh mục sản phẩm).
    /// </summary>
    [HttpGet("for-manager")]
    [HasPermission(ProductCategories.View)]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<ProductCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductCategoriesForManager(
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
    [HasPermission(ProductCategories.View)]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<ProductCategoryResponse>), StatusCodes.Status200OK)]
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
    [HasPermission(ProductCategories.View)]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
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
    [HasPermission(ProductCategories.Create)]
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
    [HasPermission(ProductCategories.Edit)]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
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
    [HasPermission(ProductCategories.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
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
    [HasPermission(ProductCategories.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
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
    [HasPermission(ProductCategories.Delete)]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
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
    [HasPermission(ProductCategories.Delete)]
    [ProducesResponseType(typeof(List<ProductCategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
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
