using Domain.Entities;
using Domain.Constants.Order;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WebAPI.Controllers;

namespace IntegrationTests;

public class OrderStatistics : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;

    public OrderStatistics(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(true);
    }

    [Fact]
    public async Task GetOrderStatistics_UsesOutputData()
    {
        await _factory.ResetDatabaseAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.OutputOrders.AddRange(
                new Output { StatusId = OrderStatus.Pending, CreatedAt = DateTimeOffset.UtcNow },
                new Output { StatusId = OrderStatus.Completed, CreatedAt = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }

        var client = _factory.CreateClient();
        var userId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
                _factory.Services,
                $"statistics_{userId}",
                "Password123!",
                [Domain.Constants.Permission.Permissions.Admin.DashboardManagement.View],
                TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
                client,
                $"statistics_{userId}",
                "Password123!",
                TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var response = await client.GetAsync(
                "/api/v1/statistics/order-statistics",
                TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var data = await response.Content.ReadFromJsonAsync<StatisticsResponse>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.IsSuccessStatusCode.Should().BeTrue();
        data.Should().NotBeNull();
        data!.PendingOrders.Should().Be(1);
        data.CompletedToday.Should().Be(1);
    }

    private sealed class StatisticsResponse
    {
        public int PendingOrders { get; set; }

        public int CompletedToday { get; set; }
    }
}