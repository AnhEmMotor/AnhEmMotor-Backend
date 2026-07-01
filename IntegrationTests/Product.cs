using Application.ApiContracts.Option.Responses;
using Application.ApiContracts.Product.Requests;
using Application.ApiContracts.Product.Responses;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.UpdateManyProductStatuses;
using Application.Features.Products.Commands.UpdateProduct;
using Domain.Constants.Permission;
using Domain.Entities;
using Domain.Primitives;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using BrandEntity = Domain.Entities.Brand;
using ProductCategoryEntity = Domain.Entities.ProductCategory;
using ProductEntity = Domain.Entities.Product;
using TechnologyEntity = Domain.Entities.Technology;

namespace IntegrationTests;

using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using ProductStatusConstants = Domain.Constants.Product.ProductStatus;

public class Product : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public Product(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
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

    #pragma warning disable IDE0079
    #pragma warning disable CRR0035
    [Fact(DisplayName = "PRODUCT_061 - Lấy danh sách sản phẩm với phân trang mặc định (10 items/page)")]
    public async Task GetProducts_DefaultPagination_Returns10ItemsPerPage()
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}", DeletedAt = null };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}", DeletedAt = null };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var products = Enumerable.Range(1, 15)
            .Select(
                i => new ProductEntity
                {
                    Name = $"Product_{uniqueId}_{i}",
                    CategoryId = category.Id,
                    BrandId = brand.Id,
                    StatusId = productStatusId,
                    DeletedAt = null
                })
            .ToList();
        db.Products.AddRange(products);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync("/api/v1/product", CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().HaveCountLessThanOrEqualTo(10);
        content.PageSize.Should().Be(10);
    }

    [Fact(DisplayName = "PRODUCT_062 - Lấy danh sách sản phẩm với Sieve filter theo BrandId")]
    public async Task GetProducts_FilterByBrandId_ReturnsFilteredProducts()
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}", DeletedAt = null };
        var brand1 = new BrandEntity { Name = $"Honda_{uniqueId}", DeletedAt = null };
        var brand2 = new BrandEntity { Name = $"Yamaha_{uniqueId}", DeletedAt = null };
        db.ProductCategories.Add(category);
        db.Brands.AddRange(brand1, brand2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var p1 = new ProductEntity
        {
            Name = $"P1_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand1.Id,
            StatusId = productStatusId
        };
        var p2 = new ProductEntity
        {
            Name = $"P2_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand2.Id,
            StatusId = productStatusId
        };
        db.Products.AddRange(p1, p2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync($"/api/v1/product?filters=BrandId=={brand1.Id}", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().ContainSingle(p => p.Id == p1.Id);
        content!.Items.Should().NotContain(p => p.Id == p2.Id);
    }

    [Fact(DisplayName = "PRODUCT_063 - Lấy danh sách sản phẩm với Sieve sort theo Name")]
    public async Task GetProducts_SortByName_ReturnsSortedProducts()
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}", DeletedAt = null };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}", DeletedAt = null };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var p1 = new ProductEntity
        {
            Name = $"A_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        var p2 = new ProductEntity
        {
            Name = $"B_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        var p3 = new ProductEntity
        {
            Name = $"C_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        db.Products.AddRange(p3, p1, p2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync("/api/v1/product?sorts=Name", CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        var expectedIds = new List<int?> { p1.Id, p2.Id, p3.Id };
        var createdItems = content!.Items!.Where(x => expectedIds.Contains(x.Id)).ToList();
        createdItems.Should().HaveCount(3);
        createdItems.Should().BeInAscendingOrder(p => p.Name);
    }

    [Fact(DisplayName = "PRODUCT_064 - Lấy danh sách sản phẩm chỉ trả về variants chưa bị xóa")]
    public async Task GetProducts_ReturnsOnlyNonDeletedVariants()
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}", DeletedAt = null };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}", DeletedAt = null };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var product = new ProductEntity
        {
            Name = $"Product_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId,
            DeletedAt = null
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var v1 = new ProductVariant
        {
            ProductId = product.Id,
            Price = 100,
            UrlSlug = $"v1_{uniqueId}",
            DeletedAt = null
        };
        var v2 = new ProductVariant
        {
            ProductId = product.Id,
            Price = 200,
            UrlSlug = $"v2_{uniqueId}",
            DeletedAt = DateTimeOffset.UtcNow
        };
        db.ProductVariants.AddRange(v1, v2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync($"/api/v1/product?filters=Id=={product.Id}", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        var item = content!.Items?.FirstOrDefault(p => p.Id == product.Id);
        item.Should().NotBeNull();
        item!.Variants.Should().Contain(v => v.Id == v1.Id);
        item.Variants.Should().NotContain(v => v.Id == v2.Id);
    }

    [Fact(DisplayName = "PRODUCT_065 - Lấy danh sách sản phẩm không hiển thị trường stock cho user thường")]
    public async Task GetProducts_NoPermission_HidesStockFields()
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}", DeletedAt = null };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}", DeletedAt = null };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var product = new ProductEntity
        {
            Name = $"Product_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync("/api/v1/product", CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_066 - Lấy danh sách sản phẩm for-manager hiển thị đầy đủ trường stock")]
    public async Task GetProductsForManager_WithPermission_ShowsStockFields()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View, Domain.Constants.Permission.Permissions.Warehouse.ReceiptManagement.View],
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}", DeletedAt = null };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}", DeletedAt = null };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var product = new ProductEntity
        {
            Name = $"Product_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync("/api/v1/product/for-manager", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_067 - Lấy danh sách sản phẩm deleted chỉ trả về sản phẩm đã xóa")]
    public async Task GetDeletedProducts_ReturnsOnlyDeletedProducts()
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var deletedProduct = new ProductEntity
        {
            Name = $"Deleted_Product_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId,
            DeletedAt = DateTimeOffset.UtcNow
        };
        db.Products.Add(deletedProduct);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync("/api/v1/product/deleted", CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().Contain(p => p.Id == deletedProduct.Id);
    }

    [Fact(
        DisplayName = "PRODUCT_069 - Lấy variants-lite/for-InventoryReceipt chỉ trả về Id, Name, CoverImageUrl, Price")]
    public async Task GetVariantsLiteForInventoryReceipt_ReturnsOnlyRequiredFields()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.Warehouse.ReceiptManagement.Edit],
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var product = new ProductEntity
        {
            Name = $"Product_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var variant = new ProductVariant { ProductId = product.Id, Price = 100, UrlSlug = $"v-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync(
            "/api/v1/product/variants-lite/for-InventoryReceipt",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductVariantLiteResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().Contain(v => v.Id == variant.Id);
    }

    [Fact(DisplayName = "PRODUCT_070 - Lấy variants-lite/for-output chỉ trả về Id, Name, CoverImageUrl, Price")]
    public async Task GetVariantsLiteForOutput_ReturnsOnlyRequiredFields()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.OutputManagement.Create],
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var product = new ProductEntity
        {
            Name = $"Product_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var variant = new ProductVariant { ProductId = product.Id, Price = 100, UrlSlug = $"v-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync("/api/v1/product/variants-lite/for-output", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductVariantLiteResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().Contain(v => v.Id == variant.Id);
    }

    [Fact(DisplayName = "PRODUCT_073 - Lấy variants theo ProductId chỉ trả về variants chưa xóa")]
    public async Task GetVariantsByProductId_ReturnsOnlyNonDeletedVariants()
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var product = new ProductEntity
        {
            Name = $"Product_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId,
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var v1 = new ProductVariant
        {
            ProductId = product.Id,
            UrlSlug = $"v1-{uniqueId}",
            Price = 100,
            DeletedAt = null
        };
        var v2 = new ProductVariant
        {
            ProductId = product.Id,
            UrlSlug = $"v2-{uniqueId}",
            Price = 200,
            DeletedAt = DateTimeOffset.UtcNow
        };
        db.ProductVariants.AddRange(v1, v2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync($"/api/v1/product/{product.Id}/variants-lite", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<List<ProductVariantLiteResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Should().Contain(v => v.Id == v1.Id);
        content!.Should().NotContain(v => v.Id == v2.Id);
    }

    [Fact(DisplayName = "PRODUCT_074 - Tạo sản phẩm tự động tạo OptionValue mới nếu chưa tồn tại")]
    public async Task CreateProduct_NewOptionValue_CreatesAutomatically()
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        db.PredefinedOptions.Add(new PredefinedOption { Key = $"Size_{uniqueId}", Value = "Kích cỡ" });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var payload = new
        {
            name = $"Product_{uniqueId}",
            category_id = category.Id,
            brand_id = brand.Id,
            status_id = "for-sale",
            variants = new[]
            {
                new
            {
                url_slug = $"slug-{uniqueId}",
                price = 20000000,
                variant_name = "V1",
                variantName = "V1",
                cover_image_url = "image.jpg",
                optionValues = new Dictionary<string, string> { { $"Size_{uniqueId}", "Green" } },
            }
            }
        };
        var response = await _client.PostAsJsonAsync("/api/v1/product", payload).ConfigureAwait(true);
        if (response!.StatusCode != HttpStatusCode.Created)
        {
            var error = await response!.Content.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
            _output.WriteLine($"PRODUCT_074 Failed: {response!.StatusCode} - {error}");
        }
        response!.StatusCode.Should().Be(HttpStatusCode.Created);
        var options = await db.Options
            .Where(o => o.Name == $"Size_{uniqueId}")
            .FirstOrDefaultAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        options.Should().NotBeNull();
        var values = await db.OptionValues
            .Where(ov => ov.OptionId == options!.Id && string.Compare(ov.Name, "Green") == 0)
            .FirstOrDefaultAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        values.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_075 - Tạo sản phẩm sử dụng OptionValue hiện có nếu đã tồn tại")]
    public async Task CreateProduct_ExistingOptionValue_UsesExisting()
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var optionName = $"Option_{uniqueId}";
        db.PredefinedOptions.Add(new PredefinedOption { Key = optionName, Value = "Tuỳ chọn" });
        var optionValueName = "Red";
        var option = new Option { Name = optionName };
        db.Options.Add(option);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var optionValue = new OptionValue { OptionId = option.Id, Name = optionValueName };
        db.OptionValues.Add(optionValue);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var payload = new
        {
            name = $"Product_{uniqueId}",
            category_id = category.Id,
            brand_id = brand.Id,
            status_id = "for-sale",
            variants = new[]
            {
                new
            {
                url_slug = $"slug-{uniqueId}",
                price = 20000000,
                variant_name = "V1",
                cover_image_url = "image.jpg",
                optionValues = new Dictionary<string, string> { { optionName, optionValueName } }
            }
            }
        };
        var response = await _client.PostAsJsonAsync("/api/v1/product", payload).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.Created);
        var count = await db.OptionValues
            .CountAsync(
                ov => ov.OptionId == option.Id && string.Compare(ov.Name, optionValueName) == 0,
                CancellationToken.None)
            .ConfigureAwait(true);
        count.Should().Be(1);
    }

    [Fact(DisplayName = "PRODUCT_076 - Tên biến thể hiển thị đúng khi có nhiều optionValues")]
    public async Task GetVariant_MultipleOptions_DisplaysCorrectName()
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var product = new ProductEntity
        {
            Name = $"Product_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var option1 = new Option { Name = "Color" };
        var option2 = new Option { Name = "Size" };
        db.Options.AddRange(option1, option2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var val1 = new OptionValue { OptionId = option1.Id, Name = "Red" };
        var val2 = new OptionValue { OptionId = option2.Id, Name = "XL" };
        db.OptionValues.AddRange(val1, val2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var variant = new ProductVariant { ProductId = product.Id, Price = 100, UrlSlug = $"v-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.VariantOptionValues.Add(new VariantOptionValue { VariantId = variant.Id, OptionValueId = val1.Id });
        db.VariantOptionValues.Add(new VariantOptionValue { VariantId = variant.Id, OptionValueId = val2.Id });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync($"/api/v1/product/{product.Id}/for-manager", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<ProductDetailForManagerResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        var v = content!.Variants.First(x => x.Id == variant.Id);
        v.OptionValues.Should().ContainKey("Color");
        v.OptionValues["Color"].Should().Be("Red");
        v.OptionValues.Should().ContainKey("Size");
        v.OptionValues["Size"].Should().Be("XL");
    }

    [Fact(DisplayName = "PRODUCT_077 - Tên biến thể để trống khi không có optionValues hợp lệ")]
    public async Task GetVariant_NoOptions_DisplaysEmptyVariantName()
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var product = new ProductEntity
        {
            Name = $"Simple_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var variant = new ProductVariant { ProductId = product.Id, UrlSlug = $"simple-{uniqueId}", Price = 100 };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync($"/api/v1/product/{product.Id}/variants-lite", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<List<ProductVariantLiteResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.First().VariantName.Should().BeNullOrEmpty();
    }

    [Fact(DisplayName = "PRODUCT_078 - Tạo sản phẩm với Price có 2 chữ số thập phân được lưu chính xác")]
    public async Task CreateProduct_DecimalPrice_SavesWithoutRounding()
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var payload = new
        {
            name = $"Decimal_{uniqueId}",
            category_id = category.Id,
            brand_id = brand.Id,
            status_id = "for-sale",
            variants = new[]
            {
                new
            {
                url_slug = $"dec-{uniqueId}",
                price = 20000000.99m,
                variant_name = "V1",
                cover_image_url = "image.jpg"
            }
            }
        };
        var response = await _client.PostAsJsonAsync("/api/v1/product", payload).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response!.Content
            .ReadFromJsonAsync<ProductDetailResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Variants[0].Price.Should().Be(20000000.99m);
    }

    [Fact(DisplayName = "PRODUCT_079 - Sửa nhiều sản phẩm với transaction - tất cả thành công hoặc tất cả fail")]
    public async Task UpdateManyProducts_Transaction_AllSucceedOrAllFail()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.ChangeStatus],
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        var outStockStatusId = ProductStatusConstants.OutOfBusiness;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, outStockStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = outStockStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var p1 = new ProductEntity
        {
            Name = $"P1_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        var p2 = new ProductEntity
        {
            Name = $"P2_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        db.Products.AddRange(p1, p2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var request = new UpdateManyProductStatusesCommand { Ids = [p1.Id, p2.Id], StatusId = outStockStatusId };
        var response = await _client.PatchAsJsonAsync("/api/v1/product/statuses", request, CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var dbP1 = await db.Products.FindAsync([p1.Id], TestContext.Current.CancellationToken).ConfigureAwait(true);
        var dbP2 = await db.Products.FindAsync([p2.Id], TestContext.Current.CancellationToken).ConfigureAwait(true);
        await db.Entry(dbP1!).ReloadAsync(CancellationToken.None).ConfigureAwait(true);
        await db.Entry(dbP2!).ReloadAsync(CancellationToken.None).ConfigureAwait(true);
        dbP1!.StatusId.Should().Be(outStockStatusId);
        dbP2!.StatusId.Should().Be(outStockStatusId);
    }

    [Fact(DisplayName = "PRODUCT_080 - Sửa nhiều sản phẩm với transaction - một sản phẩm lỗi thì rollback tất cả")]
    public async Task UpdateManyProducts_OneProductInvalid_RollbacksAll()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.ChangeStatus],
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        var outStockStatusId = ProductStatusConstants.OutOfBusiness;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, outStockStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = outStockStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var p1 = new ProductEntity
        {
            Name = $"P1_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        var p2 = new ProductEntity
        {
            Name = $"P2_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId,
            DeletedAt = DateTimeOffset.UtcNow
        };
        db.Products.AddRange(p1, p2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var request = new UpdateManyProductStatusesCommand { Ids = [p1.Id, p2.Id], StatusId = outStockStatusId };
        var response = await _client.PutAsJsonAsync("/api/v1/product/many/status", request).ConfigureAwait(true);
        response!.StatusCode.Should().NotBe(HttpStatusCode.OK);
        var dbP1 = await db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == p1.Id, CancellationToken.None)
            .ConfigureAwait(true);
        dbP1!.StatusId.Should().Be(productStatusId);
    }

    [Fact(DisplayName = "PRODUCT_101 - Lấy danh sách PredefinedOptions thành công khi có quyền Permissions.Warehouse.ProductManagement.Create")]
    public async Task GetPredefinedOptions_WithCreatePermission_ReturnsOkWithDictionary()
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        if (!await db.PredefinedOptions.AnyAsync(TestContext.Current.CancellationToken).ConfigureAwait(true))
        {
            db.PredefinedOptions
                .AddRange(
                    new PredefinedOption { Key = "VehicleType", Value = "Loại xe" },
                    new PredefinedOption { Key = "Color", Value = "Màu sắc" });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
        var response = await _client.GetAsync("/api/v1/option/predefined", CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<Dictionary<string, string>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNullOrEmpty();
        content!.Keys.Should().Contain("VehicleType");
    }

    [Fact(DisplayName = "PRODUCT_102 - Lấy danh sách PredefinedOptions thất bại khi không có quyền")]
    public async Task GetPredefinedOptions_WithoutPermission_ReturnsForbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [],
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
        var response = await _client.GetAsync("/api/v1/option/predefined", CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "PRODUCT_103 - Tạo sản phẩm thất bại khi Option Name không thuộc danh sách PredefinedOption")]
    public async Task CreateProduct_WithInvalidOptionName_ReturnsBadRequest()
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        var category = new ProductCategoryEntity { Name = $"Cat_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var command = new CreateProductCommand
        {
            Name = $"TestProduct_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId,
            Variants =
                [new CreateProductVariantRequest
                {
                    Price = 100,
                    OptionValues = new Dictionary<string, string> { { "INVALID_KEY_NOT_IN_PREDEFINED", "Val1" } }
                }]
        };
        var response = await _client.PostAsJsonAsync("/api/v1/product", command).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "PRODUCT_104 - Tạo sản phẩm với UrlSlug dài hơn 50 ký tự (lưu DB thành công)")]
    public async Task CreateProduct_LongUrlSlug_SavedSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.Create],
            CancellationToken.None,
            $"u_{uniqueId}@x.com")
            .ConfigureAwait(true);
        var loginResp = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResp.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = $"C_{uniqueId}" };
        var brand = new BrandEntity { Name = $"B_{uniqueId}" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, ProductStatusConstants.ForSale) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = ProductStatusConstants.ForSale });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        string longSlug = new('a', 200);
        var payload = new
        {
            name = "Long Slug Product",
            category_id = cat.Id,
            brand_id = brand.Id,
            status_id = "for-sale",
            variants = new[]
            {
                new
            {
                url_slug = longSlug,
                price = 1000,
                optionValueIds = Array.Empty<int>(),
                variant_name = "V1",
                cover_image_url = "image.jpg"
            }
            }
        };
        var response = await _client.PostAsJsonAsync("/api/v1/product", payload).ConfigureAwait(true);
        response!.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        var createdProduct = await db.ProductVariants
            .FirstOrDefaultAsync(v => string.Compare(v.UrlSlug, longSlug) == 0, CancellationToken.None)
            .ConfigureAwait(true);
        createdProduct.Should().NotBeNull();
        createdProduct!.UrlSlug?.Length.Should().Be(200);
    }

    [Fact(DisplayName = "PRODUCT_201 - Cập nhật sản phẩm với thuộc tính null (chuỗi trống) parse thành decimal an toàn")]
    public async Task UpdateProduct_EmptyDecimalStrings_ParsedAsNullSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View, Permissions.Warehouse.ProductManagement.Create, Permissions.Warehouse.ProductManagement.Edit],
            CancellationToken.None,
            $"u_{uniqueId}@x.com")
            .ConfigureAwait(true);
        var loginResp = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResp.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = $"C_{uniqueId}" };
        var brand = new BrandEntity { Name = $"B_{uniqueId}" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, ProductStatusConstants.ForSale) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = ProductStatusConstants.ForSale });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var reqBody = new
        {
            name = "Test Product Decimal",
            category_id = cat.Id,
            brand_id = brand.Id,
            weight = string.Empty,
            displacement = string.Empty,
            variants = new[]
            {
                new
            {
                url_slug = $"test-dec-{uniqueId}",
                price = 1000,
                optionValueIds = Array.Empty<int>(),
                variant_name = "V1",
                cover_image_url = "image.jpg"
            }
            }
        };
        var response = await _client.PostAsJsonAsync("/api/v1/product", reqBody).ConfigureAwait(true);
        var content = await response!.Content.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
        response.IsSuccessStatusCode.Should().BeTrue("Response body: {0}", content);
    }

    [Fact(DisplayName = "PRODUCT_106 - Cập nhật sản phẩm verify xóa cứng VariantOptionValue")]
    public async Task UpdateProduct_SoftDeleteVariantOptionValue_Verified()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"u_{uniqueId}";
        var password = "P1@password";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View, Permissions.Warehouse.ProductManagement.Create, Permissions.Warehouse.ProductManagement.Edit],
            CancellationToken.None,
            $"{username}@x.com")
            .ConfigureAwait(true);
        var loginResp = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResp.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = $"C_{uniqueId}" };
        var brand = new BrandEntity { Name = $"B_{uniqueId}" };
        var option = new Option { Name = "Color" };
        var optionValue = new OptionValue { Option = option, Name = "Red" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        db.Options.Add(option);
        db.OptionValues.Add(optionValue);
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, ProductStatusConstants.ForSale) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = ProductStatusConstants.ForSale });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var product = new ProductEntity
        {
            Name = "P1",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = ProductStatusConstants.ForSale
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var variant = new ProductVariant
        {
            ProductId = product.Id,
            UrlSlug = $"v-{uniqueId}",
            Price = 1000,
            VariantName = "1234"
        };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var variantOptionValue = new VariantOptionValue { VariantId = variant.Id, OptionValueId = optionValue.Id };
        db.Set<VariantOptionValue>().Add(variantOptionValue);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var updateCommand = new UpdateProductCommand
        {
            Id = product.Id,
            Name = "P1 Updated",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            Variants =
                [new UpdateProductVariantRequest
                {
                    Id = variant.Id,
                    Price = 2000,
                    OptionValues = [],
                    VariantName = "V1",
                    CoverImageUrl = "image.jpg"
                }]
        };
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/product/{product.Id}",
            updateCommand,
            options,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var contentStr = await response.Content
                .ReadAsStringAsync(TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            throw new Exception($"Response is {response.StatusCode}, content: {contentStr}");
        }
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var existsWithFilter = await db.Set<VariantOptionValue>()
            .AnyAsync(v => v.VariantId == variant.Id && v.OptionValueId == optionValue.Id, CancellationToken.None)
            .ConfigureAwait(true);
        var existsIgnoreFilter = await db.Set<VariantOptionValue>()
            .IgnoreQueryFilters()
            .AnyAsync(v => v.VariantId == variant.Id && v.OptionValueId == optionValue.Id, CancellationToken.None)
            .ConfigureAwait(true);
        existsWithFilter.Should().BeFalse();
        existsIgnoreFilter.Should().BeTrue();
    }

    [Fact(DisplayName = "PRODUCT_108 - Gọi API PredefinedOptions với quyền View hợp lệ")]
    public async Task GetPredefinedOptions_ValidPermissions_ReturnsOk()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"u_{uniqueId}";
        var password = "P1@password";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None,
            $"{username}@x.com")
            .ConfigureAwait(true);
        var loginResp = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResp.AccessToken);
        var response = await _client.GetAsync("/api/v1/option/predefined", CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "PRODUCT_109 - Gọi API PredefinedOptions không có quyền hợp lệ trả về Forbidden")]
    public async Task GetPredefinedOptions_NoPermissions_ReturnsForbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"u_{uniqueId}";
        var password = "P1@password";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [],
            CancellationToken.None,
            $"{username}@x.com")
            .ConfigureAwait(true);
        var loginResp = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResp.AccessToken);
        var response = await _client.GetAsync("/api/v1/option/predefined", CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "PRODUCT_117 - Lấy danh sách sản phẩm rút gọn trả về đúng cấu trúc lite (Happy Path)")]
    public async Task GetProductsForPriceManagement_ReturnsMinimalInfo()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"u_{uniqueId}";
        var password = "P1@password";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None,
            $"{username}@x.com")
            .ConfigureAwait(true);
        var loginResp = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResp.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = $"C_{uniqueId}" };
        var brand = new BrandEntity { Name = $"B_{uniqueId}" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, ProductStatusConstants.ForSale) == 0, CancellationToken.None)
            .ConfigureAwait(true))
        {
            db.ProductStatuses.Add(new ProductStatus { Key = ProductStatusConstants.ForSale });
        }
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var product = new ProductEntity
        {
            Name = $"Lite_Product_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = ProductStatusConstants.ForSale,
            Description = "This should be hidden"
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var variant = new ProductVariant { ProductId = product.Id, UrlSlug = $"v-lite-{uniqueId}", Price = 1234.56m };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync("/api/v1/product/for-price-management", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductPriceLiteResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        var item = content!.Items!.FirstOrDefault(x => x.Id == product.Id);
        item.Should().NotBeNull();
        item!.Name.Should().Be(product.Name);
        item.Variants.Should().HaveCount(1);
        item.Variants[0].Price.Should().Be(1234.56m);
        var json = await response!.Content!.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
        json.Should().NotContain("Description");
        json.Should().NotContain("This should be hidden");
    }

    [Fact(DisplayName = "PRODUCT_118 - Phân trang và lọc theo tên sản phẩm")]
    public async Task GetProductsForPriceManagement_FilteringByName_ReturnsMatchesOnly()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"u_{uniqueId}";
        var password = "P1@password";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None,
            $"{username}@x.com")
            .ConfigureAwait(true);
        var loginResp = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResp.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = $"C1_{uniqueId}" };
        var brand = new BrandEntity { Name = $"B1_{uniqueId}" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, ProductStatusConstants.ForSale) == 0, CancellationToken.None)
            .ConfigureAwait(true))
        {
            db.ProductStatuses.Add(new ProductStatus { Key = ProductStatusConstants.ForSale });
        }
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var p1 = new ProductEntity
        {
            Name = $"Exciter_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = ProductStatusConstants.ForSale
        };
        var p2 = new ProductEntity
        {
            Name = $"Winner_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = ProductStatusConstants.ForSale
        };
        db.Products.AddRange(p1, p2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync(
            $"/api/v1/product/for-price-management?Filters=Name@=Exciter_{uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductPriceLiteResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Items.Should().ContainSingle();
        content.Items.First().Name.Should().Be(p1.Name);
    }

    [Fact(DisplayName = "PRODUCT_121 - Lấy danh sách trống khi không có sản phẩm")]
    public async Task GetProductsForPriceManagement_NoProducts_ReturnsEmptyItems()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"u_{uniqueId}";
        var password = "P1@password";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None,
            $"{username}@x.com")
            .ConfigureAwait(true);
        var loginResp = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResp.AccessToken);
        var response = await _client.GetAsync(
            "/api/v1/product/for-price-management?Filters=Name==NON_EXISTENT_NAME",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductPriceLiteResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Items.Should().BeEmpty();
    }

    private async Task<string> AuthenticateForInventoryReceiptAsync(string uniqueId)
    {
        var username = $"u_{uniqueId}";
        var password = "P1@password";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.Warehouse.ReceiptManagement.Edit],
            CancellationToken.None,
            $"{username}@x.com")
            .ConfigureAwait(true);
        var loginResp = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        return loginResp.AccessToken;
    }

    private static async Task EnsureForSaleStatusAsync(ApplicationDBContext db)
    {
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, ProductStatusConstants.ForSale) == 0, CancellationToken.None)
            .ConfigureAwait(true))
        {
            db.ProductStatuses.Add(new ProductStatus { Key = ProductStatusConstants.ForSale });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
    }

    [Fact(DisplayName = "PRODUCT_122 - Tìm kiếm biến thể theo tên sản phẩm cha")]
    public async Task GetVariantsLiteForInventoryReceipt_SearchByParentProductName_ReturnsOnlyMatchingVariants()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await AuthenticateForInventoryReceiptAsync(uniqueId).ConfigureAwait(true));
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await EnsureForSaleStatusAsync(db).ConfigureAwait(true);
        var cat = new ProductCategoryEntity { Name = $"C_{uniqueId}" };
        var brand = new BrandEntity { Name = $"B_{uniqueId}" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var honda = new ProductEntity
        {
            Name = $"Honda_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = ProductStatusConstants.ForSale
        };
        var yamaha = new ProductEntity
        {
            Name = $"Yamaha_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = ProductStatusConstants.ForSale
        };
        db.Products.AddRange(honda, yamaha);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var v1 = new ProductVariant { ProductId = honda.Id, Price = 100, UrlSlug = $"h1_{uniqueId}" };
        var v2 = new ProductVariant { ProductId = honda.Id, Price = 200, UrlSlug = $"h2_{uniqueId}" };
        var v3 = new ProductVariant { ProductId = honda.Id, Price = 300, UrlSlug = $"h3_{uniqueId}" };
        var v4 = new ProductVariant { ProductId = yamaha.Id, Price = 400, UrlSlug = $"y1_{uniqueId}" };
        var v5 = new ProductVariant { ProductId = yamaha.Id, Price = 500, UrlSlug = $"y2_{uniqueId}" };
        db.ProductVariants.AddRange(v1, v2, v3, v4, v5);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync(
            $"/api/v1/product/variants-lite/for-InventoryReceipt?filters=search@=Honda_{uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductVariantLiteResponseForInventoryReceipt>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().HaveCount(3);
        content.Items.Should().AllSatisfy(item => item.DisplayName.Should().Contain($"Honda_{uniqueId}"));
        content.Items.Should().NotContain(item => item.DisplayName!.Contains($"Yamaha_{uniqueId}"));
    }

    [Fact(DisplayName = "PRODUCT_123 - Phân trang khi tìm lọc sản phẩm (trang 1)")]
    public async Task GetVariantsLiteForInventoryReceipt_SearchWithPagination_Page1ReturnsCorrectVariants()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await AuthenticateForInventoryReceiptAsync(uniqueId).ConfigureAwait(true));
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await EnsureForSaleStatusAsync(db).ConfigureAwait(true);
        var cat = new ProductCategoryEntity { Name = $"C_{uniqueId}" };
        var brand = new BrandEntity { Name = $"B_{uniqueId}" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var pA = new ProductEntity
        {
            Name = $"A_Paged_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = ProductStatusConstants.ForSale
        };
        var pD = new ProductEntity
        {
            Name = $"D_Paged_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = ProductStatusConstants.ForSale
        };
        db.Products.AddRange(pA, pD);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var variantsA = Enumerable.Range(1, 7)
            .Select(i => new ProductVariant { ProductId = pA.Id, Price = i * 100, UrlSlug = $"a{i}_{uniqueId}" })
            .ToList();
        var variantsD = Enumerable.Range(1, 4)
            .Select(i => new ProductVariant { ProductId = pD.Id, Price = i * 100, UrlSlug = $"d{i}_{uniqueId}" })
            .ToList();
        db.ProductVariants.AddRange(variantsA);
        db.ProductVariants.AddRange(variantsD);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync(
            $"/api/v1/product/variants-lite/for-InventoryReceipt?filters=search@=_Paged_{uniqueId}&page=1&pageSize=10",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductVariantLiteResponseForInventoryReceipt>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.TotalCount.Should().Be(11);
        content.Items.Should().HaveCount(10);
        content.TotalPages.Should().Be(2);
    }

    [Fact(DisplayName = "PRODUCT_124 - Phân trang lấy các phần tử còn lại (trang 2)")]
    public async Task GetVariantsLiteForInventoryReceipt_SearchWithPagination_Page2ReturnsRemainingVariants()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await AuthenticateForInventoryReceiptAsync(uniqueId).ConfigureAwait(true));
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await EnsureForSaleStatusAsync(db).ConfigureAwait(true);
        var cat = new ProductCategoryEntity { Name = $"C_{uniqueId}" };
        var brand = new BrandEntity { Name = $"B_{uniqueId}" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var pA = new ProductEntity
        {
            Name = $"A_P2_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = ProductStatusConstants.ForSale
        };
        var pD = new ProductEntity
        {
            Name = $"D_P2_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = ProductStatusConstants.ForSale
        };
        db.Products.AddRange(pA, pD);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var variantsA = Enumerable.Range(1, 7)
            .Select(i => new ProductVariant { ProductId = pA.Id, Price = i * 100, UrlSlug = $"a{i}_{uniqueId}" })
            .ToList();
        var variantsD = Enumerable.Range(1, 4)
            .Select(i => new ProductVariant { ProductId = pD.Id, Price = i * 100, UrlSlug = $"d{i}_{uniqueId}" })
            .ToList();
        db.ProductVariants.AddRange(variantsA);
        db.ProductVariants.AddRange(variantsD);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync(
            $"/api/v1/product/variants-lite/for-InventoryReceipt?filters=search@=_P2_{uniqueId}&page=2&pageSize=10",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductVariantLiteResponseForInventoryReceipt>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.TotalCount.Should().Be(11);
        content.Items.Should().HaveCount(1);
        content.PageNumber.Should().Be(2);
    }

    [Fact(DisplayName = "PRODUCT_125 - Hiển thị DisplayName có dịch tiếng Việt (1 option)")]
    public async Task GetVariantsLiteForInventoryReceipt_VariantWithSingleOption_DisplayNameContainsVietnameseLabel()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await AuthenticateForInventoryReceiptAsync(uniqueId).ConfigureAwait(true));
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await EnsureForSaleStatusAsync(db).ConfigureAwait(true);
        var cat = new ProductCategoryEntity { Name = $"C_{uniqueId}" };
        var brand = new BrandEntity { Name = $"B_{uniqueId}" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var product = new ProductEntity
        {
            Name = $"Prod_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = ProductStatusConstants.ForSale
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        if (!await db.PredefinedOptions
            .AnyAsync(x => string.Compare(x.Key, "Color") == 0, CancellationToken.None)
            .ConfigureAwait(true))
        {
            db.PredefinedOptions.Add(new PredefinedOption { Key = "Color", Value = "Màu sắc" });
        }
        var colorOption = await db.Options
            .FirstOrDefaultAsync(o => string.Compare(o.Name, "Color") == 0, CancellationToken.None)
            .ConfigureAwait(true);
        if (colorOption is null)
        {
            colorOption = new Option { Name = "Color" };
            db.Options.Add(colorOption);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
        var doOption = new OptionValue { OptionId = colorOption.Id, Name = "Đỏ" };
        db.OptionValues.Add(doOption);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var variant = new ProductVariant { ProductId = product.Id, Price = 100, UrlSlug = $"v-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.VariantOptionValues.Add(new VariantOptionValue { VariantId = variant.Id, OptionValueId = doOption.Id });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync(
            $"/api/v1/product/variants-lite/for-InventoryReceipt?filters=search@=Prod_{uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductVariantLiteResponseForInventoryReceipt>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        var item = content!.Items!.FirstOrDefault(v => v.Id == variant.Id);
        item.Should().NotBeNull();
        item!.DisplayName.Should().Be($"Prod_{uniqueId} (Màu sắc: Đỏ)");
    }

    [Fact(DisplayName = "PRODUCT_126 - Hiển thị DisplayName có dịch tiếng Việt (Nhiều option)")]
    public async Task GetVariantsLiteForInventoryReceipt_VariantWithMultipleOptions_DisplayNameContainsAllTranslatedLabels(
        )
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await AuthenticateForInventoryReceiptAsync(uniqueId).ConfigureAwait(true));
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await EnsureForSaleStatusAsync(db).ConfigureAwait(true);
        var cat = new ProductCategoryEntity { Name = $"C_{uniqueId}" };
        var brand = new BrandEntity { Name = $"B_{uniqueId}" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var product = new ProductEntity
        {
            Name = $"Prod_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = ProductStatusConstants.ForSale
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        if (!await db.PredefinedOptions
            .AnyAsync(x => string.Compare(x.Key, "Color") == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.PredefinedOptions.Add(new PredefinedOption { Key = "Color", Value = "Màu sắc" });
        if (!await db.PredefinedOptions
            .AnyAsync(x => string.Compare(x.Key, "Displacement") == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.PredefinedOptions.Add(new PredefinedOption { Key = "Displacement", Value = "Phân khối" });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var colorOption = await db.Options
                .FirstOrDefaultAsync(o => string.Compare(o.Name, "Color") == 0, CancellationToken.None)
                .ConfigureAwait(true) ??
            new Option { Name = "Color" };
        var displOption = await db.Options
                .FirstOrDefaultAsync(o => string.Compare(o.Name, "Displacement") == 0, CancellationToken.None)
                .ConfigureAwait(true) ??
            new Option { Name = "Displacement" };
        if (colorOption.Id == 0)
            db.Options.Add(colorOption);
        if (displOption.Id == 0)
            db.Options.Add(displOption);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var doOV = new OptionValue { OptionId = colorOption.Id, Name = "Đỏ" };
        var cc150OV = new OptionValue { OptionId = displOption.Id, Name = "150cc" };
        db.OptionValues.AddRange(doOV, cc150OV);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var variant = new ProductVariant { ProductId = product.Id, Price = 200, UrlSlug = $"v-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.VariantOptionValues.Add(new VariantOptionValue { VariantId = variant.Id, OptionValueId = doOV.Id });
        db.VariantOptionValues.Add(new VariantOptionValue { VariantId = variant.Id, OptionValueId = cc150OV.Id });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync(
            $"/api/v1/product/variants-lite/for-InventoryReceipt?filters=search@=Prod_{uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductVariantLiteResponseForInventoryReceipt>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        var item = content.Items?.FirstOrDefault(v => v.Id == variant.Id);
        item.Should().NotBeNull();
        item!.DisplayName.Should().StartWith($"Prod_{uniqueId} (");
        item.DisplayName.Should().Contain("Màu sắc: Đỏ");
        item.DisplayName.Should().Contain("Phân khối: 150cc");
    }

    [Fact(DisplayName = "PRODUCT_127 - Hiển thị DisplayName khi không có option")]
    public async Task GetVariantsLiteForInventoryReceipt_VariantWithNoOptions_DisplayNameEqualsProductName()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await AuthenticateForInventoryReceiptAsync(uniqueId).ConfigureAwait(true));
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await EnsureForSaleStatusAsync(db).ConfigureAwait(true);
        var cat = new ProductCategoryEntity { Name = $"C_{uniqueId}" };
        var brand = new BrandEntity { Name = $"B_{uniqueId}" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var product = new ProductEntity
        {
            Name = $"Solo_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = ProductStatusConstants.ForSale
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var variant = new ProductVariant { ProductId = product.Id, Price = 50, UrlSlug = $"v-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync(
            $"/api/v1/product/variants-lite/for-InventoryReceipt?filters=search@=Solo_{uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductVariantLiteResponseForInventoryReceipt>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        var item = content!.Items!.FirstOrDefault(v => v.Id == variant.Id);
        item.Should().NotBeNull();
        item!.DisplayName.Should().Be($"Solo_{uniqueId}");
    }

    [Fact(DisplayName = "PRODUCT_134 - Lấy danh sách Options thành công")]
    public async Task GetOptions_ReturnsOptionsAndValues()
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var preOpt = new PredefinedOption { Key = $"Color_{uniqueId}", Value = "Màu sắc" };
        db.PredefinedOptions.Add(preOpt);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var option = new Option { Name = preOpt.Key };
        db.Options.Add(option);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var val1 = new OptionValue { OptionId = option.Id, Name = "Red" };
        var val2 = new OptionValue { OptionId = option.Id, Name = "Blue" };
        db.OptionValues.AddRange(val1, val2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync("/api/v1/option", CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<List<OptionResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Should().Contain(o => o.Id == option.Id);
        var optRes = content!.First(o => o.Id == option.Id);
        optRes.OptionValues.Should().Contain(v => v.Id == val1.Id);
        optRes.OptionValues.Should().Contain(v => v.Id == val2.Id);
    }

    [Fact(DisplayName = "PRODUCT_135 - Lọc sản phẩm theo OptionValueIds")]
    public async Task GetProducts_FilterByOptionValueIds_ReturnsMatchingProducts()
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, TestContext.Current.CancellationToken)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var category = new ProductCategoryEntity { Name = $"Cat_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        var preOpt = new PredefinedOption { Key = $"Color_{uniqueId}", Value = "Màu sắc" };
        db.PredefinedOptions.Add(preOpt);
        var option = new Option { Name = preOpt.Key };
        db.Options.Add(option);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var valRed = new OptionValue { OptionId = option.Id, Name = "Red" };
        var valBlue = new OptionValue { OptionId = option.Id, Name = "Blue" };
        db.OptionValues.AddRange(valRed, valBlue);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var p1 = new ProductEntity
        {
            Name = $"P1_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        var p2 = new ProductEntity
        {
            Name = $"P2_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        db.Products.AddRange(p1, p2);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var v1_red = new ProductVariant { ProductId = p1.Id, Price = 100, UrlSlug = $"v1_red_{uniqueId}" };
        var v2_blue = new ProductVariant { ProductId = p2.Id, Price = 200, UrlSlug = $"v2_blue_{uniqueId}" };
        db.ProductVariants.AddRange(v1_red, v2_blue);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        db.VariantOptionValues
            .AddRange(
                new VariantOptionValue { VariantId = v1_red.Id, OptionValueId = valRed.Id },
                new VariantOptionValue { VariantId = v2_blue.Id, OptionValueId = valBlue.Id });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var response = await _client.GetAsync($"/api/v1/product?optionValueIds={valRed.Id}", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().Contain(p => p.Id == p1.Id);
        content.Items.Should().NotContain(p => p.Id == p2.Id);
        var item1 = content.Items.First(p => p.Id == p1.Id);
        item1.Variants.Should().Contain(v => v.Id == v1_red.Id);
        item1.Variants.Should().NotContain(v => v.Id == v2_blue.Id);
    }

    [Fact(DisplayName = "PRODUCT_136 - Tìm kiếm tên và lọc theo OptionValueIds")]
    public async Task GetProducts_SearchAndFilterByOption_ReturnsMatchingProduct()
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
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, TestContext.Current.CancellationToken)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        var category = new ProductCategoryEntity { Name = $"Cat_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        var preOpt = new PredefinedOption { Key = $"Color_{uniqueId}", Value = "Màu sắc" };
        db.PredefinedOptions.Add(preOpt);
        var option = new Option { Name = preOpt.Key };
        db.Options.Add(option);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var valRed = new OptionValue { OptionId = option.Id, Name = "Red" };
        db.OptionValues.Add(valRed);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var pMatch = new ProductEntity
        {
            Name = $"UniqueName_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        var pOther = new ProductEntity
        {
            Name = $"OtherName_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        db.Products.AddRange(pMatch, pOther);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var vMatch = new ProductVariant { ProductId = pMatch.Id, Price = 100, UrlSlug = $"v_match_{uniqueId}" };
        db.ProductVariants.Add(vMatch);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        db.VariantOptionValues.Add(new VariantOptionValue { VariantId = vMatch.Id, OptionValueId = valRed.Id });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var response = await _client.GetAsync(
            $"/api/v1/product?filters=search=UniqueName_{uniqueId},optionValueIds={valRed.Id}",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().ContainSingle(p => p.Id == pMatch.Id);
    }

    [Fact(DisplayName = "PRODUCT_141 - Lấy thông tin batch cho nhiều biến thể hợp lệ")]
    public async Task GetVariantCartDetailsBatch_ValidIds_ReturnsDetails()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
        {
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        }
        var category = new ProductCategoryEntity { Name = $"Cat_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var product = new ProductEntity
        {
            Name = $"Xe May {uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var v1 = new ProductVariant
        {
            ProductId = product.Id,
            Price = 50000000,
            UrlSlug = $"v1-{uniqueId}",
            CoverImageUrl = "http://honda.com/v1.jpg"
        };
        var v2 = new ProductVariant
        {
            ProductId = product.Id,
            Price = 60000000,
            UrlSlug = $"v2-{uniqueId}",
            CoverImageUrl = "http://honda.com/v2.jpg"
        };
        db.ProductVariants.AddRange(v1, v2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.PostAsJsonAsync(
            "/api/v1/product/variants-cart-details-batch",
            new List<int> { v1.Id, v2.Id })
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<List<VariantCartDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Should().HaveCount(2);
        var v1Res = content!.FirstOrDefault(x => x.Id == v1.Id);
        v1Res.Should().NotBeNull();
        v1Res!.DisplayName.Should().Contain(product.Name);
        v1Res.Price.Should().Be(v1.Price ?? 0);
        v1Res.CoverImageUrl.Should().Be(v1.CoverImageUrl);
    }

    [Fact(DisplayName = "PRODUCT_142 - Lấy ảnh từ bộ sưu tập nếu CoverImageUrl của biến thể bị trống")]
    public async Task GetVariantCartDetailsBatch_NoCoverImage_FallsBackToCollection()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
        {
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        }
        var category = new ProductCategoryEntity { Name = $"Cat_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var product = new ProductEntity
        {
            Name = $"Xe {uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var v1 = new ProductVariant
        {
            ProductId = product.Id,
            Price = 40000000,
            UrlSlug = $"v-no-cover-{uniqueId}",
            CoverImageUrl = null
        };
        db.ProductVariants.Add(v1);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var photo = new ProductCollectionPhoto
        {
            ProductVariantId = v1.Id,
            ImageUrl = "http://honda.com/collection1.jpg"
        };
        db.ProductCollectionPhotos.Add(photo);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.PostAsJsonAsync(
            "/api/v1/product/variants-cart-details-batch",
            new List<int> { v1.Id })
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<List<VariantCartDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content![0].CoverImageUrl.Should().Be(photo.ImageUrl);
    }

    [Fact(
        DisplayName = "PRODUCT_143 - Cập nhật sản phẩm sử dụng payload snake_case (đảm bảo binding đúng các trường ảnh và slug)")]
    public async Task UpdateProduct_WithSnakeCaseJson_BindsCorrectly()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Warehouse.ProductManagement.View, Permissions.Warehouse.ProductManagement.Edit],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var productStatusId = ProductStatusConstants.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, TestContext.Current.CancellationToken)
            .ConfigureAwait(true))
        {
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        }
        var category = new ProductCategoryEntity { Name = $"Cat_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var product = new ProductEntity
        {
            Name = $"Original_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId,
            ProductVariants =
                [new ProductVariant
                {
                    Price = 100,
                    UrlSlug = $"slug-{uniqueId}",
                    CoverImageUrl = "old.jpg",
                    VariantName = "1234"
                }]
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var variantId = product.ProductVariants.First().Id;
        var updatePayload = $@"{{
            ""name"": ""Updated_{uniqueId}"",
            ""category_id"": {category.Id},
            ""brand_id"": {brand.Id},
            ""short_description"": ""Mô tả ngắn"",
            ""seat_height"": 800,
            ""variants"": [{{
                ""id"": {variantId},
                ""url_slug"": ""updated-slug-{uniqueId}"",
                ""price"": 1000,
                ""variant_name"": ""V1"",
                ""cover_image_url"": ""http://example.com/new-cover.jpg"",
                ""photo_collection"": [""http://example.com/photo1.jpg""]
            }}]
        }}";
        var response = await _client.PutAsync(
            $"/api/v1/product/{product.Id}",
            new StringContent(updatePayload, Encoding.UTF8, "application/json"),
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope2 = _factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var updatedProduct = await db2.Products
            .Include(p => p.ProductVariants)
            .ThenInclude(v => v.ProductCollectionPhotos)
            .FirstOrDefaultAsync(p => p.Id == product.Id, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        updatedProduct.Should().NotBeNull();
        updatedProduct!.Name.Should().Be($"Updated_{uniqueId}");
        updatedProduct.ShortDescription.Should().Be("Mô tả ngắn");
        updatedProduct.SeatHeight.Should().Be(800);
        var variant = updatedProduct.ProductVariants.First();
        variant.UrlSlug.Should().Be($"updated-slug-{uniqueId}");
        variant.CoverImageUrl.Should().Be("http://example.com/new-cover.jpg");
        variant.ProductCollectionPhotos
            .Should()
            .ContainSingle(p => string.Compare(p.ImageUrl, "http://example.com/photo1.jpg") == 0);
    }

    [Fact(DisplayName = "PRODUCT_144 - Lưu trữ thông tin định danh chuyên sâu của biến thể")]
    public async Task CreateProduct_SpecializedIdentifiers_SavesCorrectly()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = "Cat-Special" };
        var brand = new BrandEntity { Name = "Brand-Special" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var payload = new
        {
            name = "Special Bike",
            category_id = cat.Id,
            brand_id = brand.Id,
            status_id = "for-sale",
            variants = new[]
            {
                new
            {
                sku = "SKU-123",
                variant_name = "Premium",
                price = 1000,
                url_slug = "special-bike-v1",
                cover_image_url = "http://example.com/special-bike-v1.jpg",
            }
            }
        };
        var uniqueIdAuth = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueIdAuth}",
            "Password123!",
            [Permissions.Warehouse.ProductManagement.Create, Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResp = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueIdAuth}",
            "Password123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResp.AccessToken);
        var response = await _client.PostAsJsonAsync("/api/v1/product", payload).ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var variant = await db.ProductVariants
            .Include(v => v.ProductVariantColors)
            .FirstOrDefaultAsync(v => string.Compare(v.SKU, "SKU-123") == 0, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        variant.Should().NotBeNull();
        variant!.SKU.Should().Be("SKU-123");
        variant.VariantName.Should().Be("Premium");
        variant.CoverImageUrl.Should().Be("http://example.com/special-bike-v1.jpg");
    }

    [Fact(DisplayName = "PRODUCT_149 - Tạo sản phẩm lưu mã màu vào ProductVariantColor")]
    public async Task CreateProduct_SaveColorCodeToProductVariantColor()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [Permissions.Warehouse.ProductManagement.Create],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = "Cat-ColorUpdate" };
        var brand = new BrandEntity { Name = "Brand-ColorUpdate" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var payload = new
        {
            name = "Bike",
            category_id = cat.Id,
            brand_id = brand.Id,
            variants = new[]
            {
                new
            {
                url_slug = "v1",
                price = 1000,
                variant_name = "V1",
                cover_image_url = "image.jpg",
                colors = new[] { new { color_name = "Red", color_code = "#FF0000", cover_image_url = "image.jpg" } }
            }
            }
        };
        var response = await _client.PostAsJsonAsync("/api/v1/product", payload).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.Created);
        using var assertScope = _factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var product = await assertDb.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => string.Compare(p.Name, "Bike") == 0, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        product.Should().NotBeNull("Product should have been created");
        var variant = await assertDb.ProductVariants
            .Include(v => v.ProductVariantColors)
            .AsNoTracking()
            .FirstAsync(v => string.Compare(v.UrlSlug, "v1") == 0, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        variant.ProductVariantColors.Should().ContainSingle();
        variant.ProductVariantColors.First().ColorCode.Should().Be("#FF0000");
    }

    [Fact(DisplayName = "PRODUCT_150 - Lưu trữ thông tin SEO Metadata cho sản phẩm")]
    public async Task CreateProduct_SavesSeoMetadata()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [Permissions.Warehouse.ProductManagement.Create],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = "Cat-SEO" };
        var brand = new BrandEntity { Name = "Brand-SEO" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var payload = new
        {
            name = "SEO Bike",
            category_id = cat.Id,
            brand_id = brand.Id,
            meta_title = "Meta Title",
            meta_description = "Meta Description",
            short_description = "Short Description",
            variants = new[]
            {
                new { url_slug = "seo-bike", price = 1000, variant_name = "V1", cover_image_url = "image.jpg" }
            }
        };
        var response150 = await _client.PostAsJsonAsync("/api/v1/product", payload).ConfigureAwait(true);
        if (!response150.IsSuccessStatusCode)
        {
            var error = await response150.Content
                .ReadAsStringAsync(TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            throw new Exception($"API failed with {response150.StatusCode}: {error}");
        }
        var product = await db.Products
            .FirstOrDefaultAsync(p => string.Compare(p.Name, "SEO Bike") == 0, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        product!.MetaTitle.Should().Be("Meta Title");
        product.MetaDescription.Should().Be("Meta Description");
        product.ShortDescription.Should().Be("Short Description");
    }

    [Fact(DisplayName = "PRODUCT_159 - Tạo sản phẩm kèm danh sách công nghệ nổi bật")]
    public async Task CreateProduct_WithHighlights_SavesCorrectly()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [Permissions.Warehouse.ProductManagement.Create],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = "Cat-Tech" };
        var brand = new BrandEntity { Name = "Brand-Tech" };
        var tech = new TechnologyEntity { Name = "ABS" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        db.Technologies.Add(tech);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var payload = new
        {
            name = "Tech Bike",
            category_id = cat.Id,
            brand_id = brand.Id,
            product_technologies = new[] { new { technology_id = tech.Id, custom_title = "Cool ABS" } },
            variants = new[]
            {
                new { url_slug = "tech-bike", price = 1000, variant_name = "V1", cover_image_url = "image.jpg" }
            }
        };
        var response = await _client.PostAsJsonAsync("/api/v1/product", payload).ConfigureAwait(true);
        response.EnsureSuccessStatusCode();
        using var assertScope = _factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var link = await assertDb.ProductTechnologies
            .AsNoTracking()
            .FirstOrDefaultAsync(
                pt => string.Compare(pt.CustomTitle, "Cool ABS") == 0,
                TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        link.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_161 - Cập nhật Highlights: Thêm công nghệ mới")]
    public async Task UpdateProduct_AddHighlight_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [Permissions.Warehouse.ProductManagement.Edit],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = "Cat-UpdateAdd" };
        var brand = new BrandEntity { Name = "Brand-UpdateAdd" };
        var t1 = new TechnologyEntity { Name = "T1" };
        var t2 = new TechnologyEntity { Name = "T2" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        db.Technologies.AddRange(t1, t2);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var p = new ProductEntity { Name = "P", CategoryId = cat.Id, BrandId = brand.Id, StatusId = "for-sale" };
        db.Products.Add(p);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        db.ProductTechnologies.Add(new ProductTechnology { ProductId = p.Id, TechnologyId = t1.Id });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var cmd = new UpdateProductCommand
        {
            Id = p.Id,
            Name = "P",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            ProductTechnologies =
                [new TechnologyJsonRequest { TechnologyId = t1.Id }, new TechnologyJsonRequest { TechnologyId = t2.Id }],
            Variants =
                [new UpdateProductVariantRequest
                {
                    UrlSlug = "v1",
                    Price = 1000,
                    VariantName = "V1",
                    CoverImageUrl = "image.jpg"
                }]
        };
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/product/{p.Id}",
            cmd,
            options,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var contentStr = await response.Content
                .ReadAsStringAsync(TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            throw new Exception($"Response is {response.StatusCode}, content: {contentStr}");
        }
        db.ChangeTracker.Clear();
        var links = await db.ProductTechnologies
            .Where(pt => pt.ProductId == p.Id)
            .ToListAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        links.Should().HaveCount(2);
    }

    [Fact(DisplayName = "PRODUCT_162 - Cập nhật Highlights: Xóa công nghệ khỏi sản phẩm")]
    public async Task UpdateProduct_RemoveHighlight_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [Permissions.Warehouse.ProductManagement.Edit],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = "Cat-UpdateRem" };
        var brand = new BrandEntity { Name = "Brand-UpdateRem" };
        var t1 = new TechnologyEntity { Name = "T1" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        db.Technologies.Add(t1);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var p = new ProductEntity { Name = "P", CategoryId = cat.Id, BrandId = brand.Id, StatusId = "for-sale" };
        db.Products.Add(p);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        db.ProductTechnologies.Add(new ProductTechnology { ProductId = p.Id, TechnologyId = t1.Id });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var cmd = new UpdateProductCommand
        {
            Id = p.Id,
            Name = "P",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            ProductTechnologies = [],
            Variants =
                [new UpdateProductVariantRequest
                {
                    UrlSlug = "v1",
                    Price = 1000,
                    VariantName = "V1",
                    CoverImageUrl = "image.jpg"
                }]
        };
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/product/{p.Id}",
            cmd,
            options,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var contentStr = await response.Content
                .ReadAsStringAsync(TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            throw new Exception($"Response is {response.StatusCode}, content: {contentStr}");
        }
        db.ChangeTracker.Clear();
        var links = await db.ProductTechnologies
            .Where(pt => pt.ProductId == p.Id)
            .ToListAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        links.Should().BeEmpty();
    }

    [Fact(DisplayName = "PRODUCT_163 - Cập nhật Highlights: Thay đổi thông tin tùy chỉnh")]
    public async Task UpdateProduct_UpdateHighlightCustom_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [Permissions.Warehouse.ProductManagement.Edit],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = "Cat-UpdateCust" };
        var t1 = new TechnologyEntity { Name = "T1" };
        db.ProductCategories.Add(cat);
        db.Technologies.Add(t1);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var p = new ProductEntity { Name = "P", CategoryId = cat.Id, StatusId = "for-sale" };
        db.Products.Add(p);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        db.ProductTechnologies
            .Add(new ProductTechnology { ProductId = p.Id, TechnologyId = t1.Id, CustomTitle = "Old" });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var brand = new BrandEntity { Name = "B1" };
        db.Brands.Add(brand);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var cmd = new UpdateProductCommand
        {
            Id = p.Id,
            Name = "P",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            ProductTechnologies = [new TechnologyJsonRequest { TechnologyId = t1.Id, CustomTitle = "New" }],
            Variants =
                [new UpdateProductVariantRequest
                {
                    UrlSlug = "v1",
                    Price = 1000,
                    VariantName = "V1",
                    CoverImageUrl = "image.jpg"
                }]
        };
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/product/{p.Id}",
            cmd,
            options,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var contentStr = await response.Content
                .ReadAsStringAsync(TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            throw new Exception($"Response is {response.StatusCode}, content: {contentStr}");
        }
        db.ChangeTracker.Clear();
        var link = await db.ProductTechnologies
            .FirstOrDefaultAsync(pt => pt.ProductId == p.Id, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        link!.CustomTitle.Should().Be("New");
    }

    [Fact(DisplayName = "PRODUCT_167 - Sắp xếp công nghệ theo thứ tự hiển thị khi truy vấn")]
    public async Task GetProduct_HighlightsOrderedByDisplayOrder()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = "Cat-Order" };
        var t1 = new TechnologyEntity { Name = "T1" };
        var t2 = new TechnologyEntity { Name = "T2" };
        db.ProductCategories.Add(cat);
        db.Technologies.AddRange(t1, t2);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var p = new ProductEntity { Name = "P-Ordered", CategoryId = cat.Id, StatusId = "for-sale" };
        db.Products.Add(p);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        db.ProductTechnologies.Add(new ProductTechnology { ProductId = p.Id, TechnologyId = t1.Id, DisplayOrder = 2 });
        db.ProductTechnologies.Add(new ProductTechnology { ProductId = p.Id, TechnologyId = t2.Id, DisplayOrder = 1 });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var response = await _client.GetAsync(
            $"/api/v1/product/{p.Id}/for-manager",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response.EnsureSuccessStatusCode();
        var content = await response!.Content
            .ReadFromJsonAsync<ProductDetailForManagerResponse>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var highlights = content!.ProductTechnologies;
        highlights![0].TechnologyId.Should().Be(t2.Id);
    }

    [Fact(DisplayName = "PRODUCT_171 - Tìm kiếm sản phẩm theo giá tối thiểu (MinPrice)")]
    public async Task GetProducts_MinPrice_FiltersCorrectly()
    {
        var response = await _client.GetAsync(
            "/api/v1/product?MinPrice=50000000",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "PRODUCT_172 - Tìm kiếm sản phẩm theo giá tối đa (MaxPrice)")]
    public async Task GetProducts_MaxPrice_FiltersCorrectly()
    {
        var response = await _client.GetAsync(
            "/api/v1/product?MaxPrice=30000000",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "PRODUCT_173 - Tìm kiếm sản phẩm theo nhiều thương hiệu (BrandIds)")]
    public async Task GetProducts_BrandIds_FiltersCorrectly()
    {
        var response = await _client.GetAsync("/api/v1/product?BrandIds=1,2", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "PRODUCT_174 - Kết hợp lọc giá và thương hiệu đồng thời")]
    public async Task GetProducts_CombinedPriceBrand_FiltersCorrectly()
    {
        var response = await _client.GetAsync(
            "/api/v1/product?MinPrice=40000000&BrandIds=1",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "PRODUCT_170 - Đảm bảo Cascade Delete khi xóa công nghệ gốc")]
    public async Task DeleteTechnology_CascadeDeletesLinks()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = "Cat-Cascade" };
        var t = new TechnologyEntity { Name = "T" };
        db.ProductCategories.Add(cat);
        db.Technologies.Add(t);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var p = new ProductEntity { Name = "P", CategoryId = cat.Id, StatusId = "for-sale" };
        db.Products.Add(p);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        db.ProductTechnologies.Add(new ProductTechnology { ProductId = p.Id, TechnologyId = t.Id });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        db.Technologies.Remove(t);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var link = await db.ProductTechnologies
            .FirstOrDefaultAsync(pt => pt.TechnologyId == t.Id, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        link.Should().BeNull();
    }

    [Fact(DisplayName = "PRODUCT_188 - Đính kèm các Công nghệ hiện có vào Sản phẩm")]
    public async Task UpdateProduct_AttachTechnologies_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            [Permissions.Warehouse.ProductManagement.Edit],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "Password123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = $"Cat_{uniqueId}" };
        var tech1 = new TechnologyEntity { Name = $"Tech1_{uniqueId}" };
        var tech2 = new TechnologyEntity { Name = $"Tech2_{uniqueId}" };
        db.ProductCategories.Add(cat);
        db.Technologies.AddRange(tech1, tech2);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var product = new ProductEntity { Name = $"Prod_{uniqueId}", CategoryId = cat.Id, StatusId = "for-sale" };
        db.Products.Add(product);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var highlights = new[]
        {
            new { technology_id = tech1.Id, custom_title = "Smart Key" },
            new { technology_id = tech2.Id, custom_title = "ABS" }
        };
        var payload = new
        {
            name = product.Name,
            product_technologies = highlights,
            variants = new[] { new { price = 1000, variant_name = "V1", cover_image_url = "image.jpg" } }
        };
        var response = await HttpClientJsonExtensions.PutAsJsonAsync(
            _client,
            $"/api/v1/product/{product.Id}",
            payload,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var attachedTechs = await db.ProductTechnologies
            .Where(pt => pt.ProductId == product.Id)
            .ToListAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        attachedTechs.Should().HaveCount(2);
    }

    [Fact(DisplayName = "PRODUCT_191 - Lọc danh sách sản phẩm theo Tiêu chuẩn an toàn (Safety Standard)")]
    public async Task GetProducts_FilterBySafetyStandard_ReturnsCorrectProducts()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            [Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "Password123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = $"Cat_{uniqueId}" };
        db.ProductCategories.Add(cat);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var p1 = new ProductEntity
        {
            Name = $"P1_{uniqueId}",
            CategoryId = cat.Id,
            StatusId = "for-sale",
            StdSnell = true
        };
        var p2 = new ProductEntity
        {
            Name = $"P2_{uniqueId}",
            CategoryId = cat.Id,
            StatusId = "for-sale",
            StdSnell = false
        };
        db.Products.AddRange(p1, p2);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var response = await _client.GetAsync(
            $"/api/v1/product?filters=StdSnell==true",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailResponse>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content!.Items.Should().Contain(p => p.Id == p1.Id);
        content.Items.Should().NotContain(p => p.Id == p2.Id);
    }

    [Fact(DisplayName = "PRODUCT_183 - Tạo sản phẩm mới với đầy đủ thông số kỹ thuật (Lốp, Phanh, v.v.)")]
    public async Task CreateProduct_WithDetailedSpecs_ReturnsCreated()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"u_{uniqueId}",
            "Pass123!",
            [Permissions.Warehouse.ProductManagement.Create, Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"u_{uniqueId}",
            "Pass123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = $"Cat_{uniqueId}" };
        db.ProductCategories.Add(cat);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var request = new CreateProductCommand
        {
            Name = $"Motor_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            FrontTireSize = "120/70-17",
            RearTireSize = "180/55-17",
            FrontBrake = "Brembo",
            RearBrake = "Nissin",
            Variants =
                [new CreateProductVariantRequest
                {
                    UrlSlug = $"v-{uniqueId}",
                    Price = 5000,
                    OptionValueIds = [],
                    VariantName = "V1",
                    CoverImageUrl = "image.jpg"
                }]
        };
        var response = await _client.PostAsync("/api/v1/product", JsonContent.Create(request), CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response!.Content
            .ReadFromJsonAsync<ProductDetailForManagerResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.FrontTireSize.Should().Be("120/70-17");
    }

    [Fact(DisplayName = "PRODUCT_185 - Cập nhật tiêu chuẩn an toàn cho sản phẩm (DOT, ECE, SNELL, JIS)")]
    public async Task UpdateProduct_SafetyStandards_ReturnsUpdated()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"u_{uniqueId}",
            "Pass123!",
            [Permissions.Warehouse.ProductManagement.Edit, Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"u_{uniqueId}",
            "Pass123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = $"Cat_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        var product = new ProductEntity
        {
            Name = $"P_{uniqueId}",
            ProductCategory = cat,
            Brand = brand,
            StdDot = false,
            StdSnell = false
        };
        var variant = new ProductVariant { Product = product, Price = 5000, UrlSlug = $"v-{uniqueId}" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        db.Products.Add(product);
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var request = new UpdateProductCommand
        {
            Id = product.Id,
            Name = product.Name,
            BrandId = brand.Id,
            StdDot = true,
            StdSnell = true,
            Variants =
                [new UpdateProductVariantRequest
                {
                    Id = product.ProductVariants.First().Id,
                    Price = 5500,
                    VariantName = "V1",
                    CoverImageUrl = "image.jpg"
                }]
        };
        var response = await _client.PutAsync(
            $"/api/v1/product/{product.Id}",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"),
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == product.Id, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        updated!.StdDot.Should().BeTrue();
        updated.StdSnell.Should().BeTrue();
    }

    [Fact(DisplayName = "PRODUCT_186 - Thiết lập danh sách xe tương thích cho phụ tùng")]
    public async Task SetCompatibility_ValidVehicleIds_ReturnsSuccess()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"u_{uniqueId}",
            "Pass123!",
            [Permissions.Warehouse.ProductManagement.Edit],
            CancellationToken.None)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"u_{uniqueId}",
            "Pass123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var cat = new ProductCategoryEntity { Name = $"Cat_{uniqueId}" };
        var part = new ProductEntity { Name = $"Part_{uniqueId}", ProductCategory = cat };
        var vehicle = new ProductEntity { Name = $"Bike_{uniqueId}", ProductCategory = cat };
        db.ProductCategories.Add(cat);
        db.Products.AddRange(part, vehicle);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        var request = new { compatible_vehicle_ids = new List<int> { vehicle.Id } };
        var response = await _client.PostAsync(
            $"/api/v1/product/{part.Id}/compatibility",
            JsonContent.Create(request),
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var count = await db.Set<ProductCompatibility>()
            .CountAsync(
                c => c.BaseProductId == part.Id && c.CompatibleVehicleModelId == vehicle.Id,
                TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        count.Should().Be(1);
    }

    [Fact(DisplayName = "PRODUCT_194 - Tạo sản phẩm lưu màu biến thể vào bảng ProductVariantColor")]
    public async Task CreateProduct_ColorVariant_SavesColorImageToProductVariantColorOnly()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"u_{uniqueId}",
            "Pass123!",
            [Permissions.Warehouse.ProductManagement.Create, Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"u_{uniqueId}",
            "Pass123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var category = new ProductCategoryEntity { Name = $"Cat_Color_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_Color_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var payload = new
        {
            name = $"Product_Color_{uniqueId}",
            category_id = category.Id,
            brand_id = brand.Id,
            variants = new[]
            {
                new
            {
                variant_name = "Premium",
                url_slug = $"product-color-{uniqueId}",
                price = 1000,
                colors = new[]
                {
                    new { color_name = "Red", color_code = "#FF0000", cover_image_url = "https://example.com/red.jpg" }
                }
            }
            }
        };
        var response = await _client.PostAsync(
            "/api/v1/product",
            JsonContent.Create(payload),
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.Created);
        var variant = await db.ProductVariants
            .Include(v => v.ProductVariantColors)
            .FirstAsync(
                v => string.Compare(v.UrlSlug, $"product-color-{uniqueId}") == 0,
                TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        variant.CoverImageUrl.Should().BeNull();
        variant.ProductVariantColors.Should().NotBeEmpty();
        var variantColor = variant.ProductVariantColors.First();
        variantColor.ColorName.Should().Be("Red");
        variantColor.ColorCode.Should().Be("#FF0000");
        variantColor.CoverImageUrl.Should().Be("https://example.com/red.jpg");
    }

    [Fact(DisplayName = "PRODUCT_195 - Cập nhật sản phẩm lưu màu biến thể vào bảng ProductVariantColor")]
    public async Task UpdateProduct_ColorVariant_SavesColorImageToProductVariantColorOnly()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"u_{uniqueId}",
            "Pass123!",
            [Permissions.Warehouse.ProductManagement.Edit, Permissions.Warehouse.ProductManagement.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"u_{uniqueId}",
            "Pass123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var category = new ProductCategoryEntity { Name = $"Cat_Update_Color_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_Update_Color_{uniqueId}" };
        var product = new ProductEntity
        {
            Name = $"Product_Update_Color_{uniqueId}",
            ProductCategory = category,
            Brand = brand,
            StatusId = ProductStatusConstants.ForSale,
            ProductVariants =
                [new ProductVariant
                {
                    VariantName = "Standard",
                    UrlSlug = $"product-update-color-{uniqueId}",
                    Price = 1000,
                    CoverImageUrl = "https://example.com/old.jpg"
                }]
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var variantId = product.ProductVariants.First().Id;
        var payload = new
        {
            name = product.Name,
            category_id = category.Id,
            brand_id = brand.Id,
            variants = new[]
            {
                new
            {
                id = variantId,
                variant_name = "Standard",
                url_slug = $"product-update-color-new-{uniqueId}",
                price = 1200,
                colors = new[]
                {
                    new
                {
                    color_name = "Blue",
                    color_code = "#0000FF",
                    cover_image_url = "https://example.com/blue.jpg"
                }
                }
            }
            }
        };
        var response = await _client.PutAsync(
            $"/api/v1/product/{product.Id}",
            JsonContent.Create(payload),
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var variant = await verifyDb.ProductVariants
            .Include(v => v.ProductVariantColors)
            .FirstAsync(v => v.Id == variantId, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        variant.CoverImageUrl.Should().BeNull();
        variant.ProductVariantColors.Should().NotBeEmpty();
        var variantColor = variant.ProductVariantColors.First();
        variantColor.ColorName.Should().Be("Blue");
        variantColor.ColorCode.Should().Be("#0000FF");
        variantColor.CoverImageUrl.Should().Be("https://example.com/blue.jpg");
    }
    #pragma warning restore CRR0035
}

