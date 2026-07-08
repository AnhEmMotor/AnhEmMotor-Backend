using Application.ApiContracts.Shipping.Requests;
using Application.Features.Outputs.Commands.UpdateOutputStatus;
using Domain.Constants.Order;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace WebAPI.Controllers;

[Route("api/shipping-webhook")]
[ApiController]
public class ShippingWebhookController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<ShippingWebhookController> _logger;

    public ShippingWebhookController(ISender sender, ILogger<ShippingWebhookController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [HttpPost("ghtk")]
    public async Task<IActionResult> HandleGhtkWebhook([FromForm] GhtkWebhookRequest request)
    {
        _logger.LogInformation("Received GHTK webhook for partner_id: {PartnerId}, status_id: {StatusId}", request.partner_id, request.status_id);

        if (string.IsNullOrEmpty(request.partner_id))
        {
            return BadRequest();
        }

        if (!int.TryParse(request.partner_id, out int outputIdInt))
        {
            return BadRequest();
        }
        
        string newStatus = string.Empty;

        // Map GHTK status_id to our OrderStatus
        // Theo GHTK: 5 = Đã giao hàng/Chưa đối soát, -1 = Hủy đơn hàng
        if (request.status_id == 5)
        {
            newStatus = OrderStatus.Completed;
        }
        else if (request.status_id == -1)
        {
            // The plan says: "nếu đã bị huỷ đơn hàng thì sẽ về phần đang hoàn tiền."
            newStatus = OrderStatus.Refunding;
        }

        if (!string.IsNullOrEmpty(newStatus))
        {
            var command = new UpdateOutputStatusCommand
            {
                Id = outputIdInt,
                StatusId = newStatus,
                CurrentUserId = System.Guid.Empty // System user identifier
            };

            var result = await _sender.Send(command);

            if (result.IsFailure)
            {
                _logger.LogError("Failed to update order status via webhook: {Error}", result.Error);
                // Even if our update failed due to some transition rules, we return OK to GHTK so they don't retry unnecessarily
            }
        }

        return Ok();
    }
}
