using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.ApiContracts.Vehicle.Responses;
using Domain.Entities;
using FluentAssertions;
using IntegrationTests.SetupClass;
using Infrastructure.DBContexts;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public class VehicleMaintenanceHistory : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public VehicleMaintenanceHistory(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;
    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    private async Task AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [],
            cancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            cancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
    }

    [Fact(DisplayName = "VMH_001 - Tạo lịch sử bảo dưỡng sửa chữa cho xe qua client API")]
    public async Task CreateMaintenanceHistory_ClientEndpoint_ReturnsCreatedId()
    {
        await AuthenticateAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var user = new ApplicationUser { UserName = "vehicle_maint_user", Email = "vehicle_maint_user@example.com", EmailConfirmed = true, Status = Domain.Constants.UserStatus.Active };
        var vehicle = new Vehicle { User = user, UserId = Guid.NewGuid(), VinNumber = "VIN_MVH_001", EngineNumber = "ENG_MVH_001", LicensePlate = "59A-123.45", IsActive = true };
        db.Users.Add(user);
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var payload = new
        {
            maintenance_date = DateTimeOffset.UtcNow,
            description = "Thay dầu, cân chỉnh phanh",
            mileage = 12000,
            technician_id = 2,
            parts_cost = 150000m,
            labor_cost = 50000m,
            parts_json = "{\"items\":[{\"name\":\"Nhớt\",\"price\":150000}]}",
            next_maintenance_date = DateTimeOffset.UtcNow.AddMonths(3),
            next_maintenance_odo = 14500
        };

        var response = await _client.PostAsJsonAsync($"/api/v1/client/vehicles/{vehicle.Id}/maintenance-history", payload, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<int>(TestContext.Current.CancellationToken).ConfigureAwait(true);
        createdId.Should().BeGreaterThan(0);
    }
}
