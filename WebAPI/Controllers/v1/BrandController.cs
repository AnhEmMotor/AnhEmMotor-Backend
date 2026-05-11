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
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;
using Domain.Constants.Permission.Permissions;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Qu?n lý danh sách các thuong hi?u s?n ph?m (ví d?: Honda, Yamaha, Suzuki).
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Qu?n lý danh sách các thuong hi?u s?n ph?m")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class BrandController(IMediator mediator) : ApiController
{
    /// <summary>
    /// L?y danh sách thuong hi?u (có phân trang, l?c, s?p x?p - vào du?c cho m?i ngu?i dùng).
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
    /// L?y danh sách thuong hi?u (có phân trang, l?c, s?p x?p - vào du?c khi có quy?n xem danh sách thuong hi?u).
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
    /// L?y danh sách thuong hi?u dã b? xoá (có phân trang, l?c, s?p x?p).
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
    /// L?y thông tin c?a thuong hi?u du?c ch?n.
    /// </summary>
    [HttpGet("{id:int}", Name = Domain.Constants.RouteNames.Brands.GetById)]
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
    /// T?o thuong hi?u m?i.
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
        return HandleCreated(result, Domain.Constants.RouteNames.Brands.GetById, new { id = result.IsSuccess ? result.Value.Id : 0 });
    }

    /// <summary>
    /// C?p nh?t thông tin thuong hi?u.
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
    /// Xoá thuong hi?u.
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
    /// Khôi ph?c l?i thuong hi?u dã xoá.
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
    /// Xoá nhi?u thuong hi?u cùng lúc.
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
    /// Khôi ph?c nhi?u thuong hi?u dã xoá cùng lúc.
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


