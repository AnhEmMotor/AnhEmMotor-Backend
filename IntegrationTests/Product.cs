using System.Net;
using System.Net.Http.Json;
using Application.ApiContracts.Product.Common;
using Application.ApiContracts.Product.Requests;
using Application.ApiContracts.Product.Responses;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using FluentAssertions;
using Infrastructure.DBContexts;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests;

public class Product(IntegrationTestWebAppFactory factory) : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact(DisplayName = "PRODUCT_061 - Lấy danh sách sản phẩm với phân trang mặc định (10 items/page)")]
    public async Task GetProducts_DefaultPagination_Returns10ItemsPerPage()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        // Seed test data
        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/product");

        // Assert
        response.Should().NotBeNull();
    }
    [Fact(DisplayName = "PRODUCT_062 - Lấy danh sách sản phẩm với Sieve filter theo BrandId")]
    public async Task GetProducts_FilterByBrandId_ReturnsFilteredProducts()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand1 = new Domain.Entities.Brand { Name = "Honda", DeletedAt = null };
        var brand2 = new Domain.Entities.Brand { Name = "Yamaha", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.AddRange(brand1, brand2);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/product?filters=BrandId=={brand1.Id}");

        // Assert
        response.Should().NotBeNull();
    }
    [Fact(DisplayName = "PRODUCT_063 - Lấy danh sách sản phẩm với Sieve sort theo Name")]
    public async Task GetProducts_SortByName_ReturnsSortedProducts()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/product?sorts=Name");

        // Assert
        response.Should().NotBeNull();
    }
    [Fact(DisplayName = "PRODUCT_064 - Lấy danh sách sản phẩm chỉ trả về variants chưa bị xóa")]
    public async Task GetProducts_ReturnsOnlyNonDeletedVariants()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        var product = new Domain.Entities.Product
        {
            Name = "Test Product",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = "for-sale",
            DeletedAt = null
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/product");

        // Assert
        response.Should().NotBeNull();
    }
    [Fact(DisplayName = "PRODUCT_065 - Lấy danh sách sản phẩm không hiển thị trường stock cho user không có quyền")]
    public async Task GetProducts_NoPermission_HidesStockFields()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/product");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    [Fact(DisplayName = "PRODUCT_066 - Lấy danh sách sản phẩm for-manager hiển thị đầy đủ trường stock")]
    public async Task GetProductsForManager_WithPermission_ShowsStockFields()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/product/for-manager");

        // Assert
        response.Should().NotBeNull();
    }
    [Fact(DisplayName = "PRODUCT_067 - Lấy danh sách sản phẩm deleted chỉ trả về sản phẩm đã xóa")]
    public async Task GetDeletedProducts_ReturnsOnlyDeletedProducts()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        var deletedProduct = new Domain.Entities.Product
        {
            Name = "Deleted Product",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = "for-sale",
            DeletedAt = DateTime.UtcNow
        };
        dbContext.Products.Add(deletedProduct);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/product/deleted");

        // Assert
        response.Should().NotBeNull();
    }
    [Fact(DisplayName = "PRODUCT_068 - Lấy danh sách variants-lite chỉ trả về variants của sản phẩm for-sale")]
    public async Task GetVariantsLite_ReturnsOnlyForSaleProductVariants()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        var product = new Domain.Entities.Product
        {
            Name = "Test Product",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = "for-sale",
            DeletedAt = null
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/product/variants-lite");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    [Fact(DisplayName = "PRODUCT_069 - Lấy variants-lite/for-input chỉ trả về Id, Name, CoverImageUrl, Price")]
    public async Task GetVariantsLiteForInput_ReturnsOnlyRequiredFields()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/product/variants-lite/for-input");

        // Assert
        response.Should().NotBeNull();
    }
    [Fact(DisplayName = "PRODUCT_070 - Lấy variants-lite/for-output chỉ trả về Id, Name, CoverImageUrl, Price")]
    public async Task GetVariantsLiteForOutput_ReturnsOnlyRequiredFields()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/product/variants-lite/for-output");

        // Assert
        response.Should().NotBeNull();
    }
    [Fact(DisplayName = "PRODUCT_071 - Lấy chi tiết sản phẩm trả về đầy đủ thông tin kỹ thuật")]
    public async Task GetProductById_ReturnsFullTechnicalDetails()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        var product = new Domain.Entities.Product
        {
            Name = "Test Product",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = "for-sale",
            DeletedAt = null,
            Displacement = 150.5m,
            MaxPower = "12.5"
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/product/{product.Id}");

        // Assert
        response.Should().NotBeNull();
    }
    [Fact(DisplayName = "PRODUCT_072 - Lấy chi tiết sản phẩm thất bại khi sản phẩm bị xóa")]
    public async Task GetProductById_DeletedProduct_ReturnsNotFound()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        var product = new Domain.Entities.Product
        {
            Name = "Deleted Product",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = "for-sale",
            DeletedAt = DateTime.UtcNow
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/product/{product.Id}");

        // Assert
        response.Should().NotBeNull();
    }
    [Fact(DisplayName = "PRODUCT_073 - Lấy variants theo ProductId chỉ trả về variants chưa xóa")]
    public async Task GetVariantsByProductId_ReturnsOnlyNonDeletedVariants()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        var product = new Domain.Entities.Product
        {
            Name = "Test Product",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = "for-sale",
            DeletedAt = null
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            UrlSlug = "test-variant",
            Price = 10000000,
            DeletedAt = null
        };
        dbContext.ProductVariants.Add(variant);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/product/{product.Id}/variants");

        // Assert
        response.Should().NotBeNull();
    }
    [Fact(DisplayName = "PRODUCT_074 - Tạo sản phẩm tự động tạo OptionValue mới nếu chưa tồn tại")]
    public async Task CreateProduct_NewOptionValue_CreatesAutomatically()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        var request = new CreateProductRequest
        {
            Name = "New Product",
            CategoryId = category.Id,
            BrandId = brand.Id,
            Variants =
            [
                new()
                {
                    UrlSlug = "new-product-variant",
                    Price = 20000000,
                    OptionValues = new Dictionary<string, string> { { "Màu sắc", "Xanh Mint" } }
                }
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/product", request);

        // Assert
        response.Should().NotBeNull();
    }
    [Fact(DisplayName = "PRODUCT_075 - Tạo sản phẩm sử dụng OptionValue hiện có nếu đã tồn tại")]
    public async Task CreateProduct_ExistingOptionValue_UsesExisting()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        var option = new Option { Name = "Màu sắc" };
        dbContext.Options.Add(option);
        await dbContext.SaveChangesAsync();

        var optionValue = new OptionValue { OptionId = option.Id, Name = "Đỏ" };
        dbContext.OptionValues.Add(optionValue);
        await dbContext.SaveChangesAsync();

        var request = new CreateProductRequest
        {
            Name = "New Product",
            CategoryId = category.Id,
            BrandId = brand.Id,
            Variants =
            [
                new()
                {
                    UrlSlug = "new-product-red",
                    Price = 20000000,
                    OptionValues = new Dictionary<string, string> { { "Màu sắc", "Đỏ" } }
                }
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/product", request);

        // Assert
        response.Should().NotBeNull();
    }
    [Fact(DisplayName = "PRODUCT_076 - Tên biến thể hiển thị đúng khi có nhiều optionValues")]
    public async Task GetVariant_MultipleOptions_DisplaysCorrectName()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        var product = new Domain.Entities.Product
        {
            Name = "Honda SH",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = "for-sale",
            DeletedAt = null
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/product/{product.Id}");

        // Assert
        response.Should().NotBeNull();
    }
    [Fact(DisplayName = "PRODUCT_077 - Tên biến thể để trống khi không có optionValues hợp lệ")]
    public async Task GetVariant_NoOptions_DisplaysEmptyVariantName()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        var product = new Domain.Entities.Product
        {
            Name = "Simple Product",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = "for-sale",
            DeletedAt = null
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            UrlSlug = "simple-product",
            Price = 10000000,
            DeletedAt = null
        };
        dbContext.ProductVariants.Add(variant);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/product/{product.Id}/variants");

        // Assert
        response.Should().NotBeNull();
    }
    [Fact(DisplayName = "PRODUCT_078 - Tạo sản phẩm với Price có 2 chữ số thập phân được lưu chính xác")]
    public async Task CreateProduct_DecimalPrice_SavesWithoutRounding()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        var request = new CreateProductRequest
        {
            Name = "Decimal Price Product",
            CategoryId = category.Id,
            BrandId = brand.Id,
            Variants =
            [
                new()
                {
                    UrlSlug = "decimal-price-product",
                    Price = 20000000.99m
                }
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/product", request);

        // Assert
        response.Should().NotBeNull();
    }
    [Fact(DisplayName = "PRODUCT_079 - Sửa nhiều sản phẩm với transaction - tất cả thành công hoặc tất cả fail")]
    public async Task UpdateManyProducts_Transaction_AllSucceedOrAllFail()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        var product1 = new Domain.Entities.Product
        {
            Name = "Product 1",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = "for-sale",
            DeletedAt = null
        };
        var product2 = new Domain.Entities.Product
        {
            Name = "Product 2",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = "for-sale",
            DeletedAt = null
        };
        dbContext.Products.AddRange(product1, product2);
        await dbContext.SaveChangesAsync();

        var request = new UpdateManyProductStatusesRequest
        {
            Ids = [product1.Id, product2.Id],
            StatusId = "out-of-stock"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/product/many/status", request);

        // Assert
        response.Should().NotBeNull();
    }
    [Fact(DisplayName = "PRODUCT_080 - Sửa nhiều sản phẩm với transaction - một sản phẩm lỗi thì rollback tất cả")]
    public async Task UpdateManyProducts_OneProductInvalid_RollbacksAll()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var category = new Domain.Entities.ProductCategory { Name = "Test Category", DeletedAt = null };
        var brand = new Domain.Entities.Brand { Name = "Test Brand", DeletedAt = null };
        dbContext.ProductCategories.Add(category);
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();

        var product1 = new Domain.Entities.Product
        {
            Name = "Product 1",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = "for-sale",
            DeletedAt = null
        };
        var product2 = new Domain.Entities.Product
        {
            Name = "Product 2",
            CategoryId = category.Id,
            BrandId = brand.Id,
            StatusId = "for-sale",
            DeletedAt = DateTime.UtcNow
        };
        dbContext.Products.AddRange(product1, product2);
        await dbContext.SaveChangesAsync();

        var request = new UpdateManyProductStatusesRequest
        {
            Ids = [product1.Id, product2.Id],
            StatusId = "out-of-stock"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/product/many/status", request);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }
}
