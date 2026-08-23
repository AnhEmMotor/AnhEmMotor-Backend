using Application.Features.Logistics.Queries.GetActiveShipments;
using Application.Interfaces.Repositories.Logistics.Shipment;
using Domain.Entities.Logistics;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class LogisticsActiveShipments
{
    [Fact]
    public async Task Handle_UsesRepositoryActiveDeliveryFilter()
    {
        var repository = new Mock<IShipmentReadRepository>();
        repository
            .Setup(item => item.GetActiveDeliveryShipmentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                [new Shipment
                {
                    Id = 1,
                    TrackingNumber = "GHTK-001",
                    CustomerName = "Test customer",
                    OutputId = 10,
                    CreatedAt = DateTimeOffset.UtcNow.AddHours(-1)
                }]);

        var handler = new GetActiveShipmentsQueryHandler(repository.Object);
        var result = await handler.Handle(new GetActiveShipmentsQuery(), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].TrackingNumber.Should().Be("GHTK-001");
        repository.Verify(item => item.GetActiveDeliveryShipmentsAsync(It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(item => item.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}