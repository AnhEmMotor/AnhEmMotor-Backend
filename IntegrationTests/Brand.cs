using System.Net;
using System.Net.Http.Json;
using Application.ApiContracts.Brand.Responses;
using Application.Features.Brands.Commands.CreateBrand;
using Application.Features.Brands.Commands.DeleteManyBrands;
using Application.Features.Brands.Commands.RestoreManyBrands;
using Application.Features.Brands.Commands.UpdateBrand;
using Domain.Entities;
using Domain.Primitives;
using FluentAssertions;
using Infrastructure.DBContexts;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BrandEntities = Domain.Entities.Brand;

namespace IntegrationTests;

public class Brand : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public Brand(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

#pragma warning disable CRR0035
    [Fact(DisplayName = "BRAND_001 - CreateBrand - Success")]
    public async Task BRAND_001_CreateBrand_Success()
    {
        // Arrange
        var request = new CreateBrandCommand
        {
            Name = "Honda Integration",
            Description = "Integration Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Brand", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<BrandResponse>();
        content.Should().NotBeNull();
        content!.Name.Should().Be("Honda Integration");
    }

    [Fact(DisplayName = "BRAND_006 - GetBrands - Pagination")]
    public async Task BRAND_006_GetBrands_Pagination()
    {
        // Arrange
        // Seed 11 items
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            // Clear existing to ensure count is correct
            db.Brands.RemoveRange(db.Brands);
            await db.SaveChangesAsync();

            var brands = new List<BrandEntities>();
            for (int i = 1; i <= 11; i++)
            {
                brands.Add(new BrandEntities { Name = $"Brand {i}", Description = "Desc" });
            }
            await db.Brands.AddRangeAsync(brands);
            await db.SaveChangesAsync();
        }
        
        // Act
        var response = await _client.GetAsync("/api/v1/Brand?Page=1&PageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<BrandResponse>>();
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(10);
        content.TotalCount.Should().Be(11);
        content.TotalPages.Should().Be(2);
        content.PageNumber.Should().Be(1);
        content.PageSize.Should().Be(10);
    }

    [Fact(DisplayName = "BRAND_007 - GetBrands - FilterByName")]
    public async Task BRAND_007_GetBrands_FilterByName()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!db.Brands.Any(b => b.Name == "Honda Filter"))
            {
                db.Brands.Add(new BrandEntities { Name = "Honda Filter", Description = "Desc" });
                await db.SaveChangesAsync();
            }
        }

        // Act
        var response = await _client.GetAsync("/api/v1/Brand?Filters=Name@=Honda Filter");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<BrandResponse>>();
        content.Should().NotBeNull();
        content!.Items.Should().Contain(x => x.Name == "Honda Filter");
    }

    [Fact(DisplayName = "BRAND_008 - GetBrandById - Success")]
    public async Task BRAND_008_GetBrandById_Success()
    {
        // Arrange
        int id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var brand = new BrandEntities { Name = "Brand GetById", Description = "Desc" };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();
            id = brand.Id;
        }
        
        // Act
        var response = await _client.GetAsync($"/api/v1/Brand/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<BrandResponse>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(id);
    }

    [Fact(DisplayName = "BRAND_010 - UpdateBrand - Success")]
    public async Task BRAND_010_UpdateBrand_Success()
    {
        // Arrange
        int id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var brand = new BrandEntities { Name = "Brand Update", Description = "Desc" };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();
            id = brand.Id;
        }

        var request = new UpdateBrandCommand
        {
            Name = "Brand Updated",
            Description = "Updated Desc"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/Brand/{id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "BRAND_013 - DeleteBrand - Success")]
    public async Task BRAND_013_DeleteBrand_Success()
    {
        // Arrange
        int id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var brand = new BrandEntities { Name = "Brand Delete", Description = "Desc" };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();
            id = brand.Id;
        }

        // Act
        var response = await _client.DeleteAsync($"/api/v1/Brand/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "BRAND_015 - RestoreBrand - Success")]
    public async Task BRAND_015_RestoreBrand_Success()
    {
        // Arrange
        int id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var brand = new BrandEntities { Name = "Brand Restore", Description = "Desc", DeletedAt = DateTime.UtcNow };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();
            id = brand.Id;
        }

        // Act
        var response = await _client.PostAsync($"/api/v1/Brand/restore/{id}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "BRAND_016 - DeleteManyBrands - Success")]
    public async Task BRAND_016_DeleteManyBrands_Success()
    {
        // Arrange
        var ids = new List<int>();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var b1 = new BrandEntities { Name = "Brand DelMany 1", Description = "Desc" };
            var b2 = new BrandEntities { Name = "Brand DelMany 2", Description = "Desc" };
            db.Brands.AddRange(b1, b2);
            await db.SaveChangesAsync();
            ids.Add(b1.Id);
            ids.Add(b2.Id);
        }

        var request = new DeleteManyBrandsCommand { Ids = ids };

        // Act
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/Brand/delete-many")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _client.SendAsync(requestMessage);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "BRAND_017 - RestoreManyBrands - Success")]
    public async Task BRAND_017_RestoreManyBrands_Success()
    {
        // Arrange
        var ids = new List<int>();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var b1 = new BrandEntities { Name = "Brand ResMany 1", Description = "Desc", DeletedAt = DateTime.UtcNow };
            var b2 = new BrandEntities { Name = "Brand ResMany 2", Description = "Desc", DeletedAt = DateTime.UtcNow };
            db.Brands.AddRange(b1, b2);
            await db.SaveChangesAsync();
            ids.Add(b1.Id);
            ids.Add(b2.Id);
        }

        var request = new RestoreManyBrandsCommand { Ids = ids };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Brand/restore-many", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "BRAND_018 - GetDeletedBrands - Success")]
    public async Task BRAND_018_GetDeletedBrands_Success()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Brand/deleted");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "BRAND_049 - CreateBrand - CheckAuditing")]
    public async Task BRAND_049_CreateBrand_CheckAuditing()
    {
        // Arrange
        var request = new CreateBrandCommand { Name = "Audit Brand", Description = "Audit" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Brand", request);

        // Assert
        // Verify in DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var brand = db.Set<BrandEntities>().FirstOrDefault(b => b.Name == "Audit Brand");
        brand.Should().NotBeNull();
        brand!.CreatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "BRAND_050 - UpdateBrand - CheckAuditing")]
    public async Task BRAND_050_UpdateBrand_CheckAuditing()
    {
        int id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var brand = new BrandEntities { Name = "Audit Update", Description = "Audit" };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();
            id = brand.Id;
        }

        var request = new UpdateBrandCommand { Name = "Audit Update Changed", Description = "Audit" };

        var response = await _client.PutAsJsonAsync($"/api/v1/Brand/{id}", request);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var verifyBrand = verifyDb.Set<BrandEntities>().FirstOrDefault(b => b.Id == id);
        verifyBrand!.UpdatedAt.Should().NotBeNull();
    }
#pragma warning restore CRR0035
}
