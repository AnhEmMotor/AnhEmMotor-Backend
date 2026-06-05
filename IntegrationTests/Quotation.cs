using Application.ApiContracts.PurchaseRequest.Responses;
using Domain.Constants.Permission.Permissions;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BrandEntity = Domain.Entities.Brand;
using ProductCategoryEntity = Domain.Entities.ProductCategory;
using ProductEntity = Domain.Entities.Product;
using SupplierEntity = Domain.Entities.Supplier;

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
        string uniqueId,
        CancellationToken cancellationToken)
    {
        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses
            .AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, cancellationToken)
            .ConfigureAwait(true))
        {
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });
        }

        var productStatusId = Domain.Constants.Product.ProductStatus.ForSale;
        if (!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, cancellationToken)
            .ConfigureAwait(true))
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

        var variant = new ProductVariant { ProductId = product.Id, Price = 1000, UrlSlug = $"slug-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var color = new ProductVariantColor { ProductVariantId = variant.Id, ColorName = "Red", ColorCode = "#FF0000" };
        db.ProductVariantColors.Add(color);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        return (supplier, variant, color);
    }

    [Fact(DisplayName = "PR_042 - Integration: Lấy danh sách báo giá đã duyệt theo variantId và colorId")]
    public async Task PR_042_GetApprovedPricesForVariant_Integration_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Password123!";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.Products.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var seeded = await SeedBaseDataAsync(db, uniqueId, TestContext.Current.CancellationToken).ConfigureAwait(true);

        var quotation = new Domain.Entities.Quotation
        {
            SupplierId = seeded.Supplier.Id,
            Status = "approved",
            QuotationProductRows =
            [
                new Domain.Entities.QuotationProductRow
                {
                    ProductVariantId = seeded.Variant.Id,
                    ProductVariantColorId = seeded.Color.Id,
                    QuotePrice = 120000,
                    Note = "Approved Price Note"
                }
            ]
        };
        db.Quotations.Add(quotation);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var response = await _client.GetAsync(
            $"/api/v1/Quotations/approved-prices?variantId={seeded.Variant.Id}&colorId={seeded.Color.Id}",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await response.Content
            .ReadFromJsonAsync<List<PurchaseRequestQuotedPriceResponse>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        data.Should().NotBeNull();
        data.Should().ContainSingle();
        data![0].ProductVariantId.Should().Be(seeded.Variant.Id);
        data[0].ProductVariantColorId.Should().Be(seeded.Color.Id);
        data[0].QuotePrice.Should().Be(120000);
        string.Compare(data[0].Note, "Approved Price Note").Should().Be(0);
    }
}
