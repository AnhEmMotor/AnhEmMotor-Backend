using Application.ApiContracts.BookingAppointments.Responses;
using Application.Common.Models;
using Application.Features.BookingAppointments.Commands;
using Application.Features.BookingAppointments.Queries;
using Application.Interfaces.Services;
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
/// Quản lý lịch hẹn sửa chữa / bảo hành — phân trang, tạo, xác nhận, hủy, cập nhật, xóa.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý lịch hẹn sửa chữa / bảo hành")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class BookingAppointmentsController(ISender sender, ICurrentUserContext currentUserContext) : ApiController
{
    /// <summary>
    /// Lấy danh sách lịch hẹn phân trang (hỗ trợ lọc, sắp xếp theo Sieve).
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Factory.BookingAppointmentManagement.View)]
    [ProducesResponseType(typeof(PagedResult<BookingAppointmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var query = new GetBookingAppointmentsQuery { Sieve = sieveModel };
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy chi tiết một lịch hẹn theo ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [HasPermission(Permissions.Factory.BookingAppointmentManagement.View)]
    [ProducesResponseType(typeof(BookingAppointmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(int id, CancellationToken cancellationToken)
    {
        var query = new GetBookingAppointmentDetailQuery { Id = id };
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? Ok(result.Value) : HandleResult(result);
    }

    /// <summary>
    /// Tạo lịch hẹn sửa chữa / bảo hành mới.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Factory.BookingAppointmentManagement.Create)]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBookingAppointmentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetDetail), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    /// <summary>
    /// Cập nhật thông tin lịch hẹn.
    /// </summary>
    [HttpPut("{id:int}")]
    [HasPermission(Permissions.Factory.BookingAppointmentManagement.Edit)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateBookingAppointmentCommand command,
        CancellationToken cancellationToken)
    {
        var cmd = command with { Id = id };
        var result = await sender.Send(cmd, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? Ok(result.Value) : HandleResult(result);
    }

    /// <summary>
    /// Xóa (soft-delete) lịch hẹn.
    /// </summary>
    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.Factory.BookingAppointmentManagement.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteBookingAppointmentCommand(id), cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? NoContent() : HandleResult(result);
    }

    /// <summary>
    /// Xác nhận lịch hẹn — khách chốt chắc chắn sẽ đến.
    /// </summary>
    [HttpPost("{id:int}/confirm")]
    [HasPermission(Permissions.Factory.BookingAppointmentManagement.Confirm)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(
        int id,
        [FromBody] ConfirmBookingAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = currentUserContext.GetUserId();
        var command = new ConfirmBookingAppointmentCommand(id, request.AppointmentAt, currentUserId);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? Ok(result.Value) : HandleResult(result);
    }

    /// <summary>
    /// Hủy lịch hẹn (không đến / no-show).
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [HasPermission(Permissions.Factory.BookingAppointmentManagement.Cancel)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(
        int id,
        [FromBody] CancelBookingAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CancelBookingAppointmentCommand(id, request.CancelReason);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? Ok(result.Value) : HandleResult(result);
    }
}

public class ConfirmBookingAppointmentRequest
{
    public DateTimeOffset? AppointmentAt { get; set; }
}

public class CancelBookingAppointmentRequest
{
    public string? CancelReason { get; set; }
}
