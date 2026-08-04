using Application.ApiContracts.SalesContracts.Requests;
using Application.ApiContracts.SalesContracts.Responses;
using Application.Common.Models;
using Application.Features.SalesContracts.Commands.CreateSalesContract;
using Application.Features.SalesContracts.Commands.DeleteSalesContract;
using Application.Features.SalesContracts.Commands.UpdateSalesContract;
using Application.Features.SalesContracts.Commands.UpdateSalesContractStatus;
using Application.Features.SalesContracts.Commands.UploadSalesContractScan;
using Application.Features.SalesContracts.Queries.GetSalesContractById;
using Application.Features.SalesContracts.Queries.GetSalesContractsList;
using Application.Features.SalesContracts.Queries.GetSalesContractStatistics;
using Asp.Versioning;
using Domain.Constants.Permission;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý hợp đồng bán hàng.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý hợp đồng bán hàng")]
[Route("api/v{version:apiVersion}/contracts/sales")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class SalesContractsController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách hợp đồng bán hàng (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Order.ContractManagement.View)]
    [ProducesResponseType(typeof(PagedResult<SalesContractResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSalesContractsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetSalesContractsListQuery { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thống kê hợp đồng bán hàng (số lượng theo trạng thái).
    /// </summary>
    [HttpGet("statistics")]
    [HasPermission(Permissions.Order.ContractManagement.View)]
    [ProducesResponseType(typeof(SalesContractStatisticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        var query = new GetSalesContractStatisticsQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy chi tiết một hợp đồng bán hàng.
    /// </summary>
    [HttpGet("{id:guid}", Name = nameof(GetSalesContractByIdAsync))]
    [HasPermission(Permissions.Order.ContractManagement.View)]
    [ProducesResponseType(typeof(SalesContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSalesContractByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetSalesContractByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo hợp đồng bán hàng mới từ một đơn hàng.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Order.ContractManagement.Create)]
    [ProducesResponseType(typeof(SalesContractResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSalesContractAsync(
        [FromBody] CreateSalesContractRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSalesContractCommand(request);
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleCreated(
            result,
            nameof(GetSalesContractByIdAsync),
            new { id = result.IsSuccess ? result.Value?.Id : Guid.Empty });
    }

    /// <summary>
    /// Cập nhật hợp đồng bán hàng (điều khoản, bảo hành, ghi chú).
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Order.ContractManagement.Edit)]
    [ProducesResponseType(typeof(SalesContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSalesContractAsync(
        Guid id,
        [FromBody] UpdateSalesContractRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSalesContractCommand(id, request);
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật trạng thái hợp đồng bán hàng.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [HasPermission(Permissions.Order.ContractManagement.Edit)]
    [ProducesResponseType(typeof(SalesContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSalesContractStatusAsync(
        Guid id,
        [FromBody] UpdateContractStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSalesContractStatusCommand(id, request.Status);
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Gửi bản nháp hợp đồng sang Admin để duyệt.
    /// </summary>
    [HttpPost("{id:guid}/submit-for-approval")]
    [HasPermission(Permissions.Order.ContractManagement.Edit)]
    [ProducesResponseType(typeof(SalesContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitSalesContractForApprovalAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSalesContractStatusCommand(id, Domain.Constants.SalesContractStatus.PendingApproval);
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Admin duyệt hợp đồng đã được nhân viên gửi.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [HasPermission(Permissions.Admin.ContractManagement.Edit)]
    [ProducesResponseType(typeof(SalesContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveSalesContractAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSalesContractStatusCommand(
            id,
            Domain.Constants.SalesContractStatus.Approved,
            IsAdminApproval: true);
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/scanned-file")]
    [HasPermission(Permissions.Order.ContractManagement.Edit)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadSalesContractScanAsync(
        Guid id,
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "File hợp đồng trống hoặc không hợp lệ." });
        await using var stream = file.OpenReadStream();
        var result = await mediator.Send(
            new UploadSalesContractScanCommand(id, stream, file.FileName),
            cancellationToken)
            .ConfigureAwait(false);
        return result.IsSuccess ? Ok(new { scannedFileUrl = result.Value }) : HandleResult(result);
    }

    /// <summary>
    /// Xóa hợp đồng bán hàng (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Order.ContractManagement.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSalesContractAsync(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteSalesContractCommand(id);
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
