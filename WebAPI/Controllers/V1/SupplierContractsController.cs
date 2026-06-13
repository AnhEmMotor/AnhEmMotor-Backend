using Application.ApiContracts.SupplierContracts.Requests;
using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using Application.Features.SupplierContracts.Commands.CreateSupplierContract;
using Application.Features.SupplierContracts.Commands.DeleteSupplierContract;
using Application.Features.SupplierContracts.Commands.RestoreSupplierContract;
using Application.Features.SupplierContracts.Commands.UpdateSupplierContract;
using Application.Features.SupplierContracts.Queries.GetDeletedSupplierContractsList;
using Application.Features.SupplierContracts.Queries.GetSupplierContractAuditLogs;
using Application.Features.SupplierContracts.Queries.GetSupplierContractById;
using Application.Features.SupplierContracts.Queries.GetSupplierContractsList;
using Application.Features.SupplierContracts.Queries.GetSupplierContractStatistics;
using Application.Features.Suppliers.Queries.GetSuppliersList;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Domain.Constants.RouteNames;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý hợp đồng nhà cung cấp.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý hợp đồng nhà cung cấp")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class SupplierContractsController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách hợp đồng nhà cung cấp (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet]
    [RequiresAnyPermissions(Suppliers.View, SupplierContracts.View)]
    [ProducesResponseType(typeof(PagedResult<SupplierContractResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupplierContractsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetSupplierContractsListQuery { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách hợp đồng nhà cung cấp đã bị xóa.
    /// </summary>
    [HttpGet("deleted")]
    [HasPermission(SupplierContracts.View)]
    [ProducesResponseType(typeof(PagedResult<SupplierContractResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedSupplierContractsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedSupplierContractsListQuery { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một hợp đồng nhà cung cấp.
    /// </summary>
    [HttpGet("{id:guid}", Name = SupplierContract.GetById)]
    [HasPermission(SupplierContracts.View)]
    [ProducesResponseType(typeof(SupplierContractDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupplierContractByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetSupplierContractByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy nhật ký audit log của hợp đồng nhà cung cấp.
    /// </summary>
    [HttpGet("{id:guid}/audit-logs")]
    [HasPermission(SupplierContracts.View)]
    [ProducesResponseType(typeof(List<SupplierContractAuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupplierContractAuditLogsAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetSupplierContractAuditLogsQuery { SupplierContractId = id };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo hợp đồng nhà cung cấp mới.
    /// </summary>
    [HttpPost]
    [HasPermission(SupplierContracts.Create)]
    [ProducesResponseType(typeof(SupplierContractResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSupplierContractAsync(
        [FromBody] CreateSupplierContractCommand request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken).ConfigureAwait(true);
        return HandleCreated(
            result,
            SupplierContract.GetById,
            new { id = result.IsSuccess ? result.Value?.Id : Guid.Empty });
    }

    /// <summary>
    /// Cập nhật hợp đồng nhà cung cấp.
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission(SupplierContracts.Edit)]
    [ProducesResponseType(typeof(SupplierContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplierContractAsync(
        Guid id,
        [FromBody] UpdateSupplierContractRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSupplierContractCommand(id, request);
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa mềm hợp đồng nhà cung cấp.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(SupplierContracts.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSupplierContractAsync(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteSupplierContractCommand(id);
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục hợp đồng nhà cung cấp đã xóa.
    /// </summary>
    [HttpPost("restore/{id:guid}")]
    [HasPermission(SupplierContracts.Delete)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreSupplierContractAsync(Guid id, CancellationToken cancellationToken)
    {
        var command = new RestoreSupplierContractCommand(id);
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thống kê hợp đồng nhà cung cấp.
    /// </summary>
    [HttpGet("statistics")]
    [RequiresAnyPermissions(Suppliers.View, SupplierContracts.View)]
    [ProducesResponseType(typeof(SupplierContractStatisticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupplierContractStatisticsAsync(CancellationToken cancellationToken)
    {
        var query = new GetSupplierContractStatisticsQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách nhà cung cấp cho dropdown chọn.
    /// </summary>
    [HttpGet("suppliers-for-select")]
    [HasPermission(SupplierContracts.View)]
    [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuppliersForSelectAsync(CancellationToken cancellationToken)
    {
        var query = new GetSuppliersListQuery { SieveModel = new SieveModel { PageSize = 1000, Page = 1 } };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        if (!result.IsSuccess || result.Value?.Items == null)
            return Ok(new List<object>());
        var selectList = result.Value.Items.Select(s => new { id = s.Id, name = s.Name }).Cast<object>().ToList();
        return Ok(selectList);
    }
}
