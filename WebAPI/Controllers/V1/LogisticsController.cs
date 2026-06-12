using System;
using System.Threading.Tasks;
using Application.ApiContracts.Logistics.Responses;
using Application.ApiContracts.Logistics.CarrierSettings.Requests;
using Application.ApiContracts.Logistics.CarrierSettings.Responses;
using Application.Features.Logistics.Queries.GetActiveShipments;
using Application.Features.Logistics.Commands.TestCarrierConnection;
using Application.Features.Logistics.Commands.UpdateCarrierPartner;
using Application.Features.Logistics.Queries.GetCarriers;
using Application.Features.Logistics.Queries.GetLogisticsDashboard;
using Application.Features.Logistics.Queries.GetShipmentTracking;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/logistics")]
public class LogisticsController(IMediator mediator) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<LogisticsDashboardResponse> GetDashboard([FromQuery] string range = "today")
    {
        var query = new GetLogisticsDashboardQuery { Range = range ?? "today" };
        return await mediator.Send(query);
    }

    [HttpGet("carriers")]
    public async Task<GetCarriersResponse> GetCarriers()
    {
        return await mediator.Send(new GetCarriersQuery());
    }

    [HttpPut("carriers/{id}")]
    public async Task<IActionResult> UpdateCarrierPartner(int id, [FromBody] UpdateCarrierPartnerRequest request)
    {
        request ??= new UpdateCarrierPartnerRequest();
        var command = new UpdateCarrierPartnerCommand { Id = id, Request = request };
        var result = await mediator.Send(command);
        if (!result) return NotFound(new { message = "Không tìm thấy đối tác vận chuyển" });
        return NoContent();
    }

    [HttpPost("carriers/{id}/test-connection")]
    public async Task<TestCarrierConnectionResponse> TestCarrierConnection(int id, [FromBody] TestCarrierConnectionRequest request)
    {
        request ??= new TestCarrierConnectionRequest();
        var command = new TestCarrierConnectionCommand { Id = id, Request = request };
        return await mediator.Send(command);
    }

    [HttpGet("tracking/{search}")]
    public async Task<IActionResult> GetTracking(string search)
    {
        if (string.IsNullOrWhiteSpace(search)) return BadRequest(new { message = "Vui lòng nhập mã vận đơn, mã đơn hàng hoặc số điện thoại khách hàng" });
        var query = new GetShipmentTrackingQuery { TrackingNumberOrPhone = search.Trim() };
        var result = await mediator.Send(query);
        if (result == null) return NotFound(new { message = "Không tìm thấy thông tin vận chuyển" });
        return Ok(result);
    }

    [HttpGet("active-shipments")]
    public async Task<IActionResult> GetActiveShipments()
    {
        var result = await mediator.Send(new GetActiveShipmentsQuery());
        return Ok(result);
    }

    [HttpGet("fulfillment/{id}")]
    public async Task<IActionResult> GetFulfillmentDetail(int id)
    {
        var response = await mediator.Send(new Application.Features.Logistics.Queries.GetFulfillmentDetail.GetFulfillmentDetailQuery { Id = id });
        if (response == null) return NotFound();
        return Ok(response);
    }

    [HttpPut("fulfillment/{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] Application.Features.Logistics.Commands.ProcessFulfillment.UpdateParcelStatusCommand command)
    {
        command.Id = id;
        var result = await mediator.Send(command);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPut("fulfillment/{id}/tracking")]
    public async Task<IActionResult> UpdateTracking(int id, [FromBody] Application.Features.Logistics.Commands.ProcessFulfillment.UpdateTrackingNumberCommand command)
    {
        command.Id = id;
        var result = await mediator.Send(command);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPut("fulfillment/items/{itemId}/toggle-pick")]
    public async Task<IActionResult> ToggleItemPick(int itemId, [FromBody] Application.Features.Logistics.Commands.ProcessFulfillment.ToggleItemPickCommand command)
    {
        command.ItemId = itemId;
        var result = await mediator.Send(command);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("returns")]
    public async Task<IActionResult> GetReturns([FromQuery] string? status)
    {
        var result = await mediator.Send(new Application.Features.Logistics.Returns.Queries.GetReturns.GetReturnsQuery { Status = status });
        return Ok(result);
    }

    [HttpGet("returns/{id}")]
    public async Task<IActionResult> GetReturnDetail(int id)
    {
        var result = await mediator.Send(new Application.Features.Logistics.Returns.Queries.GetReturnDetail.GetReturnDetailQuery { Id = id });
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("returns/{id}/inspect")]
    public async Task<IActionResult> InspectReturn(int id, [FromBody] Application.Features.Logistics.Returns.Commands.InspectReturn.InspectReturnCommand command)
    {
        command.Id = id;
        var result = await mediator.Send(command);
        if (!result) return NotFound();
        return NoContent();
    }
}
