using System;
using System.Threading.Tasks;
using Application.ApiContracts.Logistics.Responses;
using Application.ApiContracts.Logistics.CarrierSettings.Requests;
using Application.ApiContracts.Logistics.CarrierSettings.Responses;
using Application.Common.Models;
using Application.Features.Logistics.Queries.GetActiveShipments;
using Application.Features.Logistics.Commands.TestCarrierConnection;
using Application.Features.Logistics.Commands.UpdateCarrierPartner;
using Application.Features.Logistics.Queries.GetCarriers;
using Application.Features.Logistics.Queries.GetLogisticsDashboard;
using Application.Features.Logistics.Queries.GetShipmentTracking;
using Application.Features.Logistics.Returns;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Application.Features.Logistics.Commands.UpdateTrackingNumberCommand;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/logistics")]
public class LogisticsController(IMediator mediator) : ControllerBase
{
    [HttpGet("dashboard")]
    public Task<LogisticsDashboardResponse> GetDashboard([FromQuery] string range = "today", CancellationToken cancellationToken = default)
    {
        var query = new GetLogisticsDashboardQuery { Range = range ?? "today" };
        return mediator.Send(query, cancellationToken);
    }

    [HttpGet("carriers")]
    public async Task<ActionResult<Result<CarrierPartnerResponse>>> GetCarriers(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetCarriersQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPut("carriers/{id}")]
    public async Task<IActionResult> UpdateCarrierPartner(int id, [FromBody] UpdateCarrierPartnerRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new UpdateCarrierPartnerRequest();
        var command = new UpdateCarrierPartnerCommand { Id = id, Request = request };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        if (!result) return NotFound(new { message = "Không tìm thấy đối tác vận chuyển" });
        return NoContent();
    }

    [HttpPost("carriers/{id}/test-connection")]
    public Task<TestCarrierConnectionResponse> TestCarrierConnection(int id, [FromBody] TestCarrierConnectionRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new TestCarrierConnectionRequest();
        var command = new TestCarrierConnectionCommand { Id = id, Request = request };
        return mediator.Send(command, cancellationToken);
    }

    [HttpGet("tracking/{search}")]
    public async Task<IActionResult> GetTracking(string search, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(search)) return BadRequest(new { message = "Vui lòng nhập mã vận đơn, mã đơn hàng hoặc số điện thoại khách hàng" });
        var query = new GetShipmentTrackingQuery { TrackingNumberOrPhone = search.Trim() };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        if (result == null) return NotFound(new { message = "Không tìm thấy thông tin vận chuyển" });
        return Ok(result);
    }

    [HttpGet("active-shipments")]
    public async Task<IActionResult> GetActiveShipments(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetActiveShipmentsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("fulfillment/{id}")]
    public async Task<IActionResult> GetFulfillmentDetail(int id, CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(new Application.Features.Logistics.Queries.GetFulfillmentDetail.GetFulfillmentDetailQuery { Id = id }, cancellationToken).ConfigureAwait(false);
        if (response == null) return NotFound();
        return Ok(response);
    }

    [HttpPut("fulfillment/{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] Application.Features.Logistics.Commands.ProcessFulfillment.UpdateParcelStatusCommand command, CancellationToken cancellationToken = default)
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPut("fulfillment/{id}/tracking")]
    public async Task<IActionResult> UpdateTracking(int id, [FromBody] UpdateTrackingNumberCommand command, CancellationToken cancellationToken = default)
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPut("fulfillment/items/{itemId}/toggle-pick")]
    public async Task<IActionResult> ToggleItemPick(int itemId, [FromBody] Application.Features.Logistics.Commands.ProcessFulfillment.ToggleItemPickCommand command, CancellationToken cancellationToken = default)
    {
        command.ItemId = itemId;
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("returns")]
    public async Task<IActionResult> GetReturns([FromQuery] string? status, CancellationToken cancellationToken = default)
    {
        ReturnOrderStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<ReturnOrderStatus>(status, ignoreCase: true, out var statusValue))
        {
            parsedStatus = statusValue;
        }

        var result = await mediator.Send(new Application.Features.Logistics.Returns.Queries.GetReturns.GetReturnsQuery { Status = parsedStatus }, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("returns/{id}")]
    public async Task<IActionResult> GetReturnDetail(int id, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new Application.Features.Logistics.Returns.Queries.GetReturnDetail.GetReturnDetailQuery { Id = id }, cancellationToken).ConfigureAwait(false);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("returns/{id}/inspect")]
    public async Task<IActionResult> InspectReturn(int id, [FromBody] Application.Features.Logistics.Returns.Commands.InspectReturn.InspectReturnCommand command, CancellationToken cancellationToken = default)
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        if (!result) return NotFound();
        return NoContent();
    }
}
