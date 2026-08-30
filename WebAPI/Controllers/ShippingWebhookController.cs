using Application.ApiContracts.Shipping.Requests;
using Application.Features.Outputs.Commands.UpdateOutputStatus;
using Application.Features.Sales.Returns.Commands.ProcessReturnArrival;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Logistics.Shipment;
using Domain.Constants.Logistics;
using Domain.Constants.Order;
using Domain.Enums;
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
        int outputIdInt = 0;
        int returnRequestIdInt = 0;
        bool isReturnOrder = false;

        // 1. Phân tích từ ClientOrderCode
        if (!string.IsNullOrEmpty(request.ClientOrderCode))
        {
            var parts = request.ClientOrderCode.Split('-');
            if (parts.Length >= 4 && string.Equals(parts[1], "RETURN", StringComparison.OrdinalIgnoreCase))
            {
                // Format: GHN-RETURN-{outputId}-{returnRequestId}-{timestamp}
                isReturnOrder = true;
                int.TryParse(parts[2], out outputIdInt);
                int.TryParse(parts[3], out returnRequestIdInt);
            }
            else if (parts.Length >= 3 && string.Equals(parts[1], "RETURN", StringComparison.OrdinalIgnoreCase))
            {
                isReturnOrder = true;
                int.TryParse(parts[2], out outputIdInt);
            }
            else if (parts.Length >= 2)
            {
                // Format: GHN-{outputId}-{timestamp}
                int.TryParse(parts[1], out outputIdInt);
            }
        }

        // 2. Tra cứu Shipment theo TrackingNumber (OrderCode) nếu chưa lấy được outputId
        Domain.Entities.Logistics.Shipment? shipment = null;
        if (!string.IsNullOrEmpty(request.OrderCode))
        {
            shipment = await shipmentReadRepository.GetByTrackingNumberAsync(request.OrderCode);
        }

        if (shipment == null && outputIdInt > 0)
        {
            shipment = await shipmentReadRepository.GetByOutputIdAsync(outputIdInt);
        }

        if (shipment != null)
        {
            if (outputIdInt == 0 && shipment.OutputId.HasValue)
            {
                outputIdInt = shipment.OutputId.Value;
            }
            if (shipment.Type == ShipmentType.ReturnDelivery)
            {
                isReturnOrder = true;
            }
        }

        var lowerStatus = request.Status?.ToLower();

        if (outputIdInt == 0)
        {
            return BadRequest();
        }

        string newStatus = string.Empty;

        if (isReturnOrder)
        {
            // Đối với đơn thu hồi hàng hoàn về kho:
            // "delivered" / "returned" / "return": Shipper GHN đã giao hàng hoàn về đến Kho/Showroom
            if (lowerStatus == "delivered" || lowerStatus == "returned" || lowerStatus == "return")
            {
                newStatus = OrderStatus.Refunded;
                if (shipment != null)
                {
                    shipment.DeliveredAt = DateTimeOffset.UtcNow;
                    shipment.Status = ParcelDeliveryStatus.Completed;
                    shipmentUpdateRepository.Update(shipment);
                    await unitOfWork.SaveChangesAsync();
                }
                // Tự động duyệt phiếu nhập kho và cộng tồn kho tức thì
                await sender.Send(new ProcessReturnArrivalCommand
                {
                    OutputId = outputIdInt,
                    ReturnRequestId = returnRequestIdInt > 0 ? returnRequestIdInt : null,
                    TrackingNumber = request.OrderCode
                });
            }
            else if (lowerStatus == "cancel" || lowerStatus == "damage" || lowerStatus == "lost")
            {
                newStatus = OrderStatus.Cancelled;
            }
            else
            {
                newStatus = OrderStatus.Refunding;
            }
        }
        else
        {
            // Đối với đơn xuất bán giao hàng đến khách:
            if (lowerStatus == "delivered")
            {
                newStatus = OrderStatus.Completed;
                if (shipment != null)
                {
                    shipment.DeliveredAt = DateTimeOffset.UtcNow;
                    shipment.Status = ParcelDeliveryStatus.Completed;
                    shipmentUpdateRepository.Update(shipment);
                    await unitOfWork.SaveChangesAsync();
                }
            }
            else if (lowerStatus == "returned" || lowerStatus == "return" || lowerStatus == "return_transporting" || lowerStatus == "return_sorting")
            {
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
