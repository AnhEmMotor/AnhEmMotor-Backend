using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Features.Inputs.Queries.GetInputsBySupplierId;
using Application.Features.Suppliers.Commands.CreateSupplier;
using Application.Features.Suppliers.Commands.DeleteManySuppliers;
using Application.Features.Suppliers.Commands.DeleteSupplier;
using Application.Features.Suppliers.Commands.RestoreManySuppliers;
using Application.Features.Suppliers.Commands.RestoreSupplier;
using Application.Features.Suppliers.Commands.UpdateManySupplierStatus;
using Application.Features.Suppliers.Commands.UpdateSupplier;
using Application.Features.Suppliers.Commands.UpdateSupplierStatus;
using Application.Features.Suppliers.Queries.GetDeletedSuppliersList;
using Application.Features.Suppliers.Queries.GetSupplierById;
using Application.Features.Suppliers.Queries.GetSuppliersList;
using Application.Features.Suppliers.Queries.GetSuppliersListForInputManager;
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
/// Qu?n lý danh sách nhà cung c?p.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Qu?n lý danh sách nhà cung c?p")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class SupplierController(IMediator mediator) : ApiController
{
    /// <summary>
    /// L?y danh sách nhà cung c?p (có phân trang, l?c, s?p x?p).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, l?c, s?p x?p theo quy t?c c?a Sieve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [RequiresAnyPermissions(Suppliers.View, Inputs.Edit, Inputs.Create)]
    [ProducesResponseType(typeof(PagedResult<SupplierResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuppliersAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetSuppliersListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y danh sách nhà cung c?p dã b? xoá (có phân trang, l?c, s?p x?p).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, l?c, s?p x?p theo quy t?c c?a Sieve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("deleted")]
    [HasPermission(Suppliers.View)]
    [ProducesResponseType(typeof(PagedResult<SupplierResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedSuppliersAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedSuppliersListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y thông tin c?a nhà cung c?p du?c ch?n.
    /// </summary>
    /// <param name="id">Mã nhà cung c?p c?n l?y thông tin.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id:int}", Name = RouteNames.Supplier.GetById)]
    [HasPermission(Suppliers.View)]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupplierByIdAsync(int id, CancellationToken cancellationToken)
    {
        var query = new GetSupplierByIdQuery() { Id = id };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y l?ch s? nh?p hàng c?a nhà cung c?p.
    /// </summary>
    /// <param name="id">Id nhà cung c?p.</param>
    /// <param name="sieveModel">Các thông tin phân trang, l?c, s?p x?p.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id:int}/purchase-history")]
    [HasPermission(Suppliers.View)]
    [ProducesResponseType(typeof(PagedResult<SupplierPurchaseHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPurchaseHistoryAsync(
        int id,
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetSupplierPurchaseHistoryQuery() { SupplierId = id, SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// T?o nhà cung c?p m?i.
    /// </summary>
    /// <param name="request">Thông tin nhà cung c?p c?n t?o.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [HasPermission(Suppliers.Create)]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSupplierAsync(
        [FromBody] CreateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateSupplierCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(result, RouteNames.Supplier.GetById, new { id = result.IsSuccess ? result.Value?.Id : 0 });
    }

    /// <summary>
    /// L?y danh sách nhà cung c?p (có phân trang, l?c, s?p x?p - ch? du?c vào khi ngu?i dùng có quy?n thêm và s?a phi?u
    /// bán hàng).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, l?c, s?p x?p theo quy t?c c?a Sieve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("for-input")]
    [RequiresAnyPermissions(Inputs.Create, Inputs.Edit)]
    [ProducesResponseType(typeof(PagedResult<SupplierResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuppliersForInputAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetSuppliersListForInputManagerQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// C?p nh?t thông tin nhà cung c?p.
    /// </summary>
    /// <param name="id">Id nhà cung c?p c?n c?p nh?t.</param>
    /// <param name="request">Thông tin nhà cung c?p c?n c?p nh?t.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{id:int}")]
    [HasPermission(Suppliers.Edit)]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplierAsync(
        int id,
        [FromBody] UpdateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateSupplierCommand>() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// C?p nh?t tr?ng thái c?a nhà cung c?p.
    /// </summary>
    /// <param name="id">Id nhà cung c?p c?n c?p nh?t tr?ng thái.</param>
    /// <param name="request">Tr?ng thái m?i.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPatch("{id:int}/status")]
    [HasPermission(Suppliers.Edit)]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplierStatusAsync(
        int id,
        [FromBody] UpdateSupplierStatusCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateSupplierStatusCommand>() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Xoá nhà cung c?p.
    /// </summary>
    /// <param name="id">Id c?a nhà cung c?p c?n xoá.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{id:int}")]
    [HasPermission(Suppliers.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSupplierAsync(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteSupplierCommand() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi ph?c l?i nhà cung c?p dã xoá.
    /// </summary>
    /// <param name="id">Id c?a nhà cung c?p c?n khôi ph?c</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("restore/{id:int}")]
    [HasPermission(Suppliers.Delete)]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreSupplierAsync(int id, CancellationToken cancellationToken)
    {
        var command = new RestoreSupplierCommand() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xoá nhi?u nhà cung c?p cùng lúc.
    /// </summary>
    /// <param name="request">Danh sách Id nhà cung c?p c?n xoá.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("delete-many")]
    [HasPermission(Suppliers.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteSuppliersAsync(
        [FromBody] DeleteManySuppliersCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<DeleteManySuppliersCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi ph?c nhi?u nhà cung c?p dã xoá cùng lúc.
    /// </summary>
    /// <param name="request">Danh sách Id nhà cung c?p c?n khôi ph?c.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("restore-many")]
    [HasPermission(Suppliers.Delete)]
    [ProducesResponseType(typeof(List<SupplierResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreSuppliersAsync(
        [FromBody] RestoreManySuppliersCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManySuppliersCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// C?p nh?t tr?ng thái c?a nhi?u nhà cung c?p cùng lúc.
    /// </summary>
    /// <param name="request">Danh sách Id nhà cung c?p và tr?ng thái m?i.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPatch("update-status-many")]
    [HasPermission(Suppliers.Edit)]
    [ProducesResponseType(typeof(List<SupplierResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateManySupplierStatusAsync(
        [FromBody] UpdateManySupplierStatusCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateManySupplierStatusCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
