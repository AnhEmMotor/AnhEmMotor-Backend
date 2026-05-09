using Application.ApiContracts.Maintenance.Responses;
using Domain.Constants.Permission;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
using ProductEntity = Domain.Entities.Product;
using LeadEntity = Domain.Entities.Lead;
using VehicleEntity = Domain.Entities.Vehicle;
using ProductCategoryEntity = Domain.Entities.ProductCategory;

namespace IntegrationTests;

public class VehicleAsset : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public VehicleAsset(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _output = output;
    }

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(true);
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    private async Task AuthenticateAsync()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [], // Add required permissions if needed, but for now assuming it's [Authorize] only
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
    }

    [Fact(DisplayName = "VAS_001 - Tạo mới tài sản xe thành công")]
    public async Task CreateAsset_ValidData_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var lead = new LeadEntity { FullName = "Customer 1", PhoneNumber = "0123456789" };
        var cat = new ProductCategoryEntity { Name = "Cat1" };
        db.Leads.Add(lead);
        db.ProductCategories.Add(cat);
        await db.SaveChangesAsync().ConfigureAwait(true);
        
        var product = new ProductEntity { Name = "Product 1", CategoryId = cat.Id, StatusId = "for-sale" };
        db.Products.Add(product);
        await db.SaveChangesAsync().ConfigureAwait(true);

        var payload = new
        {
            lead_id = lead.Id,
            product_id = product.Id,
            vin_number = "VIN_VAS_001",
            engine_number = "ENG_VAS_001",
            license_plate = "59-X1 12345"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/vehicle", payload).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var asset = await response.Content.ReadFromJsonAsync<VehicleResponse>().ConfigureAwait(true);
        asset.Should().NotBeNull();
        asset!.VinNumber.Should().Be("VIN_VAS_001");
        asset.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = "VAS_002a - Chặn trùng lặp số khung (VIN) - Trùng khớp hoàn toàn")]
    public async Task CreateAsset_DuplicateVin_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var lead = new LeadEntity { FullName = "Customer 1", PhoneNumber = "0123456789" };
        var cat = new ProductCategoryEntity { Name = "Cat1" };
        db.Leads.Add(lead);
        db.ProductCategories.Add(cat);
        await db.SaveChangesAsync().ConfigureAwait(true);
        
        var product = new ProductEntity { Name = "Product 1", CategoryId = cat.Id, StatusId = "for-sale" };
        db.Products.Add(product);
        await db.SaveChangesAsync().ConfigureAwait(true);
        
        var vin = "VIN123456";
        var existingVehicle = new VehicleEntity { LeadId = lead.Id, ProductId = product.Id, VinNumber = vin, EngineNumber = "ENG1" };
        db.Vehicles.Add(existingVehicle);
        await db.SaveChangesAsync().ConfigureAwait(true);

        var payload = new
        {
            lead_id = lead.Id,
            product_id = product.Id,
            vin_number = vin,
            engine_number = "ENG2"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/vehicle", payload).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        content.Should().Contain("VIN already exists");
    }

    [Fact(DisplayName = "VAS_002b - Chặn trùng lặp số khung (VIN) - Trùng lặp có khoảng trắng")]
    public async Task CreateAsset_DuplicateVinWithWhitespace_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var lead = new LeadEntity { FullName = "Customer 1", PhoneNumber = "0123456789" };
        var cat = new ProductCategoryEntity { Name = "Cat1" };
        db.Leads.Add(lead);
        db.ProductCategories.Add(cat);
        await db.SaveChangesAsync().ConfigureAwait(true);
        
        var product = new ProductEntity { Name = "Product 1", CategoryId = cat.Id, StatusId = "for-sale" };
        db.Products.Add(product);
        await db.SaveChangesAsync().ConfigureAwait(true);
        
        var vin = "VIN_WS_123";
        var existingVehicle = new VehicleEntity { LeadId = lead.Id, ProductId = product.Id, VinNumber = vin, EngineNumber = "ENG1" };
        db.Vehicles.Add(existingVehicle);
        await db.SaveChangesAsync().ConfigureAwait(true);

        var payload = new
        {
            lead_id = lead.Id,
            product_id = product.Id,
            vin_number = $"  {vin}  ", // Lead/Trailing whitespace
            engine_number = "ENG2"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/vehicle", payload).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        content.Should().Contain("VIN already exists");
    }

    [Fact(DisplayName = "VAS_005 - Gán tài sản cho khách hàng không tồn tại")]
    public async Task CreateAsset_NonExistentCustomer_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var cat = new ProductCategoryEntity { Name = "Cat1" };
        db.ProductCategories.Add(cat);
        await db.SaveChangesAsync().ConfigureAwait(true);
        var product = new ProductEntity { Name = "Product 1", CategoryId = cat.Id, StatusId = "for-sale" };
        db.Products.Add(product);
        await db.SaveChangesAsync().ConfigureAwait(true);
        
        var payload = new
        {
            lead_id = 999999, // Non-existent
            product_id = product.Id,
            vin_number = "VIN_VAS_005",
            engine_number = "ENG_VAS_005"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/vehicle", payload).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Match(s => (int)s == 400 || (int)s == 404); 
    }

    [Fact(DisplayName = "VAS_006 - Truy vấn tài sản theo số khung (VIN)")]
    public async Task GetVehicles_ByVin_ReturnsSpecificVehicle()
    {
        // Arrange
        await AuthenticateAsync();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var lead = new LeadEntity { FullName = "Customer 1", PhoneNumber = "0123456789" };
        db.Leads.Add(lead);
        await db.SaveChangesAsync().ConfigureAwait(true);
        
        var vin = "RLH123456";
        var v1 = new VehicleEntity { LeadId = lead.Id, VinNumber = vin, EngineNumber = "E1" };
        var v2 = new VehicleEntity { LeadId = lead.Id, VinNumber = "OTHER", EngineNumber = "E2" };
        db.Vehicles.AddRange(v1, v2);
        await db.SaveChangesAsync().ConfigureAwait(true);

        // Act
        var response = await _client.GetAsync($"/api/v1/vehicle?search={vin}").ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var assets = await response.Content.ReadFromJsonAsync<List<VehicleResponse>>().ConfigureAwait(true);
        assets.Should().HaveCount(1);
        assets![0].VinNumber.Should().Be(vin);
    }

    [Fact(DisplayName = "VAS_007 - Kiểm tra tính toàn vẹn khi đổi chủ sở hữu")]
    public async Task TransferOwnership_ValidRequest_UpdatesLeadId()
    {
        // Arrange
        await AuthenticateAsync();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var lead1 = new LeadEntity { FullName = "Owner 1", PhoneNumber = "1" };
        var lead2 = new LeadEntity { FullName = "Owner 2", PhoneNumber = "2" };
        db.Leads.AddRange(lead1, lead2);
        await db.SaveChangesAsync().ConfigureAwait(true);
        
        var vehicle = new VehicleEntity { LeadId = lead1.Id, VinNumber = "VIN_TRANSFER", EngineNumber = "E_TRANSFER" };
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync().ConfigureAwait(true);

        var payload = new { new_lead_id = lead2.Id };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/vehicle/{vehicle.Id}/transfer", payload).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedAsset = await response.Content.ReadFromJsonAsync<VehicleResponse>().ConfigureAwait(true);
        updatedAsset!.LeadId.Should().Be(lead2.Id);
    }
}
