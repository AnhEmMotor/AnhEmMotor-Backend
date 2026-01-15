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
using Asp.Versioning;
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
/// Quản lý danh mục sản phẩm.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag(" Quản lý danh mục sản phẩm")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class ProductCategoryController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách danh mục sản phẩm (có phân trang, lọc, sắp xếp - vào được cho mọi người dùng).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductCategoriesAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetProductCategoriesListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách danh mục sản phẩm (có phân trang, lọc, sắp xếp - chỉ được vào khi có quyền xem danh mục sản phẩm).
    /// </summary>
    [HttpGet("for-manager")]
    [HasPermission(ProductCategories.View)]
    [ProducesResponseType(typeof(PagedResult<ProductCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductCategoriesForManagerAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetProductCategoriesListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách danh mục sản phẩm đã bị xoá (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet("deleted")]
    [HasPermission(ProductCategories.View)]
    [ProducesResponseType(typeof(PagedResult<ProductCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedProductCategoriesAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedProductCategoriesListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin danh mục sản phẩm theo Id.
    /// </summary>
    [HttpGet("{id:int}")]
    [HasPermission(ProductCategories.View)]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductCategoryByIdAsync(int id, CancellationToken cancellationToken)
    {
        var query = new GetProductCategoryByIdQuery() { Id = id };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo mới danh mục sản phẩm.
    /// </summary>
    [HttpPost]
    [HasPermission(ProductCategories.Create)]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateProductCategoryAsync(
        [FromBody] CreateProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateProductCategoryCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(result, nameof(GetProductCategoryByIdAsync), new { id = result.IsSuccess ? result.Value.Id : null });
    }

    /// <summary>
    /// Cập nhật danh mục sản phẩm.
    /// </summary>
    [HttpPut("{id:int}")]
    [HasPermission(ProductCategories.Edit)]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductCategoryAsync(
        int id,
        [FromBody] UpdateProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateProductCategoryCommand>() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xoá danh mục sản phẩm (soft delete).
    /// </summary>
    [HttpDelete("{id:int}")]
    [HasPermission(ProductCategories.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductCategoryAsync(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteProductCategoryCommand() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xoá nhiều danh mục sản phẩm cùng lúc.
    /// </summary>
    [HttpDelete("delete-many")]
    [HasPermission(ProductCategories.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProductCategoriesAsync(
        [FromBody] DeleteManyProductCategoriesCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<DeleteManyProductCategoriesCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục 1 danh mục sản phẩm đã bị xoá.
    /// </summary>
    [HttpPatch("restore/{id:int}")]
    [HasPermission(ProductCategories.Delete)]
    [ProducesResponseType(typeof(ProductCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreProductCategoryAsync(int id, CancellationToken cancellationToken)
    {
        var command = new RestoreProductCategoryCommand() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục nhiều danh mục sản phẩm cùng lúc.
    /// </summary>
    [HttpPost("restore-many")]
    [HasPermission(ProductCategories.Delete)]
    [ProducesResponseType(typeof(List<ProductCategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreProductCategoriesAsync(
        [FromBody] RestoreManyProductCategoriesCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManyProductCategoriesCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}
