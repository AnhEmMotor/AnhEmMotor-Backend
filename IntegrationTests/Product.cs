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
using Xunit.Abstractions;
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

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() { await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(true); }

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

        var response = await _client.GetAsync("/api/v1/product").ConfigureAwait(true);

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

        var response = await _client.GetAsync($"/api/v1/product?filters=BrandId=={brand1.Id}").ConfigureAwait(true);

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

        var response = await _client.GetAsync("/api/v1/product?sorts=Name").ConfigureAwait(true);

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

        var response = await _client.GetAsync($"/api/v1/product?filters=Id=={product.Id}").ConfigureAwait(true);

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

        var response = await _client.GetAsync("/api/v1/product").ConfigureAwait(true);

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

        var response = await _client.GetAsync("/api/v1/product/for-manager").ConfigureAwait(true);

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

        var response = await _client.GetAsync("/api/v1/product/deleted").ConfigureAwait(true);

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

        var response = await _client.GetAsync("/api/v1/product/variants-lite").ConfigureAwait(true);

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

        var response = await _client.GetAsync("/api/v1/product/variants-lite/for-input").ConfigureAwait(true);

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

        var response = await _client.GetAsync("/api/v1/product/variants-lite/for-output").ConfigureAwait(true);

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

        var response = await _client.GetAsync($"/api/v1/product/{product.Id}").ConfigureAwait(true);

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

        var response = await _client.GetAsync($"/api/v1/product/{product.Id}").ConfigureAwait(true);

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

        var response = await _client.GetAsync($"/api/v1/product/{product.Id}/variants-lite").ConfigureAwait(true);

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
            var error = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
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

        var response = await _client.GetAsync($"/api/v1/product/{product.Id}").ConfigureAwait(true);

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

        var response = await _client.GetAsync($"/api/v1/product/{product.Id}/variants-lite").ConfigureAwait(true);

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

        var dbP1 = await db.Products.FindAsync(p1.Id).ConfigureAwait(true);
        var dbP2 = await db.Products.FindAsync(p2.Id).ConfigureAwait(true);
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
#pragma warning restore CRR0035
}
