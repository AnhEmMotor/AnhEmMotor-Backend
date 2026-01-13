using Application.ApiContracts.ProductCategory.Responses;
using Application.Features.ProductCategories.Commands.CreateProductCategory;
using Application.Features.ProductCategories.Commands.DeleteManyProductCategories;
using Application.Features.ProductCategories.Commands.RestoreManyProductCategories;
using Application.Features.ProductCategories.Commands.UpdateProductCategory;
using Domain.Primitives;
using FluentAssertions;
using Infrastructure.DBContexts;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using ProductCategoryEntity = Domain.Entities.ProductCategory;

namespace IntegrationTests;

public class ProductCategory : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public ProductCategory(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

#pragma warning disable CRR0035
    [Fact(DisplayName = "PC_025 - Lấy danh sách danh mục sản phẩm thành công (cho mọi người dùng)")]
    public async Task GetProductCategories_WithPagination_ShouldReturnCorrectData()
    {
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.ProductCategories.RemoveRange(db.ProductCategories);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            var categories = new List<ProductCategoryEntity>();
            for(int i = 1; i <= 15; i++)
            {
                categories.Add(
                    new ProductCategoryEntity { Name = $"Category {i}", Description = "Desc", DeletedAt = null });
            }
            for(int i = 1; i <= 5; i++)
            {
                categories.Add(
                    new ProductCategoryEntity
                    {
                        Name = $"Deleted {i}",
                        Description = "Desc",
                        DeletedAt = DateTime.UtcNow
                    });
            }
            await db.ProductCategories.AddRangeAsync(categories, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var response = await _client.GetAsync("/api/v1/ProductCategory?Page=1&PageSize=10", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(10);
        content.TotalCount.Should().Be(15);
        content.TotalPages.Should().Be(2);
        content.PageNumber.Should().Be(1);
    }

    [Fact(DisplayName = "PC_026 - Lấy danh sách danh mục sản phẩm với phân trang")]
    public async Task GetProductCategories_SecondPage_ShouldReturnCorrectData()
    {
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.ProductCategories.RemoveRange(db.ProductCategories);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            var categories = new List<ProductCategoryEntity>();
            for(int i = 1; i <= 12; i++)
            {
                categories.Add(
                    new ProductCategoryEntity { Name = $"Category Page {i}", Description = "Desc", DeletedAt = null });
            }
            await db.ProductCategories.AddRangeAsync(categories, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var response = await _client.GetAsync("/api/v1/ProductCategory?Page=2&PageSize=5", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(5);
        content.TotalCount.Should().Be(12);
        content.TotalPages.Should().Be(3);
        content.PageNumber.Should().Be(2);
    }

    [Fact(DisplayName = "PC_027 - Lấy danh sách danh mục sản phẩm với filter theo Name")]
    public async Task GetProductCategories_WithFilter_ShouldReturnMatchingCategories()
    {
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.ProductCategories.RemoveRange(db.ProductCategories);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            await db.ProductCategories
                .AddRangeAsync(
                    new ProductCategoryEntity { Name = "Smartphone", Description = "Desc", DeletedAt = null },
                    new ProductCategoryEntity { Name = "Phone Case", Description = "Desc", DeletedAt = null },
                    new ProductCategoryEntity { Name = "Laptop", Description = "Desc", DeletedAt = null })
                .ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var response = await _client.GetAsync("/api/v1/ProductCategory?Filters=Name@=Phone", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(2);
        content.Items.Should().Contain(c => string.Compare(c.Name, "Smartphone") == 0);
        content.Items.Should().Contain(c => string.Compare(c.Name, "Phone Case") == 0);
    }

    [Fact(DisplayName = "PC_028 - Lấy danh sách danh mục sản phẩm với sorting")]
    public async Task GetProductCategories_WithSorting_ShouldReturnSortedCategories()
    {
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.ProductCategories.RemoveRange(db.ProductCategories);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            await db.ProductCategories
                .AddRangeAsync(
                    new ProductCategoryEntity { Name = "Zebra", Description = "Desc", DeletedAt = null },
                    new ProductCategoryEntity { Name = "Apple", Description = "Desc", DeletedAt = null },
                    new ProductCategoryEntity { Name = "Microsoft", Description = "Desc", DeletedAt = null })
                .ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var response = await _client.GetAsync("/api/v1/ProductCategory?Sorts=Name", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await     response.Content
            .ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(3);
        content.Items[0].Name.Should().Be("Apple");
        content.Items[1].Name.Should().Be("Microsoft");
        content.Items[2].Name.Should().Be("Zebra");
    }

    [Fact(DisplayName = "PC_029 - Lấy danh sách danh mục sản phẩm rỗng")]
    public async Task GetProductCategories_EmptyDatabase_ShouldReturnEmptyList()
    {
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.ProductCategories.RemoveRange(db.ProductCategories);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var response = await _client.GetAsync("/api/v1/ProductCategory?Page=1&PageSize=10", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().BeEmpty();
        content.TotalCount.Should().Be(0);
        content.TotalPages.Should().Be(0);
    }

    [Fact(DisplayName = "PC_030 - Lấy danh sách danh mục sản phẩm cho manager thành công")]
    public async Task GetProductCategoriesForManager_WithPermission_ShouldReturnCategories()
    {
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.ProductCategories.RemoveRange(db.ProductCategories);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            var categories = new List<ProductCategoryEntity>();
            for(int i = 1; i <= 8; i++)
            {
                categories.Add(
                    new ProductCategoryEntity { Name = $"Manager Category {i}", Description = "Desc", DeletedAt = null });
            }
            await db.ProductCategories.AddRangeAsync(categories, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var response = await _client.GetAsync(
            "/api/v1/ProductCategory/for-manager?Page=1&PageSize=10",
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(8);
        content.TotalCount.Should().Be(8);
    }

    [Fact(DisplayName = "PC_031 - Lấy danh sách danh mục sản phẩm đã xóa thành công")]
    public async Task GetDeletedProductCategories_ShouldReturnOnlyDeletedCategories()
    {
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.ProductCategories.RemoveRange(db.ProductCategories);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            var categories = new List<ProductCategoryEntity>();
            for(int i = 1; i <= 7; i++)
            {
                categories.Add(
                    new ProductCategoryEntity
                    {
                        Name = $"Deleted Category {i}",
                        Description = "Desc",
                        DeletedAt = DateTime.UtcNow
                    });
            }
            for(int i = 1; i <= 10; i++)
            {
                categories.Add(
                    new ProductCategoryEntity { Name = $"Active Category {i}", Description = "Desc", DeletedAt = null });
            }
            await db.ProductCategories.AddRangeAsync(categories, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var response = await _client.GetAsync(
            "/api/v1/ProductCategory/deleted?Page=1&PageSize=10",
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await     response.Content
            .ReadFromJsonAsync<PagedResult<ProductCategoryResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(7);
        content.TotalCount.Should().Be(7);
        content.Items.Should().OnlyContain(c => c.Name!.Contains("Deleted"));
    }

    [Fact(DisplayName = "PC_032 - Lấy chi tiết danh mục sản phẩm thành công")]
    public async Task GetProductCategoryById_ValidId_ShouldReturnCategoryWithProducts()
    {
        int categoryId;
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = new ProductCategoryEntity { Name = "Electronics", Description = "Desc", DeletedAt = null };
            await db.ProductCategories.AddAsync(category, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            categoryId = category.Id;
        }

        var response = await _client.GetAsync($"/api/v1/ProductCategory/{categoryId}", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<ProductCategoryResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Id.Should().Be(categoryId);
        content.Name.Should().Be("Electronics");
    }

    [Fact(DisplayName = "PC_033 - Lấy chi tiết danh mục sản phẩm không tồn tại")]
    public async Task GetProductCategoryById_InvalidId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/api/v1/ProductCategory/999").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PC_034 - Lấy chi tiết danh mục sản phẩm đã bị xóa")]
    public async Task GetProductCategoryById_DeletedCategory_ShouldReturnNotFound()
    {
        int categoryId;
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = new ProductCategoryEntity
            {
                Name = "Deleted",
                Description = "Desc",
                DeletedAt = DateTime.UtcNow
            };
            await db.ProductCategories.AddAsync(category, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            categoryId = category.Id;
        }

        var response = await _client.GetAsync($"/api/v1/ProductCategory/{categoryId}", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PC_035 - Tạo danh mục sản phẩm thành công qua API")]
    public async Task CreateProductCategory_ValidRequest_ShouldCreateCategory()
    {
        var request = new CreateProductCategoryCommand { Name = "API Test", Description = "Integration test" };

        var response = await _client.PostAsJsonAsync("/api/v1/ProductCategory", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content
            .ReadFromJsonAsync<ProductCategoryResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Name.Should().Be("API Test");
        content.Description.Should().Be("Integration test");
        content.Id.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "PC_036 - Cập nhật danh mục sản phẩm thành công qua API")]
    public async Task UpdateProductCategory_ValidRequest_ShouldUpdateCategory()
    {
        int categoryId;
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = new ProductCategoryEntity { Name = "Original", Description = "Keep", DeletedAt = null };
            await db.ProductCategories.AddAsync(category, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            categoryId = category.Id;
        }

        var request = new UpdateProductCategoryCommand { Name = "API Updated" };

        var response = await _client.PutAsJsonAsync($"/api/v1/ProductCategory/{categoryId}", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<ProductCategoryResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Name.Should().Be("API Updated");
        content.Description.Should().Be("Keep");
    }

    [Fact(DisplayName = "PC_037 - Xóa danh mục sản phẩm thành công qua API")]
    public async Task DeleteProductCategory_ValidId_ShouldDeleteCategory()
    {
        int categoryId;
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = new ProductCategoryEntity { Name = "To Delete API", Description = "Desc", DeletedAt = null };
            await db.ProductCategories.AddAsync(category, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            categoryId = category.Id;
        }

        var response = await _client.DeleteAsync($"/api/v1/ProductCategory/{categoryId}").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = await db.ProductCategories.FindAsync(categoryId, CancellationToken.None).ConfigureAwait(true);
            category.Should().NotBeNull();
            category!.DeletedAt.Should().NotBeNull();
        }
    }

    [Fact(DisplayName = "PC_038 - Xóa nhiều danh mục sản phẩm thành công")]
    public async Task DeleteManyProductCategories_ValidIds_ShouldDeleteAll()
    {
        int[] categoryIds = new int[3];
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            for(int i = 0; i < 3; i++)
            {
                var category = new ProductCategoryEntity
                {
                    Name = $"To Delete Many {i}",
                    Description = "Desc",
                    DeletedAt = null
                };
                await db.ProductCategories.AddAsync(category, CancellationToken.None).ConfigureAwait(true);
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

                categoryIds[i] = category.Id;
            }
        }

        var request = new DeleteManyProductCategoriesCommand { Ids = [ .. categoryIds ] };

        var response = await _client.PostAsJsonAsync("/api/v1/ProductCategory/delete-many", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            foreach(var id in categoryIds)
            {
                var category = await db.ProductCategories.FindAsync(id, CancellationToken.None).ConfigureAwait(true);
                category.Should().NotBeNull();
                category!.DeletedAt.Should().NotBeNull();
            }
        }
    }

    [Fact(DisplayName = "PC_039 - Xóa nhiều danh mục sản phẩm với một Id không hợp lệ")]
    public async Task DeleteManyProductCategories_WithInvalidId_ShouldNotDeleteAny()
    {
        int[] validIds = new int[2];
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            for(int i = 0; i < 2; i++)
            {
                var category = new ProductCategoryEntity { Name = $"Valid {i}", Description = "Desc", DeletedAt = null };
                await db.ProductCategories.AddAsync(category, CancellationToken.None).ConfigureAwait(true);
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

                validIds[i] = category.Id;
            }
        }

        var request = new DeleteManyProductCategoriesCommand { Ids = [ validIds[0], 999, validIds[1] ] };

        var response = await _client.PostAsJsonAsync("/api/v1/ProductCategory/delete-many", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            foreach(var id in validIds)
            {
                var category = await db.ProductCategories.FindAsync(id, CancellationToken.None).ConfigureAwait(true);
                category.Should().NotBeNull();
                category!.DeletedAt.Should().BeNull();
            }
        }
    }

    [Fact(DisplayName = "PC_040 - Xóa nhiều danh mục sản phẩm với một Id đã bị xóa")]
    public async Task DeleteManyProductCategories_WithDeletedId_ShouldNotDeleteAny()
    {
        int[] categoryIds = new int[2];
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

            var category1 = new ProductCategoryEntity { Name = "Active", Description = "Desc", DeletedAt = null };
            await db.ProductCategories.AddAsync(category1, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            categoryIds[0] = category1.Id;

            var category2 = new ProductCategoryEntity
            {
                Name = "Already Deleted",
                Description = "Desc",
                DeletedAt = DateTime.UtcNow
            };
            await db.ProductCategories.AddAsync(category2, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            categoryIds[1] = category2.Id;
        }

        var request = new DeleteManyProductCategoriesCommand { Ids = [ .. categoryIds ] };

        var response = await _client.PostAsJsonAsync("/api/v1/ProductCategory/delete-many", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = await db.ProductCategories
                .FindAsync(categoryIds[0], CancellationToken.None)
                .ConfigureAwait(true);
            category.Should().NotBeNull();
            category!.DeletedAt.Should().BeNull();
        }
    }

    [Fact(DisplayName = "PC_041 - Khôi phục nhiều danh mục sản phẩm thành công")]
    public async Task RestoreManyProductCategories_ValidIds_ShouldRestoreAll()
    {
        int[] categoryIds = new int[3];
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            for(int i = 0; i < 3; i++)
            {
                var category = new ProductCategoryEntity
                {
                    Name = $"To Restore {i}",
                    Description = "Desc",
                    DeletedAt = DateTime.UtcNow
                };
                await db.ProductCategories.AddAsync(category, CancellationToken.None).ConfigureAwait(true);
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

                categoryIds[i] = category.Id;
            }
        }

        var request = new RestoreManyProductCategoriesCommand { Ids = [ .. categoryIds ] };

        var response = await _client.PostAsJsonAsync("/api/v1/ProductCategory/restore-many", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<List<ProductCategoryResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Should().HaveCount(3);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            foreach(var id in categoryIds)
            {
                var category = await db.ProductCategories.FindAsync(id, CancellationToken.None).ConfigureAwait(true);
                category.Should().NotBeNull();
                category!.DeletedAt.Should().BeNull();
            }
        }
    }

    [Fact(DisplayName = "PC_042 - Khôi phục nhiều danh mục sản phẩm với một Id chưa bị xóa")]
    public async Task RestoreManyProductCategories_WithActiveId_ShouldNotRestoreAny()
    {
        int[] categoryIds = new int[2];
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

            var category1 = new ProductCategoryEntity
            {
                Name = "Deleted",
                Description = "Desc",
                DeletedAt = DateTime.UtcNow
            };
            await db.ProductCategories.AddAsync(category1, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            categoryIds[0] = category1.Id;

            var category2 = new ProductCategoryEntity { Name = "Active", Description = "Desc", DeletedAt = null };
            await db.ProductCategories.AddAsync(category2, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            categoryIds[1] = category2.Id;
        }

        var request = new RestoreManyProductCategoriesCommand { Ids = [ .. categoryIds ] };

        var response = await _client.PostAsJsonAsync("/api/v1/ProductCategory/restore-many", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var category = await db.ProductCategories
                .FindAsync(categoryIds[0], CancellationToken.None)
                .ConfigureAwait(true);
            category.Should().NotBeNull();
            category!.DeletedAt.Should().NotBeNull();
        }
    }
#pragma warning restore CRR0035
}
