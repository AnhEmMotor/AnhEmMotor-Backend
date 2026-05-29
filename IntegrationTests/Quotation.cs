using Application.ApiContracts.Quotation.Responses;
using Application.Features.Quotations.Commands.CreateQuotation;
using Application.Features.Quotations.Commands.UpdateQuotation;
using Domain.Constants.Permission.Permissions;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using BrandEntity = Domain.Entities.Brand;
using ProductCategoryEntity = Domain.Entities.ProductCategory;
using ProductEntity = Domain.Entities.Product;
using SupplierEntity = Domain.Entities.Supplier;
using QuotationEntity = Domain.Entities.Quotation;

namespace IntegrationTests;

public class Quotation : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public Quotation(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.ResetDatabaseAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private async Task<(SupplierEntity Supplier, ProductVariant Variant, ProductVariantColor Color)> SeedBaseDataAsync(
        ApplicationDBContext db,
        string uniqueId, CancellationToken cancellationToken)
    {
        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, cancellationToken).ConfigureAwait(true))
        {
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });
        }

        var productStatusId = Domain.Constants.Product.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, cancellationToken).ConfigureAwait(true))
        {
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        }

        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var supplier = new SupplierEntity
        {
            Name = $"Supplier_{uniqueId}",
            StatusId = supplierStatusId,
            Phone = "0123456789",
            Email = $"supplier_{uniqueId}@example.com"
        };
        db.Suppliers.Add(supplier);

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);

        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var product = new ProductEntity
        {
            Name = $"Product_{uniqueId}",
            BrandId = brand.Id,
            CategoryId = category.Id,
            StatusId = productStatusId
        };
        db.Products.Add(product);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Price = 1000,
            UrlSlug = $"slug-{uniqueId}"
        };
        db.ProductVariants.Add(variant);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var color = new ProductVariantColor
        {
            ProductVariantId = variant.Id,
            ColorName = "Red",
            ColorCode = "#FF0000"
        };
        db.ProductVariantColors.Add(color);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        return (supplier, variant, color);
    }

    [Fact(DisplayName = "QUO_022 - POST /api/v1/Quotations: Tạo mới báo giá thành công (Nháp)")]
    public async Task CreateQuotation_Success_ReturnsCreated()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Password123!";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Quotations.Create],
            TestContext.Current.CancellationToken).ConfigureAwait(true);

        var login = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, TestContext.Current.CancellationToken).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var seeded = await SeedBaseDataAsync(db, uniqueId, TestContext.Current.CancellationToken).ConfigureAwait(true);

        var command = new CreateQuotationCommand
        {
            SupplierId = seeded.Supplier.Id,
            Notes = "Initial Note",
            Products =
            [
                new()
                {
                    ProductVariantId = seeded.Variant.Id.ToString(),
                    ProductVarientColorId = seeded.Color.Id.ToString(),
                    QuotePrice = 500,
                    Note = "Item note"
                }
            ]
        };

        var response = await HttpClientJsonExtensions.PostAsJsonAsync(_client, "/api/v1/Quotations", command, TestContext.Current.CancellationToken).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<QuotationDetailResponse>(TestContext.Current.CancellationToken).ConfigureAwait(true);
        result.Should().NotBeNull();
        result!.Status.Should().Be("draft");

        using var assertScope = _factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var dbQuotation = await assertDb.Quotations.FirstOrDefaultAsync(q => q.Id == result.Id, TestContext.Current.CancellationToken).ConfigureAwait(true);
        dbQuotation.Should().NotBeNull();
        dbQuotation!.Status.Should().Be("draft");
    }

    [Fact(DisplayName = "QUO_023 - POST /api/v1/Quotations: Tạo mới báo giá thất bại do không có quyền")]
    public async Task CreateQuotation_NoPermission_ReturnsForbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Password123!";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [],
            TestContext.Current.CancellationToken).ConfigureAwait(true);

        var login = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, TestContext.Current.CancellationToken).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var command = new CreateQuotationCommand
        {
            SupplierId = 1,
            Products = []
        };

        var response = await HttpClientJsonExtensions.PostAsJsonAsync(_client, "/api/v1/Quotations", command, TestContext.Current.CancellationToken).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "QUO_024 - GET /api/v1/Quotations/{id}: Xem chi tiết báo giá thành công")]
    public async Task GetQuotationById_Success_ReturnsOk()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Password123!";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Quotations.View],
            TestContext.Current.CancellationToken).ConfigureAwait(true);

        var login = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, TestContext.Current.CancellationToken).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var seeded = await SeedBaseDataAsync(db, uniqueId, TestContext.Current.CancellationToken).ConfigureAwait(true);

        var quotation = new QuotationEntity
        {
            SupplierId = seeded.Supplier.Id,
            Status = "draft",
            Note = "Test Note"
        };
        db.Quotations.Add(quotation);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var response = await _client.GetAsync($"/api/v1/Quotations/{quotation.Id}", TestContext.Current.CancellationToken).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<QuotationDetailResponse>(TestContext.Current.CancellationToken).ConfigureAwait(true);
        result.Should().NotBeNull();
        result!.Id.Should().Be(quotation.Id);
    }

    [Fact(DisplayName = "QUO_025 - PUT /api/v1/Quotations/{id}: Cập nhật toàn bộ thông tin báo giá Nháp thành công")]
    public async Task UpdateQuotation_Draft_Success_ReturnsOk()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Password123!";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Quotations.Edit],
            TestContext.Current.CancellationToken).ConfigureAwait(true);

        var login = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, TestContext.Current.CancellationToken).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var seeded = await SeedBaseDataAsync(db, uniqueId, TestContext.Current.CancellationToken).ConfigureAwait(true);

        var quotation = new QuotationEntity
        {
            SupplierId = seeded.Supplier.Id,
            Status = "draft",
            Note = "Old Note"
        };
        db.Quotations.Add(quotation);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var command = new UpdateQuotationCommand
        {
            SupplierId = seeded.Supplier.Id,
            Notes = "New Note",
            Products =
            [
                new()
                {
                    ProductVariantId = seeded.Variant.Id.ToString(),
                    ProductVarientColorId = seeded.Color.Id.ToString(),
                    QuotePrice = 600,
                    Note = "New row note"
                }
            ]
        };

        var response = await HttpClientJsonExtensions.PutAsJsonAsync(_client, $"/api/v1/Quotations/{quotation.Id}", command, TestContext.Current.CancellationToken).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<QuotationDetailResponse>(TestContext.Current.CancellationToken).ConfigureAwait(true);
        result.Should().NotBeNull();
        result!.Notes.Should().Be("New Note");

        using var assertScope = _factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var dbQuotation = await assertDb.Quotations.Include(q => q.QuotationProductRows).FirstOrDefaultAsync(q => q.Id == quotation.Id, TestContext.Current.CancellationToken).ConfigureAwait(true);
        dbQuotation!.Note.Should().Be("New Note");
        dbQuotation.QuotationProductRows.Should().HaveCount(1);
    }

    [Fact(DisplayName = "QUO_026 - PATCH /api/v1/Quotations/{id}/send: Gửi báo giá Nháp thành công")]
    public async Task SendQuotation_Success_ReturnsOk()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Password123!";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Quotations.Send],
            TestContext.Current.CancellationToken).ConfigureAwait(true);

        var login = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, TestContext.Current.CancellationToken).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var seeded = await SeedBaseDataAsync(db, uniqueId, TestContext.Current.CancellationToken).ConfigureAwait(true);

        var quotation = new QuotationEntity
        {
            SupplierId = seeded.Supplier.Id,
            Status = "draft"
        };
        db.Quotations.Add(quotation);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var response = await _client.PatchAsync($"/api/v1/Quotations/{quotation.Id}/send", null, TestContext.Current.CancellationToken).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<QuotationDetailResponse>(TestContext.Current.CancellationToken).ConfigureAwait(true);
        result!.Status.Should().Be("sent");

        using var assertScope = _factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var dbQuotation = await assertDb.Quotations.FirstOrDefaultAsync(q => q.Id == quotation.Id, TestContext.Current.CancellationToken).ConfigureAwait(true);
        dbQuotation!.Status.Should().Be("sent");
    }

    [Fact(DisplayName = "QUO_027 - PATCH /api/v1/Quotations/{id}/approve: Phê duyệt báo giá đã gửi thành công")]
    public async Task ApproveQuotation_Success_ReturnsOk()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Password123!";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Quotations.Approve],
            TestContext.Current.CancellationToken).ConfigureAwait(true);

        var login = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, TestContext.Current.CancellationToken).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var seeded = await SeedBaseDataAsync(db, uniqueId, TestContext.Current.CancellationToken).ConfigureAwait(true);

        var quotation = new QuotationEntity
        {
            SupplierId = seeded.Supplier.Id,
            Status = "sent"
        };
        db.Quotations.Add(quotation);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var response = await HttpClientJsonExtensions.PatchAsJsonAsync(
            _client, 
            $"/api/v1/Quotations/{quotation.Id}/status", 
            new { Status = "approved" }, 
            TestContext.Current.CancellationToken).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<QuotationDetailResponse>(TestContext.Current.CancellationToken).ConfigureAwait(true);
        result!.Status.Should().Be("approved");

        using var assertScope = _factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var dbQuotation = await assertDb.Quotations.FirstOrDefaultAsync(q => q.Id == quotation.Id, TestContext.Current.CancellationToken).ConfigureAwait(true);
        dbQuotation!.Status.Should().Be("approved");
    }

    [Fact(DisplayName = "QUO_028 - PUT /api/v1/Quotations/{id}: Cập nhật báo giá đã duyệt thành công chỉ khi thay đổi Ghi chú")]
    public async Task UpdateQuotation_ApprovedNotes_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Password123!";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Quotations.Edit],
            TestContext.Current.CancellationToken).ConfigureAwait(true);

        var login = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, TestContext.Current.CancellationToken).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var seeded = await SeedBaseDataAsync(db, uniqueId, TestContext.Current.CancellationToken).ConfigureAwait(true);

        var quotation = new QuotationEntity
        {
            SupplierId = seeded.Supplier.Id,
            Status = "approved",
            Note = "Old Note"
        };
        db.Quotations.Add(quotation);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var row = new QuotationProductRow
        {
            QuotationId = quotation.Id,
            ProductVariantId = seeded.Variant.Id,
            QuotePrice = 500
        };
        db.QuotationProductRows.Add(row);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var command = new UpdateQuotationCommand
        {
            SupplierId = seeded.Supplier.Id,
            Notes = "New Note Only",
            Products =
            [
                new()
                {
                    Id = row.Id,
                    ProductVariantId = seeded.Variant.Id.ToString(),
                    QuotePrice = 500
                }
            ]
        };

        var response = await HttpClientJsonExtensions.PutAsJsonAsync(_client, $"/api/v1/Quotations/{quotation.Id}", command, TestContext.Current.CancellationToken).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<QuotationDetailResponse>(TestContext.Current.CancellationToken).ConfigureAwait(true);
        result!.Notes.Should().Be("New Note Only");
    }

    [Fact(DisplayName = "QUO_029 - PUT /api/v1/Quotations/{id}: Cập nhật báo giá đã duyệt thất bại nếu thay đổi giá hoặc sản phẩm")]
    public async Task UpdateQuotation_ApprovedModifyProducts_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Password123!";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Quotations.Edit],
            TestContext.Current.CancellationToken).ConfigureAwait(true);

        var login = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, TestContext.Current.CancellationToken).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var seeded = await SeedBaseDataAsync(db, uniqueId, TestContext.Current.CancellationToken).ConfigureAwait(true);

        var quotation = new QuotationEntity
        {
            SupplierId = seeded.Supplier.Id,
            Status = "approved",
            Note = "Old Note"
        };
        db.Quotations.Add(quotation);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var row = new QuotationProductRow
        {
            QuotationId = quotation.Id,
            ProductVariantId = seeded.Variant.Id,
            QuotePrice = 500
        };
        db.QuotationProductRows.Add(row);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var command = new UpdateQuotationCommand
        {
            SupplierId = seeded.Supplier.Id,
            Notes = "New Note",
            Products =
            [
                new()
                {
                    Id = row.Id,
                    ProductVariantId = seeded.Variant.Id.ToString(),
                    QuotePrice = 600
                }
            ]
        };

        var response = await HttpClientJsonExtensions.PutAsJsonAsync(_client, $"/api/v1/Quotations/{quotation.Id}", command, TestContext.Current.CancellationToken).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "QUO_030 - DELETE /api/v1/Quotations/{id}: Xóa báo giá Nháp thành công")]
    public async Task DeleteQuotation_Draft_ReturnsNoContent()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Password123!";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Quotations.Delete],
            TestContext.Current.CancellationToken).ConfigureAwait(true);

        var login = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, TestContext.Current.CancellationToken).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var seeded = await SeedBaseDataAsync(db, uniqueId, TestContext.Current.CancellationToken).ConfigureAwait(true);

        var quotation = new QuotationEntity
        {
            SupplierId = seeded.Supplier.Id,
            Status = "draft"
        };
        db.Quotations.Add(quotation);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var response = await _client.DeleteAsync($"/api/v1/Quotations/{quotation.Id}", TestContext.Current.CancellationToken).ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.OK);

        using var assertScope = _factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var dbQuotation = await assertDb.Quotations.FirstOrDefaultAsync(q => q.Id == quotation.Id, TestContext.Current.CancellationToken).ConfigureAwait(true);
        dbQuotation.Should().BeNull();
    }

    [Fact(DisplayName = "QUO_031 - DELETE /api/v1/Quotations/{id}: Xóa báo giá đã duyệt thành công nếu có quyền duyệt")]
    public async Task DeleteQuotation_ApprovedWithApprovePermission_ReturnsNoContent()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Password123!";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Quotations.Delete, Quotations.Approve],
            TestContext.Current.CancellationToken).ConfigureAwait(true);

        var login = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, TestContext.Current.CancellationToken).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var seeded = await SeedBaseDataAsync(db, uniqueId, TestContext.Current.CancellationToken).ConfigureAwait(true);

        var quotation = new QuotationEntity
        {
            SupplierId = seeded.Supplier.Id,
            Status = "approved"
        };
        db.Quotations.Add(quotation);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var response = await _client.DeleteAsync($"/api/v1/Quotations/{quotation.Id}", TestContext.Current.CancellationToken).ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.OK);

        using var assertScope = _factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var dbQuotation = await assertDb.Quotations.FirstOrDefaultAsync(q => q.Id == quotation.Id, TestContext.Current.CancellationToken).ConfigureAwait(true);
        dbQuotation.Should().BeNull();
    }

    [Fact(DisplayName = "QUO_032 - DELETE /api/v1/Quotations/{id}: Xóa báo giá đã duyệt thất bại nếu thiếu quyền duyệt")]
    public async Task DeleteQuotation_ApprovedWithoutApprovePermission_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Password123!";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Quotations.Delete],
            TestContext.Current.CancellationToken).ConfigureAwait(true);

        var login = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, TestContext.Current.CancellationToken).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var seeded = await SeedBaseDataAsync(db, uniqueId, TestContext.Current.CancellationToken).ConfigureAwait(true);

        var quotation = new QuotationEntity
        {
            SupplierId = seeded.Supplier.Id,
            Status = "approved"
        };
        db.Quotations.Add(quotation);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var response = await _client.DeleteAsync($"/api/v1/Quotations/{quotation.Id}", TestContext.Current.CancellationToken).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var assertScope = _factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var dbQuotation = await assertDb.Quotations.FirstOrDefaultAsync(q => q.Id == quotation.Id, TestContext.Current.CancellationToken).ConfigureAwait(true);
        dbQuotation.Should().NotBeNull();
    }
}
