using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using Application.Features.Brands.Commands.CreateBrand;
using Application.Features.Brands.Commands.DeleteBrand;
using Application.Features.Brands.Commands.DeleteManyBrands;
using Application.Features.Brands.Commands.RestoreBrand;
using Application.Features.Brands.Commands.RestoreManyBrands;
using Application.Features.Brands.Commands.UpdateBrand;
using Application.Features.Brands.Queries.GetBrandById;
using Application.Features.Brands.Queries.GetBrandsList;
using Application.Features.Brands.Queries.GetDeletedBrandsList;
using Asp.Versioning;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;
using static Domain.Constants.Permission.PermissionsList;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý danh sách các thương hiệu sản phẩm (ví dụ: Honda, Yamaha, Suzuki).
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý danh sách các thương hiệu sản phẩm")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class BrandController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách thương hiệu (có phân trang, lọc, sắp xếp - vào được cho mọi người dùng).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBrandsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetBrandsListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách thương hiệu (có phân trang, lọc, sắp xếp - vào được khi có quyền xem danh sách thương hiệu).
    /// </summary>
    [HttpGet("for-manager")]
    [HasPermission(Brands.View)]
    [ProducesResponseType(typeof(PagedResult<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBrandsForManagerAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetBrandsListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách thương hiệu đã bị xoá (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet("deleted")]
    [HasPermission(Brands.Delete)]
    [ProducesResponseType(typeof(PagedResult<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedBrandsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedBrandsListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin của thương hiệu được chọn.
    /// </summary>
    [HttpGet("{id:int}", Name = RouteNames.Brands.GetById)]
    [HasPermission(Brands.View)]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBrandByIdAsync(int id, CancellationToken cancellationToken)
    {
        var query = new GetBrandByIdQuery() { Id = id };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo thương hiệu mới.
    /// </summary>
    [HttpPost]
    [HasPermission(Brands.Create)]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateBrandAsync(
        [FromBody] CreateBrandCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateBrandCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(result, RouteNames.Brands.GetById, new { id = result.IsSuccess ? result.Value.Id : 0 });
    }

    /// <summary>
    /// Cập nhật thông tin thương hiệu.
    /// </summary>
    [HttpPut("{id:int}")]
    [HasPermission(Brands.Edit)]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBrandAsync(
        int id,
        [FromBody] UpdateBrandCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateBrandCommand>() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xoá thương hiệu.
    /// </summary>
    [HttpDelete("{id:int}")]
    [HasPermission(Brands.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBrandAsync(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteBrandCommand() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục lại thương hiệu đã xoá.
    /// </summary>
    [HttpPost("restore/{id:int}")]
    [HasPermission(Brands.Delete)]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreBrandAsync(int id, CancellationToken cancellationToken)
    {
        var command = new RestoreBrandCommand() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xoá nhiều thương hiệu cùng lúc.
    /// </summary>
    [HttpDelete("delete-many")]
    [HasPermission(Brands.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteBrandsAsync(
        [FromBody] DeleteManyBrandsCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<DeleteManyBrandsCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục nhiều thương hiệu đã xoá cùng lúc.
    /// </summary>
    [HttpPost("restore-many")]
    [HasPermission(Brands.Delete)]
    [ProducesResponseType(typeof(List<BrandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreBrandsAsync(
        [FromBody] RestoreManyBrandsCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManyBrandsCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}