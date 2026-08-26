using Application.ApiContracts.Shipping.Requests;
using Application.Features.Outputs.Commands.UpdateOutputStatus;
using Application.Features.Sales.Returns.Commands.ProcessReturnArrival;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Logistics.Shipment;
using Domain.Entities.Logistics;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers;

namespace ControllerTests;

public class ShippingWebhook
{
    private readonly Mock<ISender> _senderMock = new();
    private readonly Mock<IShipmentReadRepository> _shipmentReadMock = new();
    private readonly Mock<IShipmentUpdateRepository> _shipmentUpdateMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ShippingWebhookController _controller;

    public ShippingWebhook()
    {
        _controller = new ShippingWebhookController(
            _senderMock.Object,
            _shipmentReadMock.Object,
            _shipmentUpdateMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact(DisplayName = "HOOK_001 - Webhook 'returned': gửi arrival restock + chuyển đơn sang refunding")]
    public async Task ReturnedStatus_TriggersArrivalAndRefunding()
    {
        var request = new GhnWebhookRequest
        {
            ClientOrderCode = "GHN-42-1717000000",
            Status = "returned"
        };

        var result = await _controller.HandleGhnWebhook(request);

        result.Should().BeOfType<OkResult>();
        _senderMock.Verify(
            sender => sender.Send(
                It.Is<ProcessReturnArrivalCommand>(command => command.OutputId == 42),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _senderMock.Verify(
            sender => sender.Send(
                It.Is<UpdateOutputStatusCommand>(command =>
                    command.Id == 42 && command.StatusId == "refunding"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "HOOK_002 - Webhook 'delivered': KHÔNG gửi arrival restock")]
    public async Task DeliveredStatus_DoesNotTriggerArrivalRestock()
    {
        var shipment = new Shipment { TrackingNumber = "5ENLKKHD", OutputId = 42 };
        _shipmentReadMock
            .Setup(repository => repository.GetByOutputIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);
        _unitOfWorkMock
            .Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var request = new GhnWebhookRequest
        {
            ClientOrderCode = "GHN-42-1717000000",
            Status = "delivered"
        };

        var result = await _controller.HandleGhnWebhook(request);

        result.Should().BeOfType<OkResult>();
        _senderMock.Verify(
            sender => sender.Send(
                It.Is<ProcessReturnArrivalCommand>(command => command.OutputId == 42),
                It.IsAny<CancellationToken>()),
            Times.Never);
        shipment.DeliveredAt.Should().NotBeNull();
        _shipmentUpdateMock.Verify(repository => repository.Update(shipment), Times.Once);
    }
}
