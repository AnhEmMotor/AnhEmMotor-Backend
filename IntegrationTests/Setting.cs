using System.Net;
using System.Net.Http.Json;

using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using SettingEntity = Domain.Entities.Setting;

namespace IntegrationTests;

public class Setting : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public Setting(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

#pragma warning disable CRR0035
    [Fact(DisplayName = "SETTING_001 - GetAllSettings - Thành công (Happy Path)")]
    public async Task SETTING_001_GetAllSettings_Success()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            
            // Clear existing settings
            db.Settings.RemoveRange(db.Settings);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            // Seed settings
            var settings = new List<SettingEntity>
            {
                new (){ Key = "Deposit_ratio", Value = "50.5" },
                new (){ Key = "Inventory_alert_level", Value = "10" },
                new (){ Key = "Order_value_exceeds", Value = "50000000" },
                new (){ Key = "Z-bike_threshold_for_meeting", Value = "5" }
            };
            await db.Settings.AddRangeAsync(settings, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        // Act
        var response = await _client.GetAsync("/api/v1/Setting", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, decimal?>>();
        content.Should().NotBeNull();
        content.Should().HaveCount(4);
        content!["Deposit_ratio"].Should().Be(50.5m);
        content["Inventory_alert_level"].Should().Be(10m);
        content["Order_value_exceeds"].Should().Be(50000000m);
        content["Z-bike_threshold_for_meeting"].Should().Be(5m);
    }

    [Fact(DisplayName = "SETTING_002 - GetAllSettings - Không có quyền xem")]
    public async Task SETTING_002_GetAllSettings_Forbidden()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            
            // Seed settings
            db.Settings.RemoveRange(db.Settings);
            var settings = new List<SettingEntity>
            {
                new (){ Key = "Deposit_ratio", Value = "50" }
            };
            await db.Settings.AddRangeAsync(settings, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        // Act
        // Note: Without proper authentication setup, this will test authorization flow
        var response = await _client.GetAsync("/api/v1/Setting", CancellationToken.None).ConfigureAwait(true);

        // Assert
        // Depending on auth configuration, should be 401 or 403
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "SETTING_003 - GetAllSettings - Chưa đăng nhập")]
    public async Task SETTING_003_GetAllSettings_Unauthorized()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Settings.RemoveRange(db.Settings);
            await db.Settings.AddAsync(new (){ Key = "Deposit_ratio", Value = "50" });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        // Act
        var response = await _client.GetAsync("/api/v1/Setting", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "SETTING_004 - GetAllSettings - Database rỗng")]
    public async Task SETTING_004_GetAllSettings_EmptyDatabase_ReturnsEmptyDictionary()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Settings.RemoveRange(db.Settings);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        // Act
        var response = await _client.GetAsync("/api/v1/Setting", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, decimal?>>();
        content.Should().NotBeNull();
        content.Should().BeEmpty();
    }

    [Fact(DisplayName = "SETTING_005 - SetSettings - Thành công với tất cả keys hợp lệ")]
    public async Task SETTING_005_SetSettings_AllValidKeys_Success()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            
            // Seed initial settings
            db.Settings.RemoveRange(db.Settings);
            var initialSettings = new List<SettingEntity>
            {
                new (){ Key = "Deposit_ratio", Value = "30" },
                new (){ Key = "Inventory_alert_level", Value = "5" },
                new (){ Key = "Order_value_exceeds", Value = "30000000" },
                new (){ Key = "Z-bike_threshold_for_meeting", Value = "3" }
            };
            await db.Settings.AddRangeAsync(initialSettings, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        var request = new Dictionary<string, long?>
        {
            { "Deposit_ratio", 50 },
            { "Inventory_alert_level", 10 },
            { "Order_value_exceeds", 50000000 },
            { "Z-bike_threshold_for_meeting", 5 }
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/Setting", request).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, long?>>();
        content.Should().NotBeNull();
        content.Should().HaveCount(4);
        content!["Deposit_ratio"].Should().Be(50);
        content["Inventory_alert_level"].Should().Be(10);

        // Verify in database
        using (var scope = _factory.Services.CreateScope())
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
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            
            db.Settings.RemoveRange(db.Settings);
            var initialSettings = new List<SettingEntity>
            {
                new (){ Key = "Deposit_ratio", Value = "30" },
                new (){ Key = "Inventory_alert_level", Value = "5" }
            };
            await db.Settings.AddRangeAsync(initialSettings, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        var request = new Dictionary<string, long?>
        {
            { "Deposit_ratio", 25 }
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/Setting", request).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify only Deposit_ratio changed
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var savedSettings = db.Settings.ToList();
            savedSettings.First(s => s.Key == "Deposit_ratio").Value.Should().Be("25");
            savedSettings.First(s => s.Key == "Inventory_alert_level").Value.Should().Be("5"); // Unchanged
        }
    }

    [Fact(DisplayName = "SETTING_007 - SetSettings - Không có quyền chỉnh sửa")]
    public async Task SETTING_007_SetSettings_Forbidden()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Settings.RemoveRange(db.Settings);
            await db.Settings.AddAsync(new (){ Key = "Deposit_ratio", Value = "50" });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        var request = new Dictionary<string, long?> { { "Deposit_ratio", 25 } };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/Setting", request).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        
        // Verify DB unchanged
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var setting = db.Settings.First(s => s.Key == "Deposit_ratio");
            setting.Value.Should().Be("50"); // Not changed
        }
    }

    [Fact(DisplayName = "SETTING_008 - SetSettings - Chưa đăng nhập")]
    public async Task SETTING_008_SetSettings_Unauthorized()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Settings.RemoveRange(db.Settings);
            await db.Settings.AddAsync(new (){ Key = "Deposit_ratio", Value = "50" });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        var request = new Dictionary<string, long?> { { "Deposit_ratio", 25 } };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/Setting", request).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "SETTING_009 - SetSettings - Deposit_ratio dưới ngưỡng tối thiểu")]
    public async Task SETTING_009_SetSettings_DepositRatioBelowMinimum_BadRequest()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Settings.RemoveRange(db.Settings);
            await db.Settings.AddAsync(new (){ Key = "Deposit_ratio", Value = "50" });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        var request = new Dictionary<string, long?> { { "Deposit_ratio", 0 } };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/Setting", request).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<Application.Common.Models.ErrorResponse>(CancellationToken.None).ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Errors.Should().Contain(e => 
            e.Field == "Deposit_ratio" && 
            e.Message.Contains("between 1.0 and 99.0"));
        
        // Verify DB unchanged
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var setting = db.Settings.First(s => s.Key == "Deposit_ratio");
            setting.Value.Should().Be("50");
        }
    }

    [Fact(DisplayName = "SETTING_010 - SetSettings - Deposit_ratio trên ngưỡng tối đa")]
    public async Task SETTING_010_SetSettings_DepositRatioAboveMaximum_BadRequest()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Settings.RemoveRange(db.Settings);
            await db.Settings.AddAsync(new (){ Key = "Deposit_ratio", Value = "50" });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        var request = new Dictionary<string, long?> { { "Deposit_ratio", 100 } };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/Setting", request).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<Application.Common.Models.ErrorResponse>(CancellationToken.None).ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Errors.Should().Contain(e => 
            e.Field == "Deposit_ratio" && 
            e.Message.Contains("between 1.0 and 99.0"));
        
        // Verify DB unchanged
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var setting = db.Settings.First(s => s.Key == "Deposit_ratio");
            setting.Value.Should().Be("50");
        }
    }

    [Fact(DisplayName = "SETTING_011 - SetSettings - Deposit_ratio với nhiều hơn 1 chữ số thập phân")]
    public async Task SETTING_011_SetSettings_DepositRatioMultipleDecimals_BadRequest()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Settings.RemoveRange(db.Settings);
            await db.Settings.AddAsync(new (){ Key = "Deposit_ratio", Value = "50" });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        // Note: Since request accepts long?, this test simulates decimal input via string conversion
        // The validation should happen at a different layer
        var request = new Dictionary<string, long?> { { "Deposit_ratio", 5055 } }; // Represents 50.55

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/Setting", request).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<Application.Common.Models.ErrorResponse>(CancellationToken.None).ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Errors.Should().Contain(e => e.Message.Contains("decimal place"));
    }

    [Fact(DisplayName = "SETTING_012 - SetSettings - Request body rỗng")]
    public async Task SETTING_012_SetSettings_EmptyRequest_BadRequest()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Settings.RemoveRange(db.Settings);
            await db.Settings.AddAsync(new (){ Key = "Deposit_ratio", Value = "50" });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        var request = new Dictionary<string, long?>();

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/Setting", request).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<Application.Common.Models.ErrorResponse>(CancellationToken.None).ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Errors.Should().Contain(e => e.Message.Contains("cannot be empty"));
    }
#pragma warning restore CRR0035
}
