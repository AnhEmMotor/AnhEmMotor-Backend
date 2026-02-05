using Application.Common.Models;
using Domain.Constants.Permission;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore;
using SettingEntity = Domain.Entities.Setting;

namespace IntegrationTests;

public class Setting : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public Setting(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _output = output;
    }

#pragma warning disable CRR0035
    [Fact(DisplayName = "SETTING_001 - GetAllSettings - Thành công (Happy Path)")]
    public async Task SETTING_001_GetAllSettings_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Settings.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

            // Use AddOrUpdate logic to avoid UNIQUE constraint violations
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Deposit_ratio", "50.5");
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Inventory_alert_level", "10");
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Order_value_exceeds", "50000000");
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Z-bike_threshold_for_meeting", "5");
        }

        var response = await _client.GetAsync("/api/v1/Setting");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string?>>();
        content.Should().NotBeNull();
        content.Should().HaveCount(4);
        content!["Deposit_ratio"].Should().Be("50.5");
        content["Inventory_alert_level"].Should().Be("10");
        content["Order_value_exceeds"].Should().Be("50000000");
        content["Z-bike_threshold_for_meeting"].Should().Be("5");
    }

    [Fact(DisplayName = "SETTING_002 - GetAllSettings - Không có quyền xem")]
    public async Task SETTING_002_GetAllSettings_Forbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        // No Permissions
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Deposit_ratio", "50");
        }

        var response = await _client.GetAsync("/api/v1/Setting");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "SETTING_003 - GetAllSettings - Chưa đăng nhập")]
    public async Task SETTING_003_GetAllSettings_Unauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Deposit_ratio", "50");
            
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/v1/Setting");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "SETTING_004 - GetAllSettings - Database rỗng")]
    public async Task SETTING_004_GetAllSettings_EmptyDatabase_ReturnsEmptyDictionary()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Settings.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Settings.RemoveRange(db.Settings);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/v1/Setting");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "SETTING_005 - SetSettings - Thành công với tất cả keys hợp lệ")]
    public async Task SETTING_005_SetSettings_AllValidKeys_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Settings.Edit], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Deposit_ratio", "30");
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Inventory_alert_level", "5");
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Order_value_exceeds", "30000000");
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Z-bike_threshold_for_meeting", "3");
        }

        var request = new Dictionary<string, string?>
        {
            { "Deposit_ratio", "50" },
            { "Inventory_alert_level", "10" },
            { "Order_value_exceeds", "50000000" },
            { "Z-bike_threshold_for_meeting", "5" }
        };

        var response = await _client.PutAsJsonAsync("/api/v1/Setting", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string?>>();
        content.Should().NotBeNull();
        content.Should().HaveCount(4);
        content!["Deposit_ratio"].Should().Be("50");
        content["Inventory_alert_level"].Should().Be("10");

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var savedSettings = db.Settings.ToList();
            savedSettings.Should().HaveCount(4);
            savedSettings.First(s => string.Compare(s.Key, "Deposit_ratio") == 0).Value.Should().Be("50");
            savedSettings.First(s => string.Compare(s.Key, "Inventory_alert_level") == 0).Value.Should().Be("10");
        }
    }

    [Fact(DisplayName = "SETTING_006 - SetSettings - Cập nhật chỉ 1 key")]
    public async Task SETTING_006_SetSettings_UpdateSingleKey_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Settings.Edit], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Deposit_ratio", "30");
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Inventory_alert_level", "5");
        }

        var request = new Dictionary<string, string?> { { "Deposit_ratio", "25" } };

        var response = await _client.PutAsJsonAsync("/api/v1/Setting", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var savedSettings = db.Settings.ToList();
            savedSettings.First(s => string.Compare(s.Key, "Deposit_ratio") == 0).Value.Should().Be("25");
            savedSettings.First(s => string.Compare(s.Key, "Inventory_alert_level") == 0).Value.Should().Be("5");
        }
    }

    [Fact(DisplayName = "SETTING_007 - SetSettings - Không có quyền chỉnh sửa")]
    public async Task SETTING_007_SetSettings_Forbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        // View permission only
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Settings.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Deposit_ratio", "50");
        }

        var request = new Dictionary<string, string?> { { "Deposit_ratio", "25" } };

        var response = await _client.PutAsJsonAsync("/api/v1/Setting", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var setting = db.Settings.First(s => s.Key == "Deposit_ratio");
            setting.Value.Should().Be("50");
        }
    }

    [Fact(DisplayName = "SETTING_008 - SetSettings - Chưa đăng nhập")]
    public async Task SETTING_008_SetSettings_Unauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Deposit_ratio", "50");
        }

        var request = new Dictionary<string, string?> { { "Deposit_ratio", "25" } };

        var response = await _client.PutAsJsonAsync("/api/v1/Setting", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "SETTING_009 - SetSettings - Deposit_ratio dưới ngưỡng tối thiểu")]
    public async Task SETTING_009_SetSettings_DepositRatioBelowMinimum_BadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Settings.Edit], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Deposit_ratio", "50");
        }

        var request = new Dictionary<string, string?> { { "Deposit_ratio", "0" } };

        var response = await _client.PutAsJsonAsync("/api/v1/Setting", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        content.Should().NotBeNull();
        content!.Errors.Should().Contain(e => e.Message.Contains("between 1.0 and 99.0"));

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var setting = db.Settings.First(s => s.Key == "Deposit_ratio");
            setting.Value.Should().Be("50");
        }
    }

    [Fact(DisplayName = "SETTING_010 - SetSettings - Deposit_ratio trên ngưỡng tối đa")]
    public async Task SETTING_010_SetSettings_DepositRatioAboveMaximum_BadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Settings.Edit], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Deposit_ratio", "50");
        }

        var request = new Dictionary<string, string?> { { "Deposit_ratio", "100" } };

        var response = await _client.PutAsJsonAsync("/api/v1/Setting", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        content.Should().NotBeNull();
        content!.Errors.Should().Contain(e => e.Message.Contains("between 1.0 and 99.0"));

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var setting = db.Settings.First(s => s.Key == "Deposit_ratio");
            setting.Value.Should().Be("50");
        }
    }

    [Fact(DisplayName = "SETTING_011 - SetSettings - Deposit_ratio với nhiều hơn 1 chữ số thập phân")]
    public async Task SETTING_011_SetSettings_DepositRatioMultipleDecimals_BadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Settings.Edit], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Deposit_ratio", "50");
        }

        var request = new Dictionary<string, string?> { { "Deposit_ratio", "50.55" } };

        var response = await _client.PutAsJsonAsync("/api/v1/Setting", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        content.Should().NotBeNull();
        content!.Errors.Should().Contain(e => e.Message.Contains("decimal place"));
    }

    [Fact(DisplayName = "SETTING_012 - SetSettings - Request body rỗng")]
    public async Task SETTING_012_SetSettings_EmptyRequest_BadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Settings.Edit], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Setting (Key, Value) VALUES ({0}, {1}) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value, DeletedAt=NULL",
                "Deposit_ratio", "50");
        }

        var request = new Dictionary<string, string?>();

        var response = await _client.PutAsJsonAsync("/api/v1/Setting", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        content.Should().NotBeNull();
        content!.Errors.Should().Contain(e => e.Message.Contains("cannot be empty"));
    }
#pragma warning restore CRR0035
}
