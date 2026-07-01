using Application.ApiContracts.Brand.Responses;
using Application.Features.Brands.Commands.CreateBrand;
using Application.Features.Brands.Commands.DeleteManyBrands;
using Application.Features.Brands.Commands.RestoreManyBrands;
using Application.Features.Brands.Commands.UpdateBrand;
using Domain.Constants.Permission;
using Domain.Primitives;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BrandEntities = Domain.Entities.Brand;

namespace IntegrationTests;

using System.Threading.Tasks;
using Xunit;

public class Brand : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public Brand(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    #pragma warning disable IDE0079 
    #pragma warning disable CRR0035
    [Fact(DisplayName = "BRAND_001 - CreateBrand - Success")]
    public async Task BRAND_001_CreateBrand_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.BrandManagement.Create],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        var command = new CreateBrandCommand { Name = "Honda Integration", Description = "Integration Test" };
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/brand");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(command);
        var response = await _client.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(true);
        if (response!.StatusCode == HttpStatusCode.Unauthorized)
        {
            var errorContent = await response!.Content.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
            throw new Exception($"API returned 401. Response Body: {errorContent}");
        }
        if (response!.StatusCode == HttpStatusCode.InternalServerError)
        {
            var errorContent = await response!.Content.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
            throw new Exception($"API returned 500. Response Body: {errorContent}");
        }
        response!.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseData = await response!.Content
            .ReadFromJsonAsync<BrandResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        responseData.Should().NotBeNull();
        responseData!.Name.Should().Be("Honda Integration");
    }

    [Fact(DisplayName = "BRAND_006 - GetBrands - Pagination")]
    public async Task BRAND_006_GetBrands_Pagination()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Brands.RemoveRange(db.Brands);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            var brands = new List<BrandEntities>();
            for (int i = 1; i <= 11; i++)
            {
                brands.Add(new BrandEntities { Name = $"Brand {i}", Description = "Desc" });
            }
            await db.Brands.AddRangeAsync(brands, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
        var response = await _client.GetAsync("/api/v1/Brand?Page=1&PageSize=10", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<BrandResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().HaveCount(10);
        content.TotalCount.Should().Be(11);
        content.TotalPages.Should().Be(2);
        content.PageNumber.Should().Be(1);
        content.PageSize.Should().Be(10);
    }

    [Fact(DisplayName = "BRAND_007 - GetBrands - FilterByName")]
    public async Task BRAND_007_GetBrands_FilterByName()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!db.Brands.Any(b => b.Name == "Honda Filter"))
            {
                db.Brands.Add(new BrandEntities { Name = "Honda Filter", Description = "Desc" });
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            }
        }
        var response = await _client.GetAsync("/api/v1/Brand?Filters=Name@=Honda Filter", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<BrandResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().Contain(x => string.Compare(x.Name, "Honda Filter") == 0);
    }

    [Fact(DisplayName = "BRAND_007b - GetBrands - FilterByOrigin")]
    public async Task BRAND_007b_GetBrands_FilterByOrigin()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!db.Brands.Any(b => b.Name == "Honda Japan Filter"))
            {
                db.Brands.Add(new BrandEntities { Name = "Honda Japan Filter", Origin = "Japan", Description = "Desc" });
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            }
        }
        var response = await _client.GetAsync("/api/v1/Brand?Filters=Origin@=Japan", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<BrandResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().Contain(x => string.Compare(x.Name, "Honda Japan Filter") == 0);
    }

    [Fact(DisplayName = "BRAND_008 - GetBrandById - Success")]
    public async Task BRAND_008_GetBrandById_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.BrandManagement.View],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        int id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var brand = new BrandEntities { Name = "Brand GetById", Description = "Desc" };
            db.Brands.Add(brand);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            id = brand.Id;
        }
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/Brand/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<BrandResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Id.Should().Be(id);
    }

    [Fact(DisplayName = "BRAND_010 - UpdateBrand - Success")]
    public async Task BRAND_010_UpdateBrand_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.BrandManagement.Edit],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        int id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var brand = new BrandEntities { Name = "Brand Update", Description = "Desc" };
            db.Brands.Add(brand);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            id = brand.Id;
        }
        var request = new UpdateBrandCommand { Name = "Brand Updated", Description = "Updated Desc" };
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/Brand/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(request);
        var response = await _client.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "BRAND_010b - UpdateBrand - SameName - Success")]
    public async Task BRAND_010b_UpdateBrand_SameName_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.BrandManagement.Edit],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        int id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var brand = new BrandEntities { Name = "Brand Same Name", Description = "Desc" };
            db.Brands.Add(brand);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            id = brand.Id;
        }
        var request = new UpdateBrandCommand { Name = "Brand Same Name", Description = "Updated Desc" };
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/Brand/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(request);
        var response = await _client.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "BRAND_013 - DeleteBrand - Success")]
    public async Task BRAND_013_DeleteBrand_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.BrandManagement.Delete],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        int id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var brand = new BrandEntities { Name = "Brand Delete", Description = "Desc" };
            db.Brands.Add(brand);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            id = brand.Id;
        }
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/Brand/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "BRAND_015 - RestoreBrand - Success")]
    public async Task BRAND_015_RestoreBrand_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.BrandManagement.Delete],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        int id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var brand = new BrandEntities { Name = "Brand Restore", Description = "Desc", DeletedAt = DateTime.UtcNow };
            db.Brands.Add(brand);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            id = brand.Id;
        }
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/Brand/restore/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "BRAND_016 - DeleteManyBrands - Success")]
    public async Task BRAND_016_DeleteManyBrands_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.BrandManagement.Delete],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        var ids = new List<int>();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var b1 = new BrandEntities { Name = "Brand DelMany 1", Description = "Desc" };
            var b2 = new BrandEntities { Name = "Brand DelMany 2", Description = "Desc" };
            db.Brands.AddRange(b1, b2);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            ids.Add(b1.Id);
            ids.Add(b2.Id);
        }
        var request = new DeleteManyBrandsCommand { Ids = ids };
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/Brand/delete-many");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(request);
        var response = await _client.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "BRAND_017 - RestoreManyBrands - Success")]
    public async Task BRAND_017_RestoreManyBrands_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.BrandManagement.Delete],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        var ids = new List<int>();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var b1 = new BrandEntities { Name = "Brand ResMany 1", Description = "Desc", DeletedAt = DateTime.UtcNow };
            var b2 = new BrandEntities { Name = "Brand ResMany 2", Description = "Desc", DeletedAt = DateTime.UtcNow };
            db.Brands.AddRange(b1, b2);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            ids.Add(b1.Id);
            ids.Add(b2.Id);
        }
        var request = new RestoreManyBrandsCommand { Ids = ids };
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Brand/restore-many");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(request);
        var response = await _client.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "BRAND_018 - GetDeletedBrands - Success")]
    public async Task BRAND_018_GetDeletedBrands_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.BrandManagement.Delete],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v1/brand/deleted");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(true);
        if (response!.StatusCode == HttpStatusCode.Unauthorized)
        {
            var errorContent = await response!.Content.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
            throw new Exception($"API returned 401. Response Body: {errorContent}");
        }
        if (response!.StatusCode == HttpStatusCode.InternalServerError)
        {
            var errorContent = await response!.Content.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
            throw new Exception($"API returned 500. Response Body: {errorContent}");
        }
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "BRAND_049 - CreateBrand - CheckAuditing")]
    public async Task BRAND_049_CreateBrand_CheckAuditing()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.BrandManagement.Create],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        var request = new CreateBrandCommand { Name = "Audit Brand", Description = "Audit" };
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Brand");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(request);
        await _client.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var brand = db.Set<BrandEntities>().FirstOrDefault(b => b.Name == "Audit Brand");
        brand.Should().NotBeNull();
        brand!.CreatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "BRAND_050 - UpdateBrand - CheckAuditing")]
    public async Task BRAND_050_UpdateBrand_CheckAuditing()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.BrandManagement.Edit],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        int id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var brand = new BrandEntities { Name = "Audit Update", Description = "Audit" };
            db.Brands.Add(brand);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            id = brand.Id;
        }
        var request = new UpdateBrandCommand { Name = "Audit Update Changed", Description = "Audit" };
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/Brand/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(request);
        await _client.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(true);
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var verifyBrand = verifyDb.Set<BrandEntities>().FirstOrDefault(b => b.Id == id);
        verifyBrand!.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "BRAND_052 - GetBrandStatistics - Success")]
    public async Task BRAND_052_GetBrandStatistics_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.BrandManagement.View],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Brands.RemoveRange(db.Brands);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            var b1 = new BrandEntities { Name = "Brand One", Origin = "Japan" };
            var b2 = new BrandEntities { Name = "Brand Two", Origin = "Japan" };
            var b3 = new BrandEntities { Name = "Brand Three", Origin = "Italy" };
            var b4 = new BrandEntities { Name = "Brand Four", Origin = "Japan", DeletedAt = DateTime.UtcNow };
            db.Brands.AddRange(b1, b2, b3, b4);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            var testNow = DateTimeOffset.UtcNow;
            await db.Database
                .ExecuteSqlRawAsync(
                    "UPDATE \"Brand\" SET \"CreatedAt\" = {0}, \"UpdatedAt\" = {1} WHERE \"Id\" = {2}",
                    testNow.AddMinutes(-20),
                    testNow.AddMinutes(-10),
                    b1.Id)
                .ConfigureAwait(true);
            await db.Database
                .ExecuteSqlRawAsync(
                    "UPDATE \"Brand\" SET \"CreatedAt\" = {0}, \"UpdatedAt\" = {1} WHERE \"Id\" = {2}",
                    testNow.AddMinutes(-20),
                    testNow.AddMinutes(-5),
                    b2.Id)
                .ConfigureAwait(true);
            await db.Database
                .ExecuteSqlRawAsync(
                    "UPDATE \"Brand\" SET \"CreatedAt\" = {0}, \"UpdatedAt\" = NULL WHERE \"Id\" = {1}",
                    testNow.AddMinutes(-20),
                    b3.Id)
                .ConfigureAwait(true);
        }
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v1/brand/statistics");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats = await response.Content
            .ReadFromJsonAsync<BrandStatisticsResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        stats.Should().NotBeNull();
        stats!.TotalBrands.Should().Be(3);
        stats.PopularOrigin.Should().Be("Japan");
        stats.PopularOriginCount.Should().Be(2);
        stats.LatestUpdatedBrandName.Should().Be("Brand Two");
    }

    [Fact(DisplayName = "BRAND_053 - GetBrandStatistics - LatestTime Comparison CreatedAt vs UpdatedAt")]
    public async Task BRAND_053_GetBrandStatistics_LatestTime_Comparison_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.BrandManagement.View],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        var now = DateTimeOffset.UtcNow;
        var t1 = now.AddHours(-10);
        var t2 = now.AddHours(-8);
        var t3 = now.AddHours(-5);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Brands.RemoveRange(db.Brands);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            var brandA = new BrandEntities { Name = "Brand A", Origin = "Japan" };
            var brandB = new BrandEntities { Name = "Brand B", Origin = "Japan" };
            db.Brands.AddRange(brandA, brandB);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            await db.Database
                .ExecuteSqlRawAsync(
                    "UPDATE \"Brand\" SET \"CreatedAt\" = {0}, \"UpdatedAt\" = {1} WHERE \"Id\" = {2}",
                    t1,
                    t2,
                    brandA.Id)
                .ConfigureAwait(true);
            await db.Database
                .ExecuteSqlRawAsync(
                    "UPDATE \"Brand\" SET \"CreatedAt\" = {0}, \"UpdatedAt\" = NULL WHERE \"Id\" = {1}",
                    t3,
                    brandB.Id)
                .ConfigureAwait(true);
        }
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v1/brand/statistics");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats = await response.Content
            .ReadFromJsonAsync<BrandStatisticsResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        stats.Should().NotBeNull();
        stats!.LatestUpdatedBrandName.Should().Be("Brand B");
        stats.LatestUpdatedAt.Should().BeCloseTo(t3, TimeSpan.FromSeconds(5));
        var t4 = now.AddHours(-1);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var existingA = db.Brands.First(b => b.Name == "Brand A");
            await db.Database
                .ExecuteSqlRawAsync("UPDATE \"Brand\" SET \"UpdatedAt\" = {0} WHERE \"Id\" = {1}", t4, existingA.Id)
                .ConfigureAwait(true);
        }
        var requestMessage2 = new HttpRequestMessage(HttpMethod.Get, "/api/v1/brand/statistics");
        requestMessage2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response2 = await _client.SendAsync(requestMessage2, CancellationToken.None).ConfigureAwait(true);
        response2!.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats2 = await response2.Content
            .ReadFromJsonAsync<BrandStatisticsResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        stats2.Should().NotBeNull();
        stats2!.LatestUpdatedBrandName.Should().Be("Brand A");
        stats2.LatestUpdatedAt.Should().BeCloseTo(t4, TimeSpan.FromSeconds(5));
    }
    #pragma warning restore CRR0035
}

