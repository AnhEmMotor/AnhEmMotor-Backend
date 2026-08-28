using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Features.WorkshopPayments.Queries;
using Application.Features.WorkshopPayments.Queries.GetWorkshopPaymentStatistics;
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
/// Quản lý phiếu thu xưởng (Workshop Payment) — danh sách, thống kê và chi tiết các khoản thu từ xưởng sửa chữa.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý phiếu thu xưởng (Workshop Payment)")]
[Route("api/v{version:apiVersion}/WorkshopPayments")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class WorkshopPaymentsController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách phiếu thu xưởng với phân trang, lọc và sắp xếp.
    /// </summary>
    /// <param name="sieveModel">Tham số phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="sourceType">Lọc theo loại nguồn phiếu thu. Bỏ trống để lấy tất cả.</param>
    /// <param name="paymentStatus">Lọc theo trạng thái thanh toán. Bỏ trống để lấy tất cả.</param>
    /// <param name="paymentMethod">Lọc theo phương thức thanh toán. Bỏ trống để lấy tất cả.</param>
    /// <param name="search">Từ khóa tìm kiếm tự do. Bỏ trống để không lọc.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách phiếu thu xưởng đã phân trang.</returns>
    /// <response code="200">Trả về danh sách phiếu thu xưởng thành công.</response>
    [HttpGet]
    [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
    [ProducesResponseType(typeof(PagedResult<WorkshopPaymentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListAsync(
        [FromQuery] SieveModel sieveModel,
        [FromQuery] string? sourceType,
        [FromQuery] string? paymentStatus,
        [FromQuery] string? paymentMethod,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var query = new GetWorkshopPaymentsListQuery
        {
            Sieve = sieveModel,
            SourceType = sourceType,
            PaymentStatus = paymentStatus,
            PaymentMethod = paymentMethod,
            Search = search
        };
        var result = await mediator.Send(query, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thống kê tổng hợp về phiếu thu xưởng (tổng số phiếu, tổng doanh thu, v.v.).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Dữ liệu thống kê phiếu thu xưởng.</returns>
    /// <response code="200">Trả về thống kê thành công.</response>
    [HttpGet("stats")]
    [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
    [ProducesResponseType(typeof(WorkshopPaymentStatisticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        var query = new GetWorkshopPaymentStatisticsQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một phiếu thu xưởng theo ID.
    /// </summary>
    /// <param name="id">ID của phiếu thu xưởng cần xem chi tiết.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin chi tiết phiếu thu xưởng.</returns>
    /// <response code="200">Trả về chi tiết phiếu thu xưởng thành công.</response>
    /// <response code="404">Không tìm thấy phiếu thu xưởng với ID đã cho.</response>
    [HttpGet("{id:int}")]
    [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
    [ProducesResponseType(typeof(WorkshopPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetailAsync(int id, CancellationToken cancellationToken)
    {
        var query = new GetWorkshopPaymentDetailQuery { Id = id };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo mới phiếu thu xưởng.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Factory.RepairOrderManagement.Create)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] Application.Features.WorkshopPayments.Commands.CreateWorkshopPaymentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật phiếu thu xưởng.
    /// </summary>
    [HttpPut("{id:int}")]
    [HasPermission(Permissions.Factory.RepairOrderManagement.Create)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAsync(
        int id,
        [FromBody] Application.Features.WorkshopPayments.Commands.UpdateWorkshopPaymentCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new ErrorResponse("ID không khớp."));
        }
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
