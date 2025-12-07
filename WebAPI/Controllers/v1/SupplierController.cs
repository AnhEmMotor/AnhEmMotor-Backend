using Application.ApiContracts.Supplier.Requests;
using Application.ApiContracts.Supplier.Responses;
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
using Asp.Versioning;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý danh sách nhà cung cấp.
/// </summary>
/// <param name="mediator"></param>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý danh sách nhà cung cấp")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status500InternalServerError)]
public class SupplierController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách nhà cung cấp (có phân trang, lọc, sắp xếp).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<SupplierResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuppliers(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetSuppliersListQuery(sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Lấy danh sách nhà cung cấp đã bị xoá (có phân trang, lọc, sắp xếp).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("deleted")]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<SupplierResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedSuppliers(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedSuppliersListQuery(sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Lấy thông tin của nhà cung cấp được chọn.
    /// </summary>
    /// <param name="id">Mã nhà cung cấp cần lấy thông tin.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupplierById(int id, CancellationToken cancellationToken)
    {
        var query = new GetSupplierByIdQuery(id);
        var (data, error) = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Tạo nhà cung cấp mới.
    /// </summary>
    /// <param name="request">Thông tin nhà cung cấp cần tạo.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSupplier(
        [FromBody] CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateSupplierCommand>();
        var response = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Cập nhật thông tin nhà cung cấp.
    /// </summary>
    /// <param name="id">Id nhà cung cấp cần cập nhật.</param>
    /// <param name="request">Thông tin nhà cung cấp cần cập nhật.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplier(
        int id,
        [FromBody] UpdateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateSupplierCommand>() with { Id = id };
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);

        if(error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Cập nhật trạng thái của nhà cung cấp.
    /// </summary>
    /// <param name="id">Id nhà cung cấp cần cập nhật trạng thái.</param>
    /// <param name="request">Trạng thái mới.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplierStatus(
        int id,
        [FromBody] UpdateSupplierStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateSupplierStatusCommand>() with { Id = id };

        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if(error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Xoá nhà cung cấp.
    /// </summary>
    /// <param name="id">Id của nhà cung cấp cần xoá.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSupplier(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteSupplierCommand() with { Id = id };
        var error = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return NoContent();
    }

    /// <summary>
    /// Khôi phục lại nhà cung cấp đã xoá.
    /// </summary>
    /// <param name="id">Id của nhà cung cấp cần khôi phục</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("restore/{id:int}")]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreSupplier(int id, CancellationToken cancellationToken)
    {
        var command = new RestoreSupplierCommand() with { Id = id };
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);

        if(error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Xoá nhiều nhà cung cấp cùng lúc.
    /// </summary>
    /// <param name="request">Danh sách Id nhà cung cấp cần xoá.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("delete-many")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteSuppliers(
        [FromBody] DeleteManySuppliersRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<DeleteManySuppliersCommand>();
        var error = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if(error != null)
        {
            return BadRequest(error);
        }

        return NoContent();
    }

    /// <summary>
    /// Khôi phục nhiều nhà cung cấp đã xoá cùng lúc.
    /// </summary>
    /// <param name="request">Danh sách Id nhà cung cấp cần khôi phục.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("restore-many")]
    [ProducesResponseType(typeof(List<SupplierResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreSuppliers(
        [FromBody] RestoreManySuppliersRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManySuppliersCommand>();
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if(error != null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Cập nhật trạng thái của nhiều nhà cung cấp cùng lúc.
    /// </summary>
    /// <param name="request">Danh sách Id nhà cung cấp và trạng thái mới.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPatch("update-status-many")]
    [ProducesResponseType(typeof(List<SupplierResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateManySupplierStatus(
        [FromBody] UpdateManySupplierStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateManySupplierStatusCommand>();
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if(error != null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }
}
