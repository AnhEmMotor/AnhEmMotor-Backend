using Application.Features.Logistics.Queries.GetFulfillmentOrders;
using Application.Interfaces.Repositories.Logistics.Shipment;
using Domain.Entities.Logistics;
using Domain.Enums;
using FluentAssertions;
using Moq;
using System;
using System.Linq;

namespace UnitTests
{
    public class LogisticsFulfillment
    {
        private readonly Mock<IShipmentReadRepository> _readRepoMock;

        public LogisticsFulfillment()
        {
            _readRepoMock = new Mock<IShipmentReadRepository>();
        }

        [Fact(DisplayName = "LOGISTICS_003 - Lọc fulfillment theo carrier và khoảng ngày")]
        public async Task LOGISTICS_003_GetFulfillmentOrders_FilterByCarrierAndDate_ShouldReturnMatchingShipments()
        {
            var handler = new GetFulfillmentOrdersQueryHandler(_readRepoMock.Object);
            var query = new GetFulfillmentOrdersQuery
            {
                Carrier = "GHN",
                FromDate = new DateTime(2026, 8, 20),
                ToDate = new DateTime(2026, 8, 21)
            };
            _readRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
                [
                    new Shipment { Id = 1, Carrier = "GHN", CreatedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero) },
                    new Shipment { Id = 2, Carrier = "GHTK", CreatedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero) },
                    new Shipment { Id = 3, Carrier = "GHN", CreatedAt = new DateTimeOffset(2026, 8, 22, 12, 0, 0, TimeSpan.Zero) }
                ]);

            var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

            result.Should().ContainSingle();
            result.First().Id.Should().Be(1);
        }

        [Fact(DisplayName = "LOGISTICS_001 - Lấy danh sách đơn vận chuyển không có bộ lọc")]
        public async Task LOGISTICS_001_GetFulfillmentOrders_NoFilters_ShouldReturnAll()
        {
            var handler = new GetFulfillmentOrdersQueryHandler(_readRepoMock.Object);
            var query = new GetFulfillmentOrdersQuery();
            var mockParcels = new List<Shipment>
            {
                new()
                {
                    Id = 1,
                    TrackingNumber = "ORD-001",
                    Status = ParcelDeliveryStatus.Completed,
                    DeliveredAt = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
                },
                new() { Id = 2, TrackingNumber = "ORD-002", DeliveredAt = null, CreatedAt = DateTimeOffset.UtcNow }
            };
            _readRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockParcels);
            var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result.First().Id.Should().Be(2);
        }

        [Fact(DisplayName = "LOGISTICS_002 - Lấy danh sách đơn vận chuyển lọc theo trạng thái")]
        public async Task LOGISTICS_002_GetFulfillmentOrders_FilterByStatus_ShouldReturnFiltered()
        {
            var handler = new GetFulfillmentOrdersQueryHandler(_readRepoMock.Object);
            var query = new GetFulfillmentOrdersQuery { Status = ParcelDeliveryStatus.Completed };
            var mockParcels = new List<Shipment>
            {
                new()
                {
                    Id = 1,
                    TrackingNumber = "ORD-001",
                    Status = ParcelDeliveryStatus.Completed,
                    DeliveredAt = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
                },
                new() { Id = 2, TrackingNumber = "ORD-002", DeliveredAt = null, CreatedAt = DateTimeOffset.UtcNow }
            };
            _readRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockParcels);
            var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result.First().Id.Should().Be(1);
        }
    }
}

