using Application.Features.Order.Queries.GetOrderStatistics;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ReturnRequest;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class OrderStatistics
{
    [Fact]
    public async Task Handle_UsesOutputsAndReturnRequests()
    {
        var today = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var outputRepository = new Mock<IOutputReadRepository>();
        var returnRequestRepository = new Mock<IReturnRequestReadRepository>();
        outputRepository
            .Setup(repository => repository.GetOrderStatisticsDataAsync(It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(
                new List<Output>
                {
                    new() { Id = 1, StatusId = "pending", CreatedAt = today.AddDays(-2) },
                    new() { Id = 2, StatusId = "delivering", CreatedAt = today.AddDays(-3) },
                    new() { Id = 3, StatusId = "completed", CreatedAt = today, LastStatusChangedAt = today },
                    new() { Id = 4, StatusId = "cancelled", PaymentStatus = "Failed", CreatedAt = today.AddHours(2) }
                });
        returnRequestRepository
            .Setup(repository => repository.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var handler = new GetOrderStatisticsQueryHandler(outputRepository.Object, returnRequestRepository.Object);
        var result = await handler.Handle(new GetOrderStatisticsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PendingOrders.Should().Be(2);
        result.Value.SlaDelayed.Should().Be(2);
        result.Value.PaymentErrors.Should().Be(1);
        result.Value.ReturnRequests.Should().Be(2);
        result.Value.CompletedToday.Should().Be(1);
        result.Value.HourlyData.Should().Contain(item => item.Hour == "00:00" && item.Count == 1);
        result.Value.HourlyData.Should().Contain(item => item.Hour == "02:00" && item.Count == 1);
    }

    [Fact]
    public async Task Handle_WhenRepositoriesAreEmpty_ReturnsRealEmptyValues()
    {
        var outputRepository = new Mock<IOutputReadRepository>();
        var returnRequestRepository = new Mock<IReturnRequestReadRepository>();
        outputRepository
            .Setup(repository => repository.GetOrderStatisticsDataAsync(It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(new List<Output>());
        returnRequestRepository
            .Setup(repository => repository.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var handler = new GetOrderStatisticsQueryHandler(outputRepository.Object, returnRequestRepository.Object);
        var result = await handler.Handle(new GetOrderStatisticsQuery(), CancellationToken.None);

        result.Value.PendingOrders.Should().Be(0);
        result.Value.CompletedToday.Should().Be(0);
        result.Value.HourlyData.Should().BeEmpty();
        result.Value.ExceptionOrders.Should().BeEmpty();
    }
}