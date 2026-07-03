using Application.ApiContracts.Auth.Responses;
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
using ProductCategoryEntity = Domain.Entities.ProductCategory;
using ProductEntity = Domain.Entities.Product;

namespace IntegrationTests;

using System.Threading.Tasks;
using Xunit;

public class ProductCategory : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
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

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(true);
        GC.SuppressFinalize(this);
    }

    #pragma warning disable IDE0079
    #pragma warning disable CRR0035
    [Fact(DisplayName = "PC_025 - Lấy danh sách danh mục sản phẩm thành công (cho mọi người dùng)")]
    public async Task GetProductCategories_WithPagination_ShouldReturnCorrectData()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var categories = new List<ProductCategoryEntity>();
            for (int i = 1; i <= 15; i++)
            {
                categories.Add(
                    new ProductCategoryEntity
                    {
                        Name = $"Category_{uniqueId}_{i}",
                        Description = "Desc",
                        DeletedAt = null
                    });
            }
            await db.ProductCategories.AddRangeAsync(categories, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
        var response = await _client.GetAsync(
            $"/api/v1/ProductCategory?Page=1&PageSize=10&Filters=Name@={uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
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
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var categories = new List<ProductCategoryEntity>();
            for (int i = 1; i <= 12; i++)
            {
                categories.Add(
                    new ProductCategoryEntity
                    {
                        Name = $"Category_{uniqueId}_{i}",
                        Description = "Desc",
                        DeletedAt = null
                    });
            }
            await db.ProductCategories.AddRangeAsync(categories, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
        var response = await _client.GetAsync(
            $"/api/v1/ProductCategory?Page=2&PageSize=5&Filters=Name@={uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
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
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await db.ProductCategories
                .AddRangeAsync(
                    new ProductCategoryEntity
                    {
                        Name = $"SmartPhone_{uniqueId}",
                        Description = "Desc",
                        DeletedAt = null
                    },
                    new ProductCategoryEntity
                    {
                        Name = $"Phone Case_{uniqueId}",
                        Description = "Desc",
                        DeletedAt = null
                    },
                    new ProductCategoryEntity { Name = $"Laptop_{uniqueId}", Description = "Desc", DeletedAt = null })
                .ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
        var response = await _client.GetAsync(
            $"/api/v1/ProductCategory?Filters=Name@=Phone,Name@={uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        var items = content.Items?.Where(x => x.Name!.Contains(uniqueId)).ToList();
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
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await db.ProductCategories
                .AddRangeAsync(
                    new ProductCategoryEntity { Name = $"Zebra_{uniqueId}", Description = "Desc", DeletedAt = null },
                    new ProductCategoryEntity { Name = $"Apple_{uniqueId}", Description = "Desc", DeletedAt = null },
                    new ProductCategoryEntity { Name = $"Microsoft_{uniqueId}", Description = "Desc", DeletedAt = null })
                .ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
        var response = await _client.GetAsync(
            $"/api/v1/ProductCategory?Sorts=Name&Filters=Name@={uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().HaveCount(3);
        content.Items[0].Name.Should().StartWith("Apple");
        content.Items[1].Name.Should().StartWith("Microsoft");
        content.Items[2].Name.Should().StartWith("Zebra");
    }

    [Fact(DisplayName = "PC_029 - Lấy danh sách danh mục không tồn tại (Search không có kết quả)")]
    public async Task GetProductCategories_NoResult_WhenFilterMatchesNothing()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.GetAsync(
            $"/api/v1/ProductCategory?Filters=Name@=NonExist{uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
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
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var categories = new List<ProductCategoryEntity>();
            for (int i = 1; i <= 5; i++)
            {
                categories.Add(
                    new ProductCategoryEntity
                    {
                        Name = $"Manager_Cat_{uniqueId}_{i}",
                        Description = "Desc",
                        DeletedAt = null
                    });
            }
            await db.ProductCategories.AddRangeAsync(categories, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
        var response = await _client.GetAsync(
            $"/api/v1/ProductCategory/for-manager?Page=1&PageSize=10&Filters=Name@={uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content.Items?.Count.Should().BeGreaterThanOrEqualTo(5);
        content.Items.Should().Contain(c => c.Name!.Contains($"Manager_Cat_{uniqueId}"));
    }

    [Fact(DisplayName = "PC_031 - Lấy danh sách danh mục sản phẩm đã xóa thành công")]
    public async Task GetDeletedProductCategories_ShouldReturnOnlyDeletedCategories()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var categories = new List<ProductCategoryEntity>();
            for (int i = 1; i <= 5; i++)
            {
                categories.Add(
                    new ProductCategoryEntity
                    {
                        Name = $"Deleted_{uniqueId}_{i}",
                        Description = "Desc",
                        DeletedAt = DateTime.UtcNow
                    });
            }
            categories.Add(
                new ProductCategoryEntity { Name = $"Active_{uniqueId}", Description = "Desc", DeletedAt = null });
            await db.ProductCategories.AddRangeAsync(categories, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
        var response = await _client.GetAsync(
            $"/api/v1/ProductCategory/deleted?Page=1&PageSize=10&Filters=Name@={uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
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
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        int categoryId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = new ProductCategoryEntity
            {
                Name = $"Detail_{uniqueId}",
                Description = "Desc",
                DeletedAt = null
            };
            await db.ProductCategories.AddAsync(category, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            categoryId = category.Id;
        }
        var response = await _client.GetAsync($"/api/v1/ProductCategory/{categoryId}", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<ProductCategoryResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
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
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.GetAsync("/api/v1/ProductCategory/99999", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PC_034 - Lấy chi tiết danh mục sản phẩm đã bị xóa")]
    public async Task GetProductCategoryById_DeletedCategory_ShouldReturnNotFound()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        int categoryId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = new ProductCategoryEntity
            {
                Name = $"Deleted_{uniqueId}",
                Description = "Desc",
                DeletedAt = DateTime.UtcNow
            };
            await db.ProductCategories.AddAsync(category, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            categoryId = category.Id;
        }
        var response = await _client.GetAsync($"/api/v1/ProductCategory/{categoryId}", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PC_035 - Tạo danh mục sản phẩm thành công qua API")]
    public async Task CreateProductCategory_ValidRequest_ShouldCreateCategory()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.Create],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var request = new CreateProductCategoryCommand
        {
            Name = $"API_Test_{uniqueId}",
            Description = "Integration test",
            ManagementType = "vin_number"
        };
        var response = await _client.PostAsJsonAsync("/api/v1/ProductCategory", request).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response!.Content
            .ReadFromJsonAsync<ProductCategoryResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
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
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.Edit],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        int categoryId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = new ProductCategoryEntity
            {
                Name = $"Original_{uniqueId}",
                Description = "Keep",
                ManagementType = "vin_number"
            };
            await db.ProductCategories.AddAsync(category, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            categoryId = category.Id;
        }
        var request = new UpdateProductCategoryCommand { Name = $"Updated_{uniqueId}" };
        var response = await _client.PutAsJsonAsync($"/api/v1/ProductCategory/{categoryId}", request)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<ProductCategoryResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Name.Should().Be($"Updated_{uniqueId}");
    }

    [Fact(DisplayName = "PC_037 - Xóa danh mục sản phẩm thành công qua API")]
    public async Task DeleteProductCategory_ValidId_ShouldDeleteCategory()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.Delete],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        int categoryId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = new ProductCategoryEntity
            {
                Name = $"Delete_{uniqueId}",
                Description = "Desc",
                DeletedAt = null
            };
            await db.ProductCategories.AddAsync(category, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            categoryId = category.Id;
        }
        var response = await _client.DeleteAsync($"/api/v1/ProductCategory/{categoryId}", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = await db.ProductCategories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == categoryId, CancellationToken.None)
                .ConfigureAwait(true);
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
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.Delete],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        int[] categoryIds = new int[3];
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            for (int i = 0; i < 3; i++)
            {
                var category = new ProductCategoryEntity
                {
                    Name = $"DelMany_{uniqueId}_{i}",
                    Description = "Desc",
                    DeletedAt = null
                };
                await db.ProductCategories.AddAsync(category, CancellationToken.None).ConfigureAwait(true);
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
                categoryIds[i] = category.Id;
            }
        }
        var request = new DeleteManyProductCategoriesCommand { Ids = [.. categoryIds] };
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/ProductCategory/delete-many")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _client.SendAsync(httpRequest, CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            foreach (var id in categoryIds)
            {
                var category = await db.ProductCategories
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == id, CancellationToken.None)
                    .ConfigureAwait(true);
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
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.Delete],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        int[] validIds = new int[2];
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            for (int i = 0; i < 2; i++)
            {
                var category = new ProductCategoryEntity
                {
                    Name = $"InvalidDel_{uniqueId}_{i}",
                    Description = "Desc",
                    DeletedAt = null
                };
                await db.ProductCategories.AddAsync(category, CancellationToken.None).ConfigureAwait(true);
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
                validIds[i] = category.Id;
            }
        }
        var request = new DeleteManyProductCategoriesCommand { Ids = [validIds[0], 99999, validIds[1]] };
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/ProductCategory/delete-many")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _client.SendAsync(httpRequest, CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            foreach (var id in validIds)
            {
                var category = await db.ProductCategories
                    .FindAsync([id], TestContext.Current.CancellationToken)
                    .ConfigureAwait(true);
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
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.Delete],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        int[] categoryIds = new int[2];
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var cat1 = new ProductCategoryEntity { Name = $"Active_{uniqueId}", Description = "Desc", DeletedAt = null };
            await db.ProductCategories.AddAsync(cat1, CancellationToken.None).ConfigureAwait(true);
            var cat2 = new ProductCategoryEntity
            {
                Name = $"AlreadyDel_{uniqueId}",
                Description = "Desc",
                DeletedAt = DateTime.UtcNow
            };
            await db.ProductCategories.AddAsync(cat2, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            categoryIds[0] = cat1.Id;
            categoryIds[1] = cat2.Id;
        }
        var request = new DeleteManyProductCategoriesCommand { Ids = [.. categoryIds] };
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/ProductCategory/delete-many")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _client.SendAsync(httpRequest, CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = await db.ProductCategories
                .FindAsync([categoryIds[0]], TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
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
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.Delete],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        int[] categoryIds = new int[3];
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            for (int i = 0; i < 3; i++)
            {
                var category = new ProductCategoryEntity
                {
                    Name = $"Restore_{uniqueId}_{i}",
                    Description = "Desc",
                    DeletedAt = DateTime.UtcNow
                };
                await db.ProductCategories.AddAsync(category, CancellationToken.None).ConfigureAwait(true);
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
                categoryIds[i] = category.Id;
            }
        }
        var request = new RestoreManyProductCategoriesCommand { Ids = [.. categoryIds] };
        var response = await _client.PostAsJsonAsync("/api/v1/ProductCategory/restore-many", request)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            foreach (var id in categoryIds)
            {
                var category = await db.ProductCategories
                    .FindAsync([id], TestContext.Current.CancellationToken)
                    .ConfigureAwait(true);
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
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.Delete],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        int[] categoryIds = new int[2];
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var cat1 = new ProductCategoryEntity
            {
                Name = $"Deleted_{uniqueId}",
                Description = "Desc",
                DeletedAt = DateTime.UtcNow
            };
            await db.ProductCategories.AddAsync(cat1, CancellationToken.None).ConfigureAwait(true);
            var cat2 = new ProductCategoryEntity { Name = $"Active_{uniqueId}", Description = "Desc", DeletedAt = null };
            await db.ProductCategories.AddAsync(cat2, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            categoryIds[0] = cat1.Id;
            categoryIds[1] = cat2.Id;
        }
        var request = new RestoreManyProductCategoriesCommand { Ids = [.. categoryIds] };
        var response = await _client.PostAsJsonAsync("/api/v1/ProductCategory/restore-many", request)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = await db.ProductCategories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == categoryIds[0], CancellationToken.None)
                .ConfigureAwait(true);
            category.Should().NotBeNull();
            category!.DeletedAt.Should().NotBeNull();
        }
    }

    [Fact(DisplayName = "PC_061 - Lấy danh mục sản phẩm bao gồm thông tin cha")]
    public async Task GetProductCategories_ShouldIncludeParentInfo()
    {
        int parentId;
        var uniqueId = Guid.NewGuid().ToString();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var parent = new ProductCategoryEntity { Name = $"Parent {uniqueId}" };
            db.ProductCategories.Add(parent);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            parentId = parent.Id;
            var child = new ProductCategoryEntity { Name = $"Child {uniqueId}", ParentId = parentId };
            db.ProductCategories.Add(child);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
        var response = await _client.GetAsync(
            $"/api/v1/ProductCategory?Filters=Name@={uniqueId}",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var childCat = content!.Items!.FirstOrDefault(c => c.Name!.Contains("Child"));
        childCat.Should().NotBeNull();
        childCat!.ParentId.Should().Be(parentId);
    }

    [Fact(DisplayName = "PC_062 - Lấy danh sách toàn bộ để build cấu trúc cây")]
    public async Task GetAllCategories_ShouldReturnAllForTreeStructure()
    {
        var response = await _client.GetAsync(
            "/api/v1/ProductCategory?PageSize=1000",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content!.Items.Should().NotBeNull();
        foreach (var item in content.Items.Where(i => i.ParentId.HasValue))
        {
            content.Items.Any(i => i.Id == item.ParentId).Should().BeTrue();
        }
    }

    [Fact(DisplayName = "PC_063 - Sửa danh mục sản phẩm: Chuyển sang danh mục cha khác")]
    public async Task UpdateProductCategory_ChangeParent_ShouldSucceed()
    {
        var uniqueAuthId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueAuthId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View, Permissions.Warehouse.ProductManagement.Edit, Permissions.Warehouse.ProductManagement.Create],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            new { UsernameOrEmail = username, Password = password, RememberMe = true })
            .ConfigureAwait(true);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await loginResponse.Content
            .ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
        int childId;
        int newParentId;
        var dataId = Guid.NewGuid().ToString("N")[..8];
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var oldParent = new ProductCategoryEntity
            {
                Name = $"OldParent {dataId}",
                Slug = $"old-parent-{dataId}",
                ManagementType = "vin_number"
            };
            var newParent = new ProductCategoryEntity
            {
                Name = $"NewParent {dataId}",
                Slug = $"new-parent-{dataId}",
                ManagementType = "vin_number"
            };
            db.ProductCategories.AddRange(oldParent, newParent);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            newParentId = newParent.Id;
            var child = new ProductCategoryEntity
            {
                Name = $"Child {dataId}",
                ParentId = oldParent.Id,
                Slug = $"child-{dataId}"
            };
            db.ProductCategories.Add(child);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            childId = child.Id;
        }
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/ProductCategory/{childId}",
            new
            {
                id = childId,
                name = $"UpdatedChild{dataId}",
                parentId = newParentId,
                categoryGroup = "Product",
                slug = $"child-updated-{dataId}",
                isActive = true
            })
            .ConfigureAwait(true);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = await response.Content
                .ReadAsStringAsync(TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            _output.WriteLine($"Update Failed: {error}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var updatedCat = await db.ProductCategories
                .FindAsync([childId], TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            updatedCat.Should().NotBeNull();
            updatedCat!.ParentId.Should().Be(newParentId);
        }
    }

    [Fact(DisplayName = "PC_064 - Sửa danh mục sản phẩm: Chuyển từ cha A sang cha B")]
    public async Task UpdateProductCategory_SwitchParent_ShouldSucceed()
    {
        var uniqueAuthId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueAuthId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View, Permissions.Warehouse.ProductManagement.Edit],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            new { UsernameOrEmail = username, Password = password })
            .ConfigureAwait(true);
        var loginResult = await loginResponse.Content
            .ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
        int childId, parentBId;
        var dataId = Guid.NewGuid().ToString("N")[..8];
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var parentA = new ProductCategoryEntity
            {
                Name = $"ParentA{dataId}",
                Slug = $"pa-{dataId}",
                ManagementType = "vin_number"
            };
            var parentB = new ProductCategoryEntity
            {
                Name = $"ParentB{dataId}",
                Slug = $"pb-{dataId}",
                ManagementType = "vin_number"
            };
            db.ProductCategories.AddRange(parentA, parentB);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            parentBId = parentB.Id;
            var child = new ProductCategoryEntity
            {
                Name = $"Child{dataId}",
                ParentId = parentA.Id,
                Slug = $"c-{dataId}"
            };
            db.ProductCategories.Add(child);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            childId = child.Id;
        }
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/ProductCategory/{childId}",
            new
            {
                id = childId,
                name = $"ChildUpdated{dataId}",
                parentId = parentBId,
                categoryGroup = "Product",
                slug = $"c-up-{dataId}",
                isActive = true
            })
            .ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var updatedCat = await db.ProductCategories
                .FindAsync([childId], TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            updatedCat!.ParentId.Should().Be(parentBId);
        }
    }

    private async Task AuthenticateAsync(List<string> permissions)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            permissions,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
    }

    [Fact(DisplayName = "PC_065 - Xóa danh mục cha khi còn danh mục con (Không có sản phẩm) - Thành công")]
    public async Task DeleteParentCategory_WithChildren_NoProducts_ShouldSucceed()
    {
        await AuthenticateAsync([Permissions.Warehouse.ProductManagement.Delete, Permissions.Warehouse.ProductManagement.View]).ConfigureAwait(true);
        int parentId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var parent = new ProductCategoryEntity { Name = "Parent" };
            db.ProductCategories.Add(parent);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            parentId = parent.Id;
            var child = new ProductCategoryEntity { Name = "Child", ParentId = parentId };
            db.ProductCategories.Add(child);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
        var response = await _client.DeleteAsync(
            $"/api/v1/ProductCategory/{parentId}",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var parent = await db.ProductCategories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == parentId, TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            parent!.DeletedAt.Should().NotBeNull();
            var children = await db.ProductCategories
                .IgnoreQueryFilters()
                .Where(c => c.ParentId == parentId)
                .ToListAsync(TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            children.Should().AllSatisfy(c => c.DeletedAt.Should().NotBeNull());
        }
    }

    [Fact(DisplayName = "PC_066 - Sửa danh mục cha thành chính nó - Thất bại")]
    public async Task UpdateCategory_ParentAsSelf_ShouldFail()
    {
        await AuthenticateAsync([Permissions.Warehouse.ProductManagement.Edit, Permissions.Warehouse.ProductManagement.View]).ConfigureAwait(true);
        int categoryId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = new ProductCategoryEntity { Name = "Category", Slug = "category-slug" };
            db.ProductCategories.Add(category);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            categoryId = category.Id;
        }
        var request = new UpdateProductCategoryCommand { Id = categoryId, ParentId = categoryId };
        var response = await HttpClientJsonExtensions.PutAsJsonAsync(
            _client,
            $"/api/v1/ProductCategory/{categoryId}",
            request,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "PC_067 - Sửa danh mục cha thành con của chính nó - Thất bại")]
    public async Task UpdateCategory_ParentAsChild_ShouldFail()
    {
        await AuthenticateAsync([Permissions.Warehouse.ProductManagement.Edit, Permissions.Warehouse.ProductManagement.View]).ConfigureAwait(true);
        int parentId, childId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var parent = new ProductCategoryEntity { Name = "Parent", Slug = "parent-slug" };
            db.ProductCategories.Add(parent);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            parentId = parent.Id;
            var child = new ProductCategoryEntity { Name = "Child", ParentId = parentId, Slug = "child-slug" };
            db.ProductCategories.Add(child);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            childId = child.Id;
        }
        var request = new UpdateProductCategoryCommand { Id = parentId, ParentId = childId };
        var response = await HttpClientJsonExtensions.PutAsJsonAsync(
            _client,
            $"/api/v1/ProductCategory/{parentId}",
            request,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "PC_068 - Xóa danh mục cấp 1 có sản phẩm ở danh mục con (cấp 2) - Thất bại")]
    public async Task DeleteLevel1_WithProductsInLevel2_ShouldFail()
    {
        await AuthenticateAsync([Permissions.Warehouse.ProductManagement.Delete, Permissions.Warehouse.ProductManagement.View]).ConfigureAwait(true);
        int parentId, childId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var parent = new ProductCategoryEntity { Name = "L1" };
            db.ProductCategories.Add(parent);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            parentId = parent.Id;
            var child = new ProductCategoryEntity { Name = "L2", ParentId = parentId };
            db.ProductCategories.Add(child);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            childId = child.Id;
            var product = new ProductEntity { Name = "Product", CategoryId = childId, StatusId = "for-sale" };
            db.Products.Add(product);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
        var response = await _client.DeleteAsync(
            $"/api/v1/ProductCategory/{parentId}",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "PC_069 - Xóa danh mục cấp 2 đang có sản phẩm - Thất bại")]
    public async Task DeleteLevel2_WithProducts_ShouldFail()
    {
        await AuthenticateAsync([Permissions.Warehouse.ProductManagement.Delete, Permissions.Warehouse.ProductManagement.View]).ConfigureAwait(true);
        int childId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var parent = new ProductCategoryEntity { Name = "L1" };
            db.ProductCategories.Add(parent);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            var child = new ProductCategoryEntity { Name = "L2", ParentId = parent.Id };
            db.ProductCategories.Add(child);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            childId = child.Id;
            var product = new ProductEntity { Name = "Product", CategoryId = childId, StatusId = "for-sale" };
            db.Products.Add(product);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
        var response = await _client.DeleteAsync(
            $"/api/v1/ProductCategory/{childId}",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "PC_070 - Xóa nhiều danh mục, hỗn hợp thành công và thất bại - Phải Rollback tất cả")]
    public async Task DeleteMany_MixedResults_ShouldRollbackAll()
    {
        await AuthenticateAsync([Permissions.Warehouse.ProductManagement.Delete, Permissions.Warehouse.ProductManagement.View]).ConfigureAwait(true);
        int cat1Id, cat2Id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var cat1 = new ProductCategoryEntity { Name = "Cat1" };
            db.ProductCategories.Add(cat1);
            var cat2 = new ProductCategoryEntity { Name = "Cat2" };
            db.ProductCategories.Add(cat2);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            cat1Id = cat1.Id;
            cat2Id = cat2.Id;
            var product = new ProductEntity { Name = "Product", CategoryId = cat2Id, StatusId = "for-sale" };
            db.Products.Add(product);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
        var request = new DeleteManyProductCategoriesCommand { Ids = [cat1Id, cat2Id] };
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/ProductCategory/delete-many")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _client.SendAsync(httpRequest, TestContext.Current.CancellationToken).ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var cat1 = await db.ProductCategories
                .FindAsync([cat1Id], TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            cat1!.DeletedAt.Should().BeNull();
            var cat2 = await db.ProductCategories
                .FindAsync([cat2Id], TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            cat2!.DeletedAt.Should().BeNull();
        }
    }
    #pragma warning restore CRR0035
}
