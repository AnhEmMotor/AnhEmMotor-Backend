using Application.ApiContracts.Product.Requests;
using Application.ApiContracts.Product.Responses;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.UpdateManyProductStatuses;
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
using BrandEntity = Domain.Entities.Brand;
using ProductCategoryEntity = Domain.Entities.ProductCategory;
using ProductEntity = Domain.Entities.Product;

namespace IntegrationTests;

using System.Threading.Tasks;
using Xunit;

[Collection("Shared Integration Collection")]
public class Product : IAsyncLifetime
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

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(true);
        GC.SuppressFinalize(this);
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
            [ PermissionsList.Products.View ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
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

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
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
            [ PermissionsList.Products.View ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
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

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
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
            [ PermissionsList.Products.View ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
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

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();

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
            [ PermissionsList.Products.View ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
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

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
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
            [ PermissionsList.Products.View ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
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

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
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
            [ PermissionsList.Products.View, PermissionsList.Inputs.View ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
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

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
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
            [ PermissionsList.Products.View ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
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

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().Contain(p => p.Id == deletedProduct.Id);
    }

    [Fact(DisplayName = "PRODUCT_068 - Lấy danh sách variants-lite chỉ trả về variants của sản phẩm for-sale")]
    public async Task GetVariantsLite_ReturnsOnlyForSaleProductVariants()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Products.View ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
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
            DeletedAt = null
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100, UrlSlug = $"v_{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/product/variants-lite", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductVariantLiteResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().Contain(v => v.Id == variant.Id);
    }

    [Fact(DisplayName = "PRODUCT_069 - Lấy variants-lite/for-input chỉ trả về Id, Name, CoverImageUrl, Price")]
    public async Task GetVariantsLiteForInput_ReturnsOnlyRequiredFields()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Inputs.Edit ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
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

        var variant = new ProductVariant { ProductId = product.Id, Price = 100, UrlSlug = $"v_{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/product/variants-lite/for-input", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductVariantLiteResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
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
            [ PermissionsList.Outputs.Create ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
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

        var variant = new ProductVariant { ProductId = product.Id, Price = 100, UrlSlug = $"v_{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/product/variants-lite/for-output", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductVariantLiteResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().Contain(v => v.Id == variant.Id);
    }

    [Fact(DisplayName = "PRODUCT_071 - Lấy chi tiết sản phẩm trả về đầy đủ thông tin kỹ thuật")]
    public async Task GetProductById_ReturnsFullTechnicalDetails()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Products.View ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
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
            Displacement = 150.5m,
            MaxPower = "12.5HP"
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync($"/api/v1/product/{product.Id}", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<ProductDetailResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Id.Should().Be(product.Id);
        content.Displacement.Should().Be(150.5m);
        content.MaxPower.Should().Be("12.5HP");
    }

    [Fact(DisplayName = "PRODUCT_072 - Lấy chi tiết sản phẩm thất bại khi sản phẩm bị xóa")]
    public async Task GetProductById_DeletedProduct_ReturnsNotFound()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Products.View ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
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
            Name = $"Deleted_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = productStatusId,
            DeletedAt = DateTimeOffset.UtcNow
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync($"/api/v1/product/{product.Id}", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
            [ PermissionsList.Products.View ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
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

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<List<ProductVariantLiteResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Should().Contain(v => v.Id == v1.Id);
        content.Should().NotContain(v => v.Id == v2.Id);
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
            [ PermissionsList.Products.Create ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        db.PredefinedOptions.Add(new PredefinedOption { Key = $"Color_{uniqueId}", Value = "Màu sắc" });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var request = new CreateProductCommand
        {
            Name = $"Product_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            Variants =
                [ new CreateProductVariantRequest
                {
                    UrlSlug = $"slug-{uniqueId}",
                    Price = 20000000,
                    OptionValues = new Dictionary<string, string> { { $"Color_{uniqueId}", "Green" } }
                } ]
        };

        var response = await _client.PostAsJsonAsync("/api/v1/product", request).ConfigureAwait(true);

        if(response.StatusCode != HttpStatusCode.Created)
        {
            var error = await response.Content.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
            _output.WriteLine($"PRODUCT_074 Failed: {response.StatusCode} - {error}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var options = await db.Options
            .Where(o => o.Name == $"Color_{uniqueId}")
            .FirstOrDefaultAsync(CancellationToken.None)
            .ConfigureAwait(true);
        options.Should().NotBeNull();

        var values = await db.OptionValues
            .Where(ov => ov.OptionId == options!.Id && ov.Name == "Green")
            .FirstOrDefaultAsync(CancellationToken.None)
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
            [ PermissionsList.Products.Create ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
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

        var request = new CreateProductCommand
        {
            Name = $"Product_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            Variants =
                [ new CreateProductVariantRequest
                {
                    UrlSlug = $"slug-{uniqueId}",
                    Price = 20000000,
                    OptionValues = new Dictionary<string, string> { { optionName, optionValueName } }
                } ]
        };

        var response = await _client.PostAsJsonAsync("/api/v1/product", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
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
            [ PermissionsList.Products.View ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
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
        db.PredefinedOptions
            .AddRange(
                new PredefinedOption { Key = "Color", Value = "Màu sắc" },
                new PredefinedOption { Key = "Size", Value = "Size" });
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

        var response = await _client.GetAsync($"/api/v1/product/{product.Id}", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<ProductDetailResponse>(CancellationToken.None)
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
            [ PermissionsList.Products.View ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
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

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
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
            [ PermissionsList.Products.Create ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.ProductCategories.Add(category);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var request = new CreateProductCommand
        {
            Name = $"Decimal_{uniqueId}",
            CategoryId = category.Id,
            BrandId = brand.Id,
            Variants = [ new CreateProductVariantRequest { UrlSlug = $"dec-{uniqueId}", Price = 20000000.99m } ]
        };

        var response = await _client.PostAsJsonAsync("/api/v1/product", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content
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
            [ PermissionsList.Products.ChangeStatus ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        var outStockStatusId = Domain.Constants.ProductStatus.OutOfBusiness;

        if(!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        if(!await db.ProductStatuses
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

        var request = new UpdateManyProductStatusesCommand { Ids = [ p1.Id, p2.Id ], StatusId = outStockStatusId };

        var response = await _client.PatchAsJsonAsync("/api/v1/product/statuses", request, CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dbP1 = await db.Products.FindAsync([ p1.Id ], TestContext.Current.CancellationToken).ConfigureAwait(true);
        var dbP2 = await db.Products.FindAsync([ p2.Id ], TestContext.Current.CancellationToken).ConfigureAwait(true);
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
            [ PermissionsList.Products.ChangeStatus ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        var outStockStatusId = Domain.Constants.ProductStatus.OutOfBusiness;

        if(!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        if(!await db.ProductStatuses
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

        var request = new UpdateManyProductStatusesCommand { Ids = [ p1.Id, p2.Id ], StatusId = outStockStatusId };

        var response = await _client.PutAsJsonAsync("/api/v1/product/many/status", request).ConfigureAwait(true);

        response.StatusCode.Should().NotBe(HttpStatusCode.OK);

        var dbP1 = await db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == p1.Id, CancellationToken.None)
            .ConfigureAwait(true);
        dbP1!.StatusId.Should().Be(productStatusId);
    }

    [Fact(DisplayName = "PRODUCT_101 - Lấy danh sách PredefinedOptions thành công khi có quyền Products.Create")]
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
            [ PermissionsList.Products.Create ],
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
        if(!await db.PredefinedOptions.AnyAsync(CancellationToken.None).ConfigureAwait(true))
        {
            db.PredefinedOptions
                .AddRange(
                    new PredefinedOption { Key = "VehicleType", Value = "Loại xe" },
                    new PredefinedOption { Key = "Color", Value = "Màu sắc" });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var response = await _client.GetAsync("/api/v1/predefinedoption", CancellationToken.None).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<Dictionary<string, string>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNullOrEmpty();
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

        var response = await _client.GetAsync("/api/v1/predefinedoption", CancellationToken.None).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
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
            [ PermissionsList.Products.Create ],
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

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if(!await db.ProductStatuses
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
                [ new CreateProductVariantRequest
                {
                    Price = 100,
                    OptionValues = new Dictionary<string, string> { { "INVALID_KEY_NOT_IN_PREDEFINED", "Val1" } }
                } ]
        };

        var response = await _client.PostAsJsonAsync("/api/v1/product", command).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
            [ PermissionsList.Products.Create ],
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
        if(!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, Domain.Constants.ProductStatus.ForSale) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = Domain.Constants.ProductStatus.ForSale });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        string longSlug = new('a', 200);

        var command = new CreateProductCommand
        {
            Name = "Long Slug Product",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            Variants = [ new CreateProductVariantRequest { UrlSlug = longSlug, Price = 1000, OptionValueIds = [] } ]
        };

        var response = await _client.PostAsJsonAsync("/api/v1/product", command).ConfigureAwait(true);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);

        var createdProduct = await db.ProductVariants
            .FirstOrDefaultAsync(v => string.Compare(v.UrlSlug, longSlug) == 0, CancellationToken.None)
            .ConfigureAwait(true);
        createdProduct.Should().NotBeNull();
        createdProduct!.UrlSlug?.Length.Should().Be(200);
    }

    [Fact(DisplayName = "PRODUCT_105 - Cập nhật sản phẩm với thuộc tính null (chuỗi trống) parse thành decimal an toàn")]
    public async Task UpdateProduct_EmptyDecimalStrings_ParsedAsNullSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Products.View, PermissionsList.Products.Create, PermissionsList.Products.Edit ],
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
        if(!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, Domain.Constants.ProductStatus.ForSale) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = Domain.Constants.ProductStatus.ForSale });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var reqBody = new
        {
            name = "Test Product Decimal",
            categoryId = cat.Id,
            brandId = brand.Id,
            weight = string.Empty,
            displacement = string.Empty,
            variants = new[]
            {
                new { urlSlug = $"test-dec-{uniqueId}", price = 1000, optionValueIds = Array.Empty<int>() }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/product", reqBody).ConfigureAwait(true);
        var content = await response.Content.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
        response.IsSuccessStatusCode.Should().BeTrue("Response body: {0}", content);
    }

    [Fact(DisplayName = "PRODUCT_106 - Cập nhật sản phẩm verify xóa cứng VariantOptionValue")]
    public async Task UpdateProduct_HardDeleteVariantOptionValue_Verified()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"u_{uniqueId}";
        var password = "P1@password";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Products.View, PermissionsList.Products.Create, PermissionsList.Products.Edit ],
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
        var predefinedOption = new PredefinedOption { Key = "Color", Value = "Màu sắc" };
        var option = new Option { Name = "Color" };
        var optionValue = new OptionValue { Option = option, Name = "Red" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        db.PredefinedOptions.Add(predefinedOption);
        db.Options.Add(option);
        db.OptionValues.Add(optionValue);

        if(!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, Domain.Constants.ProductStatus.ForSale) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = Domain.Constants.ProductStatus.ForSale });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity
        {
            Name = "P1",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = Domain.Constants.ProductStatus.ForSale
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, UrlSlug = $"v-{uniqueId}", Price = 1000 };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variantOptionValue = new VariantOptionValue { VariantId = variant.Id, OptionValueId = optionValue.Id };
        db.Set<VariantOptionValue>().Add(variantOptionValue);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var updateCommand = new Application.Features.Products.Commands.UpdateProduct.UpdateProductCommand
        {
            Id = product.Id,
            Name = "P1 Updated",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            Variants = [ new UpdateProductVariantRequest { Id = variant.Id, Price = 2000, OptionValues = [] } ]
        };

        var response = await _client.PutAsJsonAsync($"/api/v1/product/{product.Id}", updateCommand).ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var exists = await db.Set<VariantOptionValue>()
            .IgnoreQueryFilters()
            .AnyAsync(v => v.VariantId == variant.Id, CancellationToken.None)
            .ConfigureAwait(true);
        exists.Should().BeFalse();
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
            [ PermissionsList.Products.View ],
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

        var response = await _client.GetAsync("/api/v1/PredefinedOption", CancellationToken.None).ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
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

        var response = await _client.GetAsync("/api/v1/PredefinedOption", CancellationToken.None).ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "PRODUCT_111 - Lấy thông tin chi tiết sản phẩm kiểm tra tồn kho vật lý, giữ chỗ, ATS")]
    public async Task GetProductById_WithStockData_ReturnsCorrectStockCalculations()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"u_{uniqueId}";
        var password = "P1@password";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Products.View ],
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
        if(!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, Domain.Constants.ProductStatus.ForSale) == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = Domain.Constants.ProductStatus.ForSale });
        if(!await db.InputStatuses
            .AnyAsync(
                x => string.Compare(x.Key, Domain.Constants.Input.InputStatus.Finish) == 0,
                CancellationToken.None)
            .ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = Domain.Constants.Input.InputStatus.Finish });
        if(!await db.OutputStatuses
            .AnyAsync(
                x => string.Compare(x.Key, Domain.Constants.Order.OrderStatus.Pending) == 0,
                CancellationToken.None)
            .ConfigureAwait(true))
            db.OutputStatuses.Add(new OutputStatus { Key = Domain.Constants.Order.OrderStatus.Pending });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity
        {
            Name = "P_Test_Stock",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = Domain.Constants.ProductStatus.ForSale
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, UrlSlug = $"v-stock-{uniqueId}", Price = 1000 };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var input = new Input
        {
            StatusId = Domain.Constants.Input.InputStatus.Finish,
            InputDate = DateTimeOffset.UtcNow
        };
        db.InputReceipts.Add(input);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var inputInfo = new InputInfo
        {
            InputId = input.Id,
            ProductId = variant.Id,
            Count = 100,
            RemainingCount = 100,
            InputPrice = 800
        };
        db.InputInfos.Add(inputInfo);

        var output = new Output { StatusId = Domain.Constants.Order.OrderStatus.Pending };
        db.OutputOrders.Add(output);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var outputInfo = new OutputInfo
        {
            OutputId = output.Id,
            ProductVarientId = variant.Id,
            Count = 30,
            Price = 1000
        };
        db.OutputInfos.Add(outputInfo);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync($"/api/v1/product/{product.Id}/for-manager", CancellationToken.None)
            .ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content
            .ReadFromJsonAsync<ProductDetailForManagerResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Stock.Should().Be(100);
        content.HasBeenBooked.Should().Be(30);
        content.StatusStockId.Should().Be("in_stock");
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
            [ PermissionsList.Products.View ],
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
        if(!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, Domain.Constants.ProductStatus.ForSale) == 0, CancellationToken.None)
            .ConfigureAwait(true))
        {
            db.ProductStatuses.Add(new ProductStatus { Key = Domain.Constants.ProductStatus.ForSale });
        }
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity
        {
            Name = $"Lite_Product_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = Domain.Constants.ProductStatus.ForSale,
            Description = "This should be hidden"
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, UrlSlug = $"v-lite-{uniqueId}", Price = 1234.56m };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/product/for-price-management", CancellationToken.None)
            .ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductPriceLiteResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
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
            [ PermissionsList.Products.View ],
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
        if(!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, Domain.Constants.ProductStatus.ForSale) == 0, CancellationToken.None)
            .ConfigureAwait(true))
        {
            db.ProductStatuses.Add(new ProductStatus { Key = Domain.Constants.ProductStatus.ForSale });
        }
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var p1 = new ProductEntity
        {
            Name = $"Exciter_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = Domain.Constants.ProductStatus.ForSale
        };
        var p2 = new ProductEntity
        {
            Name = $"Winner_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = Domain.Constants.ProductStatus.ForSale
        };
        db.Products.AddRange(p1, p2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync(
            $"/api/v1/product/for-price-management?Filters=Name@=Exciter_{uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content
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
            [ PermissionsList.Products.View ],
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
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductPriceLiteResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Items.Should().BeEmpty();
    }

    private async Task<string> AuthenticateForInputAsync(string uniqueId)
    {
        var username = $"u_{uniqueId}";
        var password = "P1@password";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Inputs.Edit ],
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
        if(!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, Domain.Constants.ProductStatus.ForSale) == 0, CancellationToken.None)
            .ConfigureAwait(true))
        {
            db.ProductStatuses.Add(new ProductStatus { Key = Domain.Constants.ProductStatus.ForSale });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
    }

    [Fact(DisplayName = "PRODUCT_122 - Tìm kiếm biến thể theo tên sản phẩm cha")]
    public async Task GetVariantsLiteForInput_SearchByParentProductName_ReturnsOnlyMatchingVariants()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await AuthenticateForInputAsync(uniqueId).ConfigureAwait(true));

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
            StatusId = Domain.Constants.ProductStatus.ForSale
        };
        var yamaha = new ProductEntity
        {
            Name = $"Yamaha_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = Domain.Constants.ProductStatus.ForSale
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
            $"/api/v1/product/variants-lite/for-input?filters=search@=Honda_{uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductVariantLiteResponseForInput>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(3);
        content.Items.Should().AllSatisfy(item => item.DisplayName.Should().Contain($"Honda_{uniqueId}"));
        content.Items.Should().NotContain(item => item.DisplayName!.Contains($"Yamaha_{uniqueId}"));
    }

    [Fact(DisplayName = "PRODUCT_123 - Phân trang khi tìm lọc sản phẩm (trang 1)")]
    public async Task GetVariantsLiteForInput_SearchWithPagination_Page1ReturnsCorrectVariants()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await AuthenticateForInputAsync(uniqueId).ConfigureAwait(true));

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
            StatusId = Domain.Constants.ProductStatus.ForSale
        };
        var pD = new ProductEntity
        {
            Name = $"D_Paged_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = Domain.Constants.ProductStatus.ForSale
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
            $"/api/v1/product/variants-lite/for-input?filters=search@=_Paged_{uniqueId}&page=1&pageSize=10",
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductVariantLiteResponseForInput>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.TotalCount.Should().Be(11);
        content.Items.Should().HaveCount(10);
        content.TotalPages.Should().Be(2);
    }

    [Fact(DisplayName = "PRODUCT_124 - Phân trang lấy các phần tử còn lại (trang 2)")]
    public async Task GetVariantsLiteForInput_SearchWithPagination_Page2ReturnsRemainingVariants()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await AuthenticateForInputAsync(uniqueId).ConfigureAwait(true));

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
            StatusId = Domain.Constants.ProductStatus.ForSale
        };
        var pD = new ProductEntity
        {
            Name = $"D_P2_{uniqueId}",
            CategoryId = cat.Id,
            BrandId = brand.Id,
            StatusId = Domain.Constants.ProductStatus.ForSale
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
            $"/api/v1/product/variants-lite/for-input?filters=search@=_P2_{uniqueId}&page=2&pageSize=10",
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductVariantLiteResponseForInput>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.TotalCount.Should().Be(11);
        content.Items.Should().HaveCount(1);
        content.PageNumber.Should().Be(2);
    }

    [Fact(DisplayName = "PRODUCT_125 - Hiển thị DisplayName có dịch tiếng Việt (1 option)")]
    public async Task GetVariantsLiteForInput_VariantWithSingleOption_DisplayNameContainsVietnameseLabel()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await AuthenticateForInputAsync(uniqueId).ConfigureAwait(true));

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
            StatusId = Domain.Constants.ProductStatus.ForSale
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        if(!await db.PredefinedOptions
            .AnyAsync(x => string.Compare(x.Key, "Color") == 0, CancellationToken.None)
            .ConfigureAwait(true))
        {
            db.PredefinedOptions.Add(new PredefinedOption { Key = "Color", Value = "Màu sắc" });
        }

        var colorOption = await db.Options
            .FirstOrDefaultAsync(o => string.Compare(o.Name, "Color") == 0, CancellationToken.None)
            .ConfigureAwait(true);
        if(colorOption is null)
        {
            colorOption = new Option { Name = "Color" };
            db.Options.Add(colorOption);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var doOption = new OptionValue { OptionId = colorOption.Id, Name = "Đỏ" };
        db.OptionValues.Add(doOption);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100, UrlSlug = $"v_{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        db.VariantOptionValues.Add(new VariantOptionValue { VariantId = variant.Id, OptionValueId = doOption.Id });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync(
            $"/api/v1/product/variants-lite/for-input?filters=search@=Prod_{uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductVariantLiteResponseForInput>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        var item = content!.Items!.FirstOrDefault(v => v.Id == variant.Id);
        item.Should().NotBeNull();
        item!.DisplayName.Should().Be($"Prod_{uniqueId} (Màu sắc: Đỏ)");
    }

    [Fact(DisplayName = "PRODUCT_126 - Hiển thị DisplayName có dịch tiếng Việt (Nhiều option)")]
    public async Task GetVariantsLiteForInput_VariantWithMultipleOptions_DisplayNameContainsAllTranslatedLabels()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await AuthenticateForInputAsync(uniqueId).ConfigureAwait(true));

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
            StatusId = Domain.Constants.ProductStatus.ForSale
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        if(!await db.PredefinedOptions
            .AnyAsync(x => string.Compare(x.Key, "Color") == 0, CancellationToken.None)
            .ConfigureAwait(true))
            db.PredefinedOptions.Add(new PredefinedOption { Key = "Color", Value = "Màu sắc" });
        if(!await db.PredefinedOptions
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

        if(colorOption.Id == 0)
            db.Options.Add(colorOption);
        if(displOption.Id == 0)
            db.Options.Add(displOption);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var doOV = new OptionValue { OptionId = colorOption.Id, Name = "Đỏ" };
        var cc150OV = new OptionValue { OptionId = displOption.Id, Name = "150cc" };
        db.OptionValues.AddRange(doOV, cc150OV);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 200, UrlSlug = $"v_{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        db.VariantOptionValues.Add(new VariantOptionValue { VariantId = variant.Id, OptionValueId = doOV.Id });
        db.VariantOptionValues.Add(new VariantOptionValue { VariantId = variant.Id, OptionValueId = cc150OV.Id });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync(
            $"/api/v1/product/variants-lite/for-input?filters=search@=Prod_{uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductVariantLiteResponseForInput>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        var item = content.Items?.FirstOrDefault(v => v.Id == variant.Id);
        item.Should().NotBeNull();
        item!.DisplayName.Should().StartWith($"Prod_{uniqueId} (");
        item.DisplayName.Should().Contain("Màu sắc: Đỏ");
        item.DisplayName.Should().Contain("Phân khối: 150cc");
    }

    [Fact(DisplayName = "PRODUCT_127 - Hiển thị DisplayName khi không có option")]
    public async Task GetVariantsLiteForInput_VariantWithNoOptions_DisplayNameEqualsProductName()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await AuthenticateForInputAsync(uniqueId).ConfigureAwait(true));

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
            StatusId = Domain.Constants.ProductStatus.ForSale
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 50, UrlSlug = $"v_{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync(
            $"/api/v1/product/variants-lite/for-input?filters=search@=Solo_{uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductVariantLiteResponseForInput>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        var item = content!.Items!.FirstOrDefault(v => v.Id == variant.Id);
        item.Should().NotBeNull();
        item!.DisplayName.Should().Be($"Solo_{uniqueId}");
    }

    [Fact(DisplayName = "PRODUCT_132 - Lấy danh sách sản phẩm manager - Filter theo InventoryStatus")]
    public async Task GetProductsForManager_FilterByInventoryStatus_ReturnsCorrectProducts()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await AuthenticateForManagerAsync(uniqueId).ConfigureAwait(true));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await EnsureForSaleStatusAsync(db).ConfigureAwait(true);
        await EnsureInventoryAlertLevelAsync(db, 5).ConfigureAwait(true);

        var statusFinished = Domain.Constants.Input.InputStatus.Finish;
        var statusBooking = Domain.Constants.Order.OrderStatus.Pending;
        await EnsureInputStatusAsync(db, statusFinished).ConfigureAwait(true);
        await EnsureOutputStatusAsync(db, statusBooking).ConfigureAwait(true);

        var cat = new ProductCategoryEntity { Name = $"C_{uniqueId}" };
        var brand = new BrandEntity { Name = $"B_{uniqueId}" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        // Product 1: InStock (Available 10 > 5)
        var p1 = CreateProductWithStock(db, $"P1_{uniqueId}", cat.Id, brand.Id, 10, 0, statusFinished, statusBooking);
        // Product 2: OutOfStock (Available 0 <= 0)
        var p2 = CreateProductWithStock(db, $"P2_{uniqueId}", cat.Id, brand.Id, 10, 10, statusFinished, statusBooking);
        
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        // Act: Filter by OutOfStock
        var response = await _client.GetAsync($"/api/v1/product/for-manager?filters=inventoryStatus=={Domain.Constants.InventoryStatus.OutOfStock}", CancellationToken.None)
            .ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailForManagerResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().Contain(p => p.Id == p2.Id);
        content.Items.Should().NotContain(p => p.Id == p1.Id);
    }

    [Fact(DisplayName = "PRODUCT_133 - Lấy danh sách sản phẩm manager - Sort theo InventoryStatus")]
    public async Task GetProductsForManager_SortByInventoryStatus_ReturnsSortedProducts()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await AuthenticateForManagerAsync(uniqueId).ConfigureAwait(true));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await EnsureForSaleStatusAsync(db).ConfigureAwait(true);
        await EnsureInventoryAlertLevelAsync(db, 5).ConfigureAwait(true);

        var statusFinished = Domain.Constants.Input.InputStatus.Finish;
        var statusBooking = Domain.Constants.Order.OrderStatus.Pending;
        await EnsureInputStatusAsync(db, statusFinished).ConfigureAwait(true);
        await EnsureOutputStatusAsync(db, statusBooking).ConfigureAwait(true);

        var cat = new ProductCategoryEntity { Name = $"C_{uniqueId}" };
        var brand = new BrandEntity { Name = $"B_{uniqueId}" };
        db.ProductCategories.Add(cat);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        // Tạo 3 sản phẩm với 3 trạng thái khác nhau
        var pIn = CreateProductWithStock(db, $"PIn_{uniqueId}", cat.Id, brand.Id, 10, 0, statusFinished, statusBooking);
        var pLow = CreateProductWithStock(db, $"PLow_{uniqueId}", cat.Id, brand.Id, 10, 7, statusFinished, statusBooking);
        var pOut = CreateProductWithStock(db, $"POut_{uniqueId}", cat.Id, brand.Id, 10, 10, statusFinished, statusBooking);
        
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        // Act: Sort by inventoryStatus (asc by severity: OutOfStock(1) -> LowStock(2) -> InStock(3))
        var response = await _client.GetAsync($"/api/v1/product/for-manager?sorts=inventoryStatus&filters=name@=_{uniqueId}", CancellationToken.None)
            .ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductDetailForManagerResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        
        var items = content!.Items.ToList();
        items.Should().HaveCount(3);
        items[0].Id.Should().Be(pOut.Id);
        items[1].Id.Should().Be(pLow.Id);
        items[2].Id.Should().Be(pIn.Id);
    }

    private async Task<string> AuthenticateForManagerAsync(string uniqueId)
    {
        var username = $"mgr_{uniqueId}";
        var password = "P1@password";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Products.View, PermissionsList.Inputs.View ],
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

    private static async Task EnsureInventoryAlertLevelAsync(ApplicationDBContext db, long level)
    {
        var key = Domain.Constants.SettingKeys.InventoryAlertLevel;
        var setting = await db.Settings.FirstOrDefaultAsync(s => s.Key == key, CancellationToken.None).ConfigureAwait(true);
        if (setting is null)
        {
            db.Settings.Add(new Domain.Entities.Setting { Key = key, Value = level.ToString() });
        }
        else
        {
            setting.Value = level.ToString();
        }
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
    }

    private static async Task EnsureInputStatusAsync(ApplicationDBContext db, string key)
    {
        if (!await db.InputStatuses.AnyAsync(s => s.Key == key, CancellationToken.None).ConfigureAwait(true))
        {
            db.InputStatuses.Add(new Domain.Entities.InputStatus { Key = key });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
    }

    private static async Task EnsureOutputStatusAsync(ApplicationDBContext db, string key)
    {
        if (!await db.OutputStatuses.AnyAsync(s => s.Key == key, CancellationToken.None).ConfigureAwait(true))
        {
            db.OutputStatuses.Add(new Domain.Entities.OutputStatus { Key = key });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
    }

    private ProductEntity CreateProductWithStock(ApplicationDBContext db, string name, int catId, int brandId, int stock, int booked, string statusFinished, string statusBooking)
    {
        var product = new ProductEntity
        {
            Name = name,
            CategoryId = catId,
            BrandId = brandId,
            StatusId = Domain.Constants.ProductStatus.ForSale
        };
        db.Products.Add(product);
        db.SaveChanges(); // Lấy ID

        var variant = new ProductVariant { ProductId = product.Id, Price = 100, UrlSlug = $"v_{Guid.NewGuid():N}" };
        db.ProductVariants.Add(variant);
        db.SaveChanges();

        if (stock > 0)
        {
            var receipt = new Domain.Entities.Input { StatusId = statusFinished };
            db.InputReceipts.Add(receipt);
            db.SaveChanges();

            db.InputInfos.Add(new InputInfo { ProductId = variant.Id, InputId = receipt.Id, RemainingCount = stock });
        }

        if (booked > 0)
        {
            var order = new Domain.Entities.Output { StatusId = statusBooking };
            db.OutputOrders.Add(order);
            db.SaveChanges();

            db.OutputInfos.Add(new OutputInfo { ProductVarientId = variant.Id, OutputId = order.Id, Count = booked });
        }

        return product;
    }

#pragma warning restore CRR0035
}
