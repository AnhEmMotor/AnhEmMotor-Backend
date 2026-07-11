using Application.ApiContracts.Vehicle.Responses;
using Domain.Primitives;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LeadEntity = Domain.Entities.Lead;
using ProductCategoryEntity = Domain.Entities.ProductCategory;
using ProductEntity = Domain.Entities.Product;
using VehicleEntity = Domain.Entities.Vehicle;

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

    [Fact(DisplayName = "VAS_001 - Tạo mới tài sản xe thành công")]
    public async Task CreateAsset_ValidData_ReturnsCreated()
    {
        await AuthenticateAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var lead = new LeadEntity { FullName = "Customer 1", PhoneNumber = "0123456789" };
        var cat = new ProductCategoryEntity { Name = "Cat1" };
        db.Leads.Add(lead);
        db.ProductCategories.Add(cat);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var product = new ProductEntity { Name = "Product 1", CategoryId = cat.Id, StatusId = "for-sale" };
        db.Products.Add(product);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var payload = new
        {
            lead_id = lead.Id,
            product_id = product.Id,
            vin_number = "VIN_VAS_001",
            engine_number = "ENG_VAS_001",
            license_plate = "59-X1 12345"
        };
        var response = await _client.PostAsJsonAsync("/api/v1/vehicle", payload).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.Created);
        var asset = await response!.Content
            .ReadFromJsonAsync<VehicleResponse>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        asset!.Should().NotBeNull();
        asset!.VinNumber.Should().Be("VIN_VAS_001");
        asset.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = "VAS_002a - Chặn trùng lặp số khung (VIN) - Trùng khớp hoàn toàn")]
    public async Task CreateAsset_DuplicateVin_ReturnsBadRequest()
    {
        await AuthenticateAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var lead = new LeadEntity { FullName = "Customer 1", PhoneNumber = "0123456789" };
        var cat = new ProductCategoryEntity { Name = "Cat1" };
        db.Leads.Add(lead);
        db.ProductCategories.Add(cat);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var product = new ProductEntity { Name = "Product 1", CategoryId = cat.Id, StatusId = "for-sale" };
        db.Products.Add(product);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var vin = "VIN123456";
        var existingVehicle = new VehicleEntity { LeadId = lead.Id, VinNumber = vin, EngineNumber = "ENG1" };
        db.Vehicles.Add(existingVehicle);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var payload = new { lead_id = lead.Id, product_id = product.Id, vin_number = vin, engine_number = "ENG2" };
        var response = await _client.PostAsJsonAsync("/api/v1/vehicle", payload).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response!.Content
            .ReadAsStringAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content!.Should().Contain("VIN already exists");
    }

    [Fact(DisplayName = "VAS_002b - Chặn trùng lặp số khung (VIN) - Trùng lặp có khoảng trắng")]
    public async Task CreateAsset_DuplicateVinWithWhitespace_ReturnsBadRequest()
    {
        await AuthenticateAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var lead = new LeadEntity { FullName = "Customer 1", PhoneNumber = "0123456789" };
        var cat = new ProductCategoryEntity { Name = "Cat1" };
        db.Leads.Add(lead);
        db.ProductCategories.Add(cat);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var product = new ProductEntity { Name = "Product 1", CategoryId = cat.Id, StatusId = "for-sale" };
        db.Products.Add(product);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var vin = "VIN_WS_123";
        var existingVehicle = new VehicleEntity { LeadId = lead.Id, VinNumber = vin, EngineNumber = "ENG1" };
        db.Vehicles.Add(existingVehicle);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var payload = new
        {
            lead_id = lead.Id,
            product_id = product.Id,
            vin_number = $"  {vin}  ",
            engine_number = "ENG2"
        };
        var response = await _client.PostAsJsonAsync("/api/v1/vehicle", payload).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response!.Content
            .ReadAsStringAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content!.Should().Contain("VIN already exists");
    }

    [Fact(DisplayName = "VAS_005 - Gán tài sản cho khách hàng không tồn tại")]
    public async Task CreateAsset_NonExistentCustomer_ReturnsBadRequest()
    {
        await AuthenticateAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = "Cat1" };
        db.ProductCategories.Add(cat);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var product = new ProductEntity { Name = "Product 1", CategoryId = cat.Id, StatusId = "for-sale" };
        db.Products.Add(product);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var payload = new
        {
            lead_id = 999999,
            product_id = product.Id,
            vin_number = "VIN_VAS_005",
            engine_number = "ENG_VAS_005"
        };
        var response = await _client.PostAsJsonAsync("/api/v1/vehicle", payload).ConfigureAwait(true);
        response!.StatusCode.Should().Match(s => (int?)s == 400 || (int?)s == 404);
    }

    [Fact(DisplayName = "VAS_006 - Truy vấn tài sản theo số khung (VIN)")]
    public async Task GetVehicles_ByVin_ReturnsSpecificVehicle()
    {
        await AuthenticateAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var lead = new LeadEntity { FullName = "Customer 1", PhoneNumber = "0123456789" };
        db.Leads.Add(lead);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var vin = "RLH123456";
        var v1 = new VehicleEntity { LeadId = lead.Id, VinNumber = vin, EngineNumber = "E1" };
        var v2 = new VehicleEntity { LeadId = lead.Id, VinNumber = "OTHER", EngineNumber = "E2" };
        db.Vehicles.AddRange(v1, v2);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var response = await _client.GetAsync(
            $"/api/v1/vehicle?Filters=search@={vin}",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResult = await response!.Content
            .ReadFromJsonAsync<PagedResult<VehicleResponse>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        pagedResult!.Items.Should().HaveCount(1);
        pagedResult.Items![0].VinNumber.Should().Be(vin);
    }

    [Fact(DisplayName = "VAS_007 - Kiểm tra tính toàn vẹn khi đổi chủ sở hữu")]
    public async Task TransferOwnership_ValidRequest_UpdatesLeadId()
    {
        await AuthenticateAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var lead1 = new LeadEntity { FullName = "Owner 1", PhoneNumber = "1" };
        var lead2 = new LeadEntity { FullName = "Owner 2", PhoneNumber = "2" };
        db.Leads.AddRange(lead1, lead2);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var vehicle = new VehicleEntity { LeadId = lead1.Id, VinNumber = "VIN_TRANSFER", EngineNumber = "E_TRANSFER" };
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var payload = new { new_lead_id = lead2.Id };
        var response = await _client.PostAsJsonAsync($"/api/v1/vehicle/{vehicle.Id}/transfer", payload)
            .ConfigureAwait(true);
        var errContent = await response.Content
            .ReadAsStringAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK, $"Error 500 content: {errContent}");
        var updatedAsset = await response!.Content
            .ReadFromJsonAsync<VehicleResponse>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        updatedAsset!.LeadId.Should().Be(lead2.Id);
    }

    [Fact(DisplayName = "VAS_010 - Client endpoint đăng ký và lấy thông tin xe")]
    public async Task ClientVehicleEndpoints_RegisterAndGetVehicles_ReturnsVehicleProfile()
    {
        await AuthenticateAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var payload = new
        {
            plate = "59A-12345",
            vin_number = "VIN_CLIENT_001",
            engine_number = "ENG_CLIENT_001",
            color = "Đỏ",
            purchase_date = DateTimeOffset.UtcNow,
            warranty_date = DateTimeOffset.UtcNow.AddYears(2),
            current_odometer = 1250
        };

        var registerResponse = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(
            _client,
            "/api/v1/client/vehicles/register-odo",
            payload,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync(
            "/api/v1/client/vehicles",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await getResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        content.Should().Contain("59A-12345");
        content.Should().Contain("VIN_CLIENT_001");
    }

    [Fact(DisplayName = "VAS_015 - Client endpoint lấy chi tiết xe bao gồm bảo hành và bảo trì")]
    public async Task ClientVehicleDetailEndpoint_ReturnsWarrantyAndMaintenanceInfo()
    {
        await AuthenticateAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var payload = new
        {
            plate = "59A-77777",
            vin_number = "VIN_CLIENT_DETAIL_001",
            engine_number = "ENG_CLIENT_DETAIL_001",
            color = "Xanh",
            purchase_date = DateTimeOffset.UtcNow,
            warranty_date = DateTimeOffset.UtcNow.AddMonths(6),
            current_odometer = 500
        };

        var registerResponse = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(
            _client,
            "/api/v1/client/vehicles/register-odo",
            payload,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdVehicle = await registerResponse.Content.ReadFromJsonAsync<VehicleResponse>(TestContext.Current.CancellationToken).ConfigureAwait(true);
        createdVehicle.Should().NotBeNull();

        var detailResponse = await _client.GetAsync($"/api/v1/client/vehicles/{createdVehicle!.Id}/detail", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var vehicleDetail = await detailResponse.Content.ReadFromJsonAsync<VehicleResponse>(TestContext.Current.CancellationToken).ConfigureAwait(true);

        vehicleDetail.Should().NotBeNull();
        vehicleDetail!.Id.Should().Be(createdVehicle.Id);
        vehicleDetail.WarrantyRemainingDays.Should().BeGreaterThan(0);
        vehicleDetail.MaintenanceStatus.Should().NotBeNull();
    }
}
