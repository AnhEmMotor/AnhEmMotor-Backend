using Application.ApiContracts.Logistics.Responses;
using Application.Features.Logistics.Queries.GetFulfillmentOrders;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Entities.Logistics;
using Domain.Enums;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class LogisticsFulfillment
    {
        private readonly Mock<IParcelDeliveryOrderReadRepository> _readRepoMock;

        public LogisticsFulfillment()
        {
            _readRepoMock = new Mock<IParcelDeliveryOrderReadRepository>();
        }

        [Fact(DisplayName = "LOGISTICS_001 - Lấy danh sách đơn vận chuyển không có bộ lọc")]
        public async Task LOGISTICS_001_GetFulfillmentOrders_NoFilters_ShouldReturnAll()
        {
            var handler = new GetFulfillmentOrdersQueryHandler(_readRepoMock.Object);
            var query = new GetFulfillmentOrdersQuery();
            
            var mockParcels = new List<ParcelDeliveryOrder>
            {
                new() { Id = 1, OriginalOrderCode = "ORD-001", Status = ParcelDeliveryStatus.Completed, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new() { Id = 2, OriginalOrderCode = "ORD-002", Status = ParcelDeliveryStatus.Shipping, CreatedAt = DateTime.UtcNow }
            };

            _readRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockParcels);

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
            
            var mockParcels = new List<ParcelDeliveryOrder>
            {
                new() { Id = 1, OriginalOrderCode = "ORD-001", Status = ParcelDeliveryStatus.Completed, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new() { Id = 2, OriginalOrderCode = "ORD-002", Status = ParcelDeliveryStatus.Shipping, CreatedAt = DateTime.UtcNow }
            };

            _readRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockParcels);

            var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result.First().Id.Should().Be(1);
        }
    }
}
