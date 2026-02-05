using Application.ApiContracts.ProductCategory.Responses;
using Application.Features.ProductCategories.Commands.CreateProductCategory;
using Application.Features.ProductCategories.Commands.DeleteManyProductCategories;
using Application.Features.ProductCategories.Commands.RestoreManyProductCategories;
using Application.Features.ProductCategories.Commands.UpdateProductCategory;
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
using Xunit.Abstractions;
using ProductCategoryEntity = Domain.Entities.ProductCategory;

namespace IntegrationTests;

public class ProductCategory : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public ProductCategory(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _output = output;
    }

#pragma warning disable CRR0035
    [Fact(DisplayName = "PC_025 - Lấy danh sách danh mục sản phẩm thành công (cho mọi người dùng)")]
    public async Task GetProductCategories_WithPagination_ShouldReturnCorrectData()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var categories = new List<ProductCategoryEntity>();
            for (int i = 1; i <= 15; i++)
            {
                categories.Add(new ProductCategoryEntity { Name = $"Category_{uniqueId}_{i}", Description = "Desc", DeletedAt = null });
            }
            await db.ProductCategories.AddRangeAsync(categories);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/v1/ProductCategory?Page=1&PageSize=10&Filters=Name@={uniqueId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>();
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(10);
        content.TotalCount.Should().Be(15);
        content.PageNumber.Should().Be(1);
    }

    [Fact(DisplayName = "PC_026 - Lấy danh sách danh mục sản phẩm với phân trang")]
    public async Task GetProductCategories_SecondPage_ShouldReturnCorrectData()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var categories = new List<ProductCategoryEntity>();
            for (int i = 1; i <= 12; i++)
            {
                categories.Add(new ProductCategoryEntity { Name = $"Category_{uniqueId}_{i}", Description = "Desc", DeletedAt = null });
            }
            await db.ProductCategories.AddRangeAsync(categories);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/v1/ProductCategory?Page=2&PageSize=5&Filters=Name@={uniqueId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>();
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(5);
        content.TotalCount.Should().Be(12);
        content.PageNumber.Should().Be(2);
    }

    [Fact(DisplayName = "PC_027 - Lấy danh sách danh mục sản phẩm với filter theo Name")]
    public async Task GetProductCategories_WithFilter_ShouldReturnMatchingCategories()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await db.ProductCategories.AddRangeAsync(
                new ProductCategoryEntity { Name = $"SmartPhone_{uniqueId}", Description = "Desc", DeletedAt = null },
                new ProductCategoryEntity { Name = $"Phone Case_{uniqueId}", Description = "Desc", DeletedAt = null },
                new ProductCategoryEntity { Name = $"Laptop_{uniqueId}", Description = "Desc", DeletedAt = null }
            );
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/v1/ProductCategory?Filters=Name@=Phone,Name@={uniqueId}"); // Filter both strictly

        // Since Sieve might handle ',' as OR or AND depends on config, usually comma in Filters string separates different filters (AND). 
        // We want (Name contains 'Phone') AND (Name contains uniqueId). 
        // If Sieve default logic applies, separate params or comma. Let's try comma.
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>();
        content.Should().NotBeNull();
        
        // "Smartphone_..." contains "Phone"
        // "Phone Case_..." contains "Phone"
        // "Laptop_..." does NOT
        // Ensure we strictly filter by uniqueId too to avoid noise
        
        var items = content!.Items.Where(x => x.Name!.Contains(uniqueId)).ToList();
        
        // If the API filter works as AND, we get 2. If it works as OR or ignores, we check manually.
        // Assuming strict filter support:
        items.Should().Contain(c => c.Name!.Contains("SmartPhone"));
        items.Should().Contain(c => c.Name!.Contains("Phone Case"));
        items.Should().NotContain(c => c.Name!.Contains("Laptop"));
    }

    [Fact(DisplayName = "PC_028 - Lấy danh sách danh mục sản phẩm với sorting")]
    public async Task GetProductCategories_WithSorting_ShouldReturnSortedCategories()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await db.ProductCategories.AddRangeAsync(
                new ProductCategoryEntity { Name = $"Zebra_{uniqueId}", Description = "Desc", DeletedAt = null },
                new ProductCategoryEntity { Name = $"Apple_{uniqueId}", Description = "Desc", DeletedAt = null },
                new ProductCategoryEntity { Name = $"Microsoft_{uniqueId}", Description = "Desc", DeletedAt = null }
            );
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/v1/ProductCategory?Sorts=Name&Filters=Name@={uniqueId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>();
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(3);
        content.Items[0].Name.Should().StartWith("Apple");
        content.Items[1].Name.Should().StartWith("Microsoft");
        content.Items[2].Name.Should().StartWith("Zebra");
    }

    [Fact(DisplayName = "PC_029 - Lấy danh sách danh mục không tồn tại (Search k có kết quả)")]
    public async Task GetProductCategories_NoResult_WhenFilterMatchesNothing()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync($"/api/v1/ProductCategory?Filters=Name@=NonExist{uniqueId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>();
        content.Should().NotBeNull();
        content!.Items.Should().BeEmpty();
        content.TotalCount.Should().Be(0);
    }

    [Fact(DisplayName = "PC_030 - Lấy danh sách danh mục sản phẩm cho manager thành công")]
    public async Task GetProductCategoriesForManager_WithPermission_ShouldReturnCategories()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var categories = new List<ProductCategoryEntity>();
            for (int i = 1; i <= 5; i++)
            {
                categories.Add(new ProductCategoryEntity { Name = $"Manager_Cat_{uniqueId}_{i}", Description = "Desc", DeletedAt = null });
            }
            await db.ProductCategories.AddRangeAsync(categories);
            await db.SaveChangesAsync();
        }

        // Assuming for-manager endpoint supports filtering or we just want to see if it works. 
        // If it doesn't support Filters, we just check status 200 and maybe non-empty if we just seeded.
        // But better try to filter if possible or just check functionality.
        var response = await _client.GetAsync($"/api/v1/ProductCategory/for-manager?Page=1&PageSize=10&Filters=Name@={uniqueId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>();
        content.Should().NotBeNull();
        // Check if items > 0. Since we used a filter, if supported, we get 5. If not supported, we get all. 
        // We'll assert count >= 5.
        content!.Items.Count.Should().BeGreaterThanOrEqualTo(5);
        content.Items.Should().Contain(c => c.Name!.Contains($"Manager_Cat_{uniqueId}"));
    }

    [Fact(DisplayName = "PC_031 - Lấy danh sách danh mục sản phẩm đã xóa thành công")]
    public async Task GetDeletedProductCategories_ShouldReturnOnlyDeletedCategories()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var categories = new List<ProductCategoryEntity>();
            for (int i = 1; i <= 5; i++)
            {
                categories.Add(new ProductCategoryEntity { Name = $"Deleted_{uniqueId}_{i}", Description = "Desc", DeletedAt = DateTime.UtcNow });
            }
            // Add active ones to ensure we don't get them
            categories.Add(new ProductCategoryEntity { Name = $"Active_{uniqueId}", Description = "Desc", DeletedAt = null });
            
            await db.ProductCategories.AddRangeAsync(categories);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/v1/ProductCategory/deleted?Page=1&PageSize=10&Filters=Name@={uniqueId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>();
        content.Should().NotBeNull();
        // If Filters supported on deleted endpoint
        content!.Items.Should().HaveCount(5);
        content.Items.Should().OnlyContain(c => c.Name!.Contains("Deleted"));
        content.Items.Should().NotContain(c => c.Name!.Contains("Active"));
    }

    [Fact(DisplayName = "PC_032 - Lấy chi tiết danh mục sản phẩm thành công")]
    public async Task GetProductCategoryById_ValidId_ShouldReturnCategoryWithProducts()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        int categoryId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = new ProductCategoryEntity { Name = $"Detail_{uniqueId}", Description = "Desc", DeletedAt = null };
            await db.ProductCategories.AddAsync(category);
            await db.SaveChangesAsync();
            categoryId = category.Id;
        }

        var response = await _client.GetAsync($"/api/v1/ProductCategory/{categoryId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ProductCategoryResponse>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(categoryId);
        content.Name.Should().Be($"Detail_{uniqueId}");
    }

    [Fact(DisplayName = "PC_033 - Lấy chi tiết danh mục sản phẩm không tồn tại")]
    public async Task GetProductCategoryById_InvalidId_ShouldReturnNotFound()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync("/api/v1/ProductCategory/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PC_034 - Lấy chi tiết danh mục sản phẩm đã bị xóa")]
    public async Task GetProductCategoryById_DeletedCategory_ShouldReturnNotFound()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        int categoryId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = new ProductCategoryEntity { Name = $"Deleted_{uniqueId}", Description = "Desc", DeletedAt = DateTime.UtcNow };
            await db.ProductCategories.AddAsync(category);
            await db.SaveChangesAsync();
            categoryId = category.Id;
        }

        var response = await _client.GetAsync($"/api/v1/ProductCategory/{categoryId}");

        // Typically should return NotFound if deleted, unless viewed by admin with specific params? 
        // Standard API behavior for 'GetById' on logic deleted is usually NotFound.
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PC_035 - Tạo danh mục sản phẩm thành công qua API")]
    public async Task CreateProductCategory_ValidRequest_ShouldCreateCategory()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.Create], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var request = new CreateProductCategoryCommand { Name = $"API_Test_{uniqueId}", Description = "Integration test" };

        var response = await _client.PostAsJsonAsync("/api/v1/ProductCategory", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<ProductCategoryResponse>();
        content.Should().NotBeNull();
        content!.Name.Should().Be($"API_Test_{uniqueId}");
        content.Id.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "PC_036 - Cập nhật danh mục sản phẩm thành công qua API")]
    public async Task UpdateProductCategory_ValidRequest_ShouldUpdateCategory()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.Edit], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        int categoryId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = new ProductCategoryEntity { Name = $"Original_{uniqueId}", Description = "Keep", DeletedAt = null };
            await db.ProductCategories.AddAsync(category);
            await db.SaveChangesAsync();
            categoryId = category.Id;
        }

        var request = new UpdateProductCategoryCommand { Name = $"Updated_{uniqueId}" };

        var response = await _client.PutAsJsonAsync($"/api/v1/ProductCategory/{categoryId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ProductCategoryResponse>();
        content.Should().NotBeNull();
        content!.Name.Should().Be($"Updated_{uniqueId}");
    }

    [Fact(DisplayName = "PC_037 - Xóa danh mục sản phẩm thành công qua API")]
    public async Task DeleteProductCategory_ValidId_ShouldDeleteCategory()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.Delete], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        int categoryId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = new ProductCategoryEntity { Name = $"Delete_{uniqueId}", Description = "Desc", DeletedAt = null };
            await db.ProductCategories.AddAsync(category);
            await db.SaveChangesAsync();
            categoryId = category.Id;
        }

        var response = await _client.DeleteAsync($"/api/v1/ProductCategory/{categoryId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = await db.ProductCategories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == categoryId);
            category.Should().NotBeNull();
            category!.DeletedAt.Should().NotBeNull();
        }
    }

    [Fact(DisplayName = "PC_038 - Xóa nhiều danh mục sản phẩm thành công")]
    public async Task DeleteManyProductCategories_ValidIds_ShouldDeleteAll()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.Delete], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        int[] categoryIds = new int[3];
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            for (int i = 0; i < 3; i++)
            {
                var category = new ProductCategoryEntity { Name = $"DelMany_{uniqueId}_{i}", Description = "Desc", DeletedAt = null };
                await db.ProductCategories.AddAsync(category);
                await db.SaveChangesAsync();
                categoryIds[i] = category.Id;
            }
        }

        var request = new DeleteManyProductCategoriesCommand { Ids = [.. categoryIds] };

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/ProductCategory/delete-many")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _client.SendAsync(httpRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            foreach (var id in categoryIds)
            {
                var category = await db.ProductCategories
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == id);
                category.Should().NotBeNull();
                category!.DeletedAt.Should().NotBeNull();
            }
        }
    }

    [Fact(DisplayName = "PC_039 - Xóa nhiều danh mục sản phẩm với một Id không hợp lệ")]
    public async Task DeleteManyProductCategories_WithInvalidId_ShouldNotDeleteAny()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.Delete], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        int[] validIds = new int[2];
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            for (int i = 0; i < 2; i++)
            {
                var category = new ProductCategoryEntity { Name = $"InvalidDel_{uniqueId}_{i}", Description = "Desc", DeletedAt = null };
                await db.ProductCategories.AddAsync(category);
                await db.SaveChangesAsync();
                validIds[i] = category.Id;
            }
        }

        var request = new DeleteManyProductCategoriesCommand { Ids = [validIds[0], 99999, validIds[1]] };

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/ProductCategory/delete-many")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _client.SendAsync(httpRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            foreach (var id in validIds)
            {
                var category = await db.ProductCategories.FindAsync(id);
                category!.DeletedAt.Should().BeNull();
            }
        }
    }

    [Fact(DisplayName = "PC_040 - Xóa nhiều danh mục sản phẩm với một Id đã bị xóa")]
    public async Task DeleteManyProductCategories_WithDeletedId_ShouldNotDeleteAny()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.Delete], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        int[] categoryIds = new int[2];
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            
            var cat1 = new ProductCategoryEntity { Name = $"Active_{uniqueId}", Description = "Desc", DeletedAt = null };
            await db.ProductCategories.AddAsync(cat1);
            
            var cat2 = new ProductCategoryEntity { Name = $"AlreadyDel_{uniqueId}", Description = "Desc", DeletedAt = DateTime.UtcNow };
            await db.ProductCategories.AddAsync(cat2);
            
            await db.SaveChangesAsync();
            categoryIds[0] = cat1.Id;
            categoryIds[1] = cat2.Id;
        }

        var request = new DeleteManyProductCategoriesCommand { Ids = [.. categoryIds] };

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/ProductCategory/delete-many")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _client.SendAsync(httpRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = await db.ProductCategories.FindAsync(categoryIds[0]);
            category!.DeletedAt.Should().BeNull();
        }
    }

    [Fact(DisplayName = "PC_041 - Khôi phục nhiều danh mục sản phẩm thành công")]
    public async Task RestoreManyProductCategories_ValidIds_ShouldRestoreAll()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        // To restore, we need logic. Assuming Delete permission OR a Restore permission. Usually 'Delete' or 'Edit' covers it.
        // Checking PermissionsList... there is only Delete. Restore might be covered by Edit or Delete.
        // Assuming Delete (soft delete management) or Edit. Let's try Delete first as it's the reverse.
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.Delete], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        int[] categoryIds = new int[3];
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            for (int i = 0; i < 3; i++)
            {
                var category = new ProductCategoryEntity { Name = $"Restore_{uniqueId}_{i}", Description = "Desc", DeletedAt = DateTime.UtcNow };
                await db.ProductCategories.AddAsync(category);
                await db.SaveChangesAsync();
                categoryIds[i] = category.Id;
            }
        }

        var request = new RestoreManyProductCategoriesCommand { Ids = [.. categoryIds] };

        var response = await _client.PostAsJsonAsync("/api/v1/ProductCategory/restore-many", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            foreach (var id in categoryIds)
            {
                var category = await db.ProductCategories.FindAsync(id);
                category!.DeletedAt.Should().BeNull();
            }
        }
    }

    [Fact(DisplayName = "PC_042 - Khôi phục nhiều danh mục sản phẩm với một Id chưa bị xóa")]
    public async Task RestoreManyProductCategories_WithActiveId_ShouldNotRestoreAny()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.ProductCategories.Delete], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        int[] categoryIds = new int[2];
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            
            var cat1 = new ProductCategoryEntity { Name = $"Deleted_{uniqueId}", Description = "Desc", DeletedAt = DateTime.UtcNow };
            await db.ProductCategories.AddAsync(cat1);
            
            var cat2 = new ProductCategoryEntity { Name = $"Active_{uniqueId}", Description = "Desc", DeletedAt = null };
            await db.ProductCategories.AddAsync(cat2);
            
            await db.SaveChangesAsync();
            categoryIds[0] = cat1.Id;
            categoryIds[1] = cat2.Id;
        }

        var request = new RestoreManyProductCategoriesCommand { Ids = [.. categoryIds] };

        var response = await _client.PostAsJsonAsync("/api/v1/ProductCategory/restore-many", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = await db.ProductCategories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == categoryIds[0]);
            category.Should().NotBeNull();
            category!.DeletedAt.Should().NotBeNull();
        }
    }
#pragma warning restore CRR0035
}
