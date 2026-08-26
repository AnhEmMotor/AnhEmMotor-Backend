using Application.ApiContracts.Shipping.Requests;
using Application.Features.Outputs.Commands.UpdateOutputStatus;
using Application.Features.Sales.Returns.Commands.ProcessReturnArrival;
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
    IShipmentReadRepository shipmentReadRepository,
    IShipmentUpdateRepository shipmentUpdateRepository,
    IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpPost("ghn")]
    public async Task<IActionResult> HandleGhnWebhook([FromBody] GhnWebhookRequest request)
    {
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
        var lowerStatus = request.Status?.ToLower();
        if (lowerStatus == "delivered")
        {
            newStatus = OrderStatus.Completed;
            var shipment = await shipmentReadRepository.GetByOutputIdAsync(outputIdInt);
            if (shipment != null)
            {
                shipment.DeliveredAt = DateTimeOffset.UtcNow;
                shipmentUpdateRepository.Update(shipment);
                await unitOfWork.SaveChangesAsync();
            }
        }
        else if (lowerStatus == "returned" || lowerStatus == "return" || lowerStatus == "return_transporting" || lowerStatus == "return_sorting")
        {
            // Khi hàng hoàn về kho thành công: Tự động nhập kho và hoàn tiền / đóng đơn
            if (lowerStatus == "returned" || lowerStatus == "return")
            {
                newStatus = OrderStatus.Refunded;
                await sender.Send(new ProcessReturnArrivalCommand { OutputId = outputIdInt });
            }
            else
            {
                newStatus = OrderStatus.Refunding;
            }
        }
        else if (lowerStatus == "cancel" || lowerStatus == "damage" || lowerStatus == "lost")
        {
            newStatus = OrderStatus.Cancelled;
            await sender.Send(new ProcessReturnArrivalCommand { OutputId = outputIdInt });
        }

        if (!string.IsNullOrEmpty(newStatus))
        {
            var command = new UpdateOutputStatusCommand
            {
                Id = outputIdInt,
                StatusId = newStatus,
                CurrentUserId = Guid.Empty
            };
            await sender.Send(command);
        }
        return Ok();
    }
}
