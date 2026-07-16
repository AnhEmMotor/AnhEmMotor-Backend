using Application.ApiContracts.Logistics.CarrierSettings.Requests;
using Application.ApiContracts.Logistics.CarrierSettings.Responses;
using Application.ApiContracts.Logistics.Responses;
using Application.Common.Models;
using Application.Features.Logistics.Commands.TestCarrierConnection;
using Application.Features.Logistics.Commands.ToggleItemPickCommand;
using Application.Features.Logistics.Commands.UpdateCarrierPartner;
using Application.Features.Logistics.Commands.UpdateParcelStatusCommand;
using Application.Features.Logistics.Commands.UpdateTrackingNumberCommand;
using Application.Features.Logistics.Queries.CalculateShippingFee;
using Application.Features.Logistics.Queries.GetActiveShipments;
using Application.Features.Logistics.Queries.GetCarriers;
using Application.Features.Logistics.Queries.GetDeliveryStatuses;
using Application.Features.Logistics.Queries.GetFulfillmentDetail;
using Application.Features.Logistics.Queries.GetFulfillmentOrders;
using Application.Features.Logistics.Queries.GetLogisticsDashboard;
using Application.Features.Logistics.Queries.GetShipmentTracking;
using Application.Features.Logistics.Returns;
using Application.Features.Logistics.Returns.Commands.InspectReturn;
using Application.Features.Logistics.Returns.Commands.RejectReturn;
using Application.Features.Logistics.Returns.Queries.GetReturnDetail;
using Application.Features.Logistics.Returns.Queries.GetReturns;
using Asp.Versioning;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý kho vận — vận chuyển, đối tác giao hàng, đơn fulfillment, đổi trả.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/logistics")]
public class LogisticsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lấy dữ liệu tổng quan cho bảng điều khiển kho vận (dashboard).
    /// </summary>
    /// <param name="range">Khoảng thời gian: "today", "week", "month". Mặc định "today".</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpGet("dashboard")]
    public Task<LogisticsDashboardResponse> GetDashboard(
        [FromQuery] string range = "today",
        CancellationToken cancellationToken = default)
    {
        var query = new GetLogisticsDashboardQuery { Range = range ?? "today" };
        return mediator.Send(query, cancellationToken);
    }

    /// <summary>
    /// Lấy danh sách tất cả đối tác vận chuyển (carrier partners).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpGet("carriers")]
    public async Task<ActionResult<Result<CarrierPartnerResponse>>> GetCarriers(
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetCarriersQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật thông tin đối tác vận chuyển (tên, trạng thái, cấu hình API).
    /// </summary>
    /// <param name="id">ID của đối tác vận chuyển.</param>
    /// <param name="request">Thông tin cập nhật.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpPut("carriers/{id}")]
    public async Task<IActionResult> UpdateCarrierPartner(
        int id,
        [FromBody] UpdateCarrierPartnerRequest request,
        CancellationToken cancellationToken = default)
    {
        request ??= new UpdateCarrierPartnerRequest();
        var command = new UpdateCarrierPartnerCommand { Id = id, Request = request };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        if (!result)
            return NotFound(new { message = "Không tìm thấy đối tác vận chuyển" });
        return NoContent();
    }

    /// <summary>
    /// Kiểm tra kết nối API với đối tác vận chuyển (test credentials).
    /// </summary>
    /// <param name="id">ID của đối tác vận chuyển.</param>
    /// <param name="request">Thông tin test kết nối.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpPost("carriers/{id}/test-connection")]
    public Task<TestCarrierConnectionResponse> TestCarrierConnection(
        int id,
        [FromBody] TestCarrierConnectionRequest request,
        CancellationToken cancellationToken = default)
    {
        request ??= new TestCarrierConnectionRequest();
        var command = new TestCarrierConnectionCommand { Id = id, Request = request };
        return mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Tra cứu thông tin vận chuyển theo mã vận đơn, mã đơn hàng hoặc số ĐT khách hàng.
    /// </summary>
    /// <param name="search">Mã vận đơn, mã đơn hàng hoặc SĐT khách hàng.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpGet("tracking/{search}")]
    public async Task<IActionResult> GetTracking(string search, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(search))
            return BadRequest(new { message = "Vui lòng nhập mã vận đơn, mã đơn hàng hoặc số điện thoại khách hàng" });
        var query = new GetShipmentTrackingQuery { TrackingNumberOrPhone = search.Trim() };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        if (result == null)
            return NotFound(new { message = "Không tìm thấy thông tin vận chuyển" });
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách đơn vận chuyển đang active (chưa giao thành công).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpGet("active-shipments")]
    public async Task<IActionResult> GetActiveShipments(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetActiveShipmentsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách các trạng thái giao hàng hợp lệ.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpGet("delivery-statuses")]
    public async Task<IActionResult> GetDeliveryStatuses(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetDeliveryStatusesQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách đơn fulfillment (đơn hàng cần chuẩn bị và giao).
    /// </summary>
    /// <param name="status">Lọc theo trạng thái giao hàng.</param>
    /// <param name="carrier">Lọc theo đối tác vận chuyển.</param>
    /// <param name="fromDate">Lọc từ ngày.</param>
    /// <param name="toDate">Lọc đến ngày.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpGet("fulfillment")]
    public async Task<ActionResult<List<FulfillmentOrderResponse>>> GetFulfillmentOrders(
        [FromQuery] ParcelDeliveryStatus? status,
        [FromQuery] string? carrier,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFulfillmentOrdersQuery
        {
            Status = status,
            Carrier = carrier,
            FromDate = fromDate,
            ToDate = toDate
        };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết một đơn fulfillment theo ID.
    /// </summary>
    /// <param name="id">ID của đơn fulfillment.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpGet("fulfillment/{id}")]
    public async Task<IActionResult> GetFulfillmentDetail(int id, CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(new GetFulfillmentDetailQuery { Id = id }, cancellationToken)
        .ConfigureAwait(false);
        if (response == null)
            return NotFound();
        return Ok(response);
    }

    /// <summary>
    /// Cập nhật trạng thái giao hàng của đơn fulfillment.
    /// </summary>
    /// <param name="id">ID của đơn fulfillment.</param>
    /// <param name="command">Trạng thái giao hàng mới.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpPut("fulfillment/{id}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateParcelStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        if (!result)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Cập nhật mã vận đơn (tracking number) cho đơn fulfillment.
    /// </summary>
    /// <param name="id">ID của đơn fulfillment.</param>
    /// <param name="command">Mã vận đơn mới.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpPut("fulfillment/{id}/tracking")]
    public async Task<IActionResult> UpdateTracking(
        int id,
        [FromBody] UpdateTrackingNumberCommand command,
        CancellationToken cancellationToken = default)
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        if (!result)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Bật/tắt trạng thái đã lấy hàng cho một item trong đơn fulfillment.
    /// </summary>
    /// <param name="itemId">ID của item trong đơn.</param>
    /// <param name="command">Thông tin toggle pick.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpPut("fulfillment/items/{itemId}/toggle-pick")]
    public async Task<IActionResult> ToggleItemPick(
        int itemId,
        [FromBody] ToggleItemPickCommand command,
        CancellationToken cancellationToken = default)
    {
        command.ItemId = itemId;
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        if (!result)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Lấy danh sách yêu cầu đổi trả hàng (Returns).
    /// </summary>
    /// <param name="status">Lọc theo trạng thái đổi trả (tùy chọn).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpGet("returns")]
    public async Task<IActionResult> GetReturns(
        [FromQuery] string? status,
        CancellationToken cancellationToken = default)
    {
        ReturnOrderStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status) &&
        Enum.TryParse<ReturnOrderStatus>(status, ignoreCase: true, out var statusValue))
        {
            parsedStatus = statusValue;
        }
        var result = await mediator.Send(new GetReturnsQuery { Status = parsedStatus }, cancellationToken)
        .ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết một yêu cầu đổi trả hàng theo ID.
    /// </summary>
    /// <param name="id">ID của yêu cầu đổi trả.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpGet("returns/{id}")]
    public async Task<IActionResult> GetReturnDetail(int id, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetReturnDetailQuery { Id = id }, cancellationToken)
        .ConfigureAwait(false);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Xác nhận kiểm tra hàng đổi trả (chuyển sang trạng thái đã kiểm tra).
    /// </summary>
    /// <param name="id">ID của yêu cầu đổi trả.</param>
    /// <param name="command">Kết quả kiểm tra (pass/fail, ghi chú).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpPost("returns/{id}/inspect")]
    public async Task<IActionResult> InspectReturn(
        int id,
        [FromBody] InspectReturnCommand command,
        CancellationToken cancellationToken = default)
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        if (!result)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Từ chối yêu cầu đổi trả hàng.
    /// </summary>
    /// <param name="id">ID của yêu cầu đổi trả.</param>
    /// <param name="command">Lý do từ chối.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpPost("returns/{id}/reject")]
    public async Task<IActionResult> RejectReturn(
        int id,
        [FromBody] RejectReturnCommand command,
        CancellationToken cancellationToken = default)
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        if (!result)
            return NotFound();
        return NoContent();
    }

    [HttpPost("calculate-fee")]
    public async Task<IActionResult> CalculateFee(
        [FromBody] CalculateShippingFeeQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return BadRequest(result.Errors);
        return Ok(result.Value);
    }
}

