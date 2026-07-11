using Application.ApiContracts.Shipping.Requests;
using Application.Features.Outputs.Commands.UpdateOutputStatus;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Logistics.Shipment;
using Domain.Constants.Order;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebAPI.Controllers;

[Route("api/shipping-webhook")]
[ApiController]
public class ShippingWebhookController(
    ISender sender,
    ILogger<ShippingWebhookController> logger,
    IShipmentReadRepository shipmentReadRepository,
    IShipmentUpdateRepository shipmentUpdateRepository,
    IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpPost("ghn")]
    public async Task<IActionResult> HandleGhnWebhook([FromBody] GhnWebhookRequest request)
    {
        logger.LogInformation(
            "Received GHN webhook for OrderCode: {OrderCode}, Status: {Status}",
            request.ClientOrderCode,
            request.Status);
        if (string.IsNullOrEmpty(request.ClientOrderCode))
        {
            return BadRequest();
        }
        var parts = request.ClientOrderCode.Split('-');
        if (parts.Length < 2 || !int.TryParse(parts[1], out int outputIdInt))
        {
            return BadRequest();
        }
        string newStatus = string.Empty;
        if (request.Status == "delivered")
        {
            newStatus = OrderStatus.Completed;
            var shipment = await shipmentReadRepository.GetByOutputIdAsync(outputIdInt);
            if (shipment != null)
            {
                shipment.DeliveredAt = DateTimeOffset.UtcNow;
                shipmentUpdateRepository.Update(shipment);
                await unitOfWork.SaveChangesAsync();
            }
        } else if (request.Status == "cancel" || request.Status == "returned")
        {
            newStatus = OrderStatus.Refunding;
        }
        if (!string.IsNullOrEmpty(newStatus))
        {
            var command = new UpdateOutputStatusCommand
            {
                Id = outputIdInt,
                StatusId = newStatus,
                CurrentUserId = Guid.Empty
            };
            var result = await sender.Send(command);
            if (result.IsFailure)
            {
                logger.LogError("Failed to update order status via webhook: {Error}", result.Error);
            }
        }
        return Ok();
    }
}
