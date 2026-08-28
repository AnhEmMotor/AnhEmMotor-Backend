using Application.Features.Outputs.Commands.UpdateOutputStatus;
using Application.Features.Sales.Returns.Commands.ProcessReturnArrival;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Logistics.Shipment;
using Application.Interfaces.Services.Shipping;
using Domain.Constants.Logistics;
using Domain.Constants.Order;
using Domain.Enums;
using MediatR;
using System;

namespace WebAPI.BackgroundServices;

public class GhnStatusPollingWorker : BackgroundService
{
    private readonly ILogger<GhnStatusPollingWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public GhnStatusPollingWorker(ILogger<GhnStatusPollingWorker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                await DoWorkAsync(stoppingToken);
            } catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var shipmentReadRepository = scope.ServiceProvider.GetRequiredService<IShipmentReadRepository>();
        var shipmentUpdateRepository = scope.ServiceProvider.GetRequiredService<IShipmentUpdateRepository>();
        var shippingService = scope.ServiceProvider.GetRequiredService<IShippingService>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var activeShipments = await shipmentReadRepository.GetActiveDeliveryShipmentsAsync(stoppingToken);

        foreach (var shipment in activeShipments)
        {
            if (string.IsNullOrEmpty(shipment.TrackingNumber))
                continue;
            var statusResult = await shippingService.GetShippingOrderStatusAsync(shipment.TrackingNumber, stoppingToken);
            if (statusResult.IsFailure)
            {
                continue;
            }
            var ghnStatus = statusResult.Value?.ToLowerInvariant();
            if (string.IsNullOrEmpty(ghnStatus))
                continue;

            string newStatus = string.Empty;
            var isReturn = shipment.Type == ShipmentType.ReturnDelivery;

            if (isReturn)
            {
                // Khi đơn thu hồi/hoàn hàng được GHN giao thành công về kho (delivered/returned)
                if (ghnStatus == "delivered" || ghnStatus == "returned" || ghnStatus == "return")
                {
                    newStatus = OrderStatus.Refunded;
                    shipment.Status = ParcelDeliveryStatus.Completed;
                    shipment.DeliveredAt = DateTimeOffset.UtcNow;
                    shipmentUpdateRepository.Update(shipment);
                    await unitOfWork.SaveChangesAsync(stoppingToken);

                    if (shipment.OutputId.HasValue)
                    {
                        await sender.Send(
                            new ProcessReturnArrivalCommand
                            {
                                OutputId = shipment.OutputId.Value,
                                TrackingNumber = shipment.TrackingNumber
                            },
                            stoppingToken);
                    }
                }
                else if (ghnStatus == "cancel" || ghnStatus == "damage" || ghnStatus == "lost")
                {
                    newStatus = OrderStatus.Cancelled;
                    shipment.Status = ParcelDeliveryStatus.Returned;
                    shipmentUpdateRepository.Update(shipment);
                    await unitOfWork.SaveChangesAsync(stoppingToken);
                }
            }
            else
            {
                // Khi đơn xuất bán giao cho khách
                if (ghnStatus == "delivered")
                {
                    newStatus = OrderStatus.Completed;
                    shipment.Status = ParcelDeliveryStatus.Completed;
                    shipment.DeliveredAt = DateTimeOffset.UtcNow;
                    shipmentUpdateRepository.Update(shipment);
                    await unitOfWork.SaveChangesAsync(stoppingToken);
                }
                else if (ghnStatus == "cancel" || ghnStatus == "returned" || ghnStatus == "return")
                {
                    newStatus = OrderStatus.Refunding;
                    shipment.Status = ParcelDeliveryStatus.Returned;
                    shipmentUpdateRepository.Update(shipment);
                    await unitOfWork.SaveChangesAsync(stoppingToken);

                    if (shipment.OutputId.HasValue)
                    {
                        await sender.Send(
                            new ProcessReturnArrivalCommand { OutputId = shipment.OutputId.Value },
                            stoppingToken);
                    }
                }
            }

            if (!string.IsNullOrEmpty(newStatus) && shipment.OutputId.HasValue)
            {
                var command = new UpdateOutputStatusCommand
                {
                    Id = shipment.OutputId.Value,
                    StatusId = newStatus,
                    CurrentUserId = Guid.Empty
                };
                await sender.Send(command, stoppingToken);
            }
        }
    }
}
