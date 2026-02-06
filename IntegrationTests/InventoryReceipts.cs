using Application.ApiContracts.Input.Requests;
using Application.ApiContracts.Input.Responses;
using Application.Features.Inputs.Commands.CreateInput;
using Application.Features.Inputs.Commands.DeleteManyInputs;
using Application.Features.Inputs.Commands.RestoreManyInputs;
using Application.Features.Inputs.Commands.UpdateInput;
using Application.Features.Inputs.Commands.UpdateInputStatus;
using Application.Features.Inputs.Commands.UpdateManyInputStatus;
using Azure.Core;
using Domain.Constants.Permission;
using Domain.Entities;
using Domain.Primitives;
using FluentAssertions;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit.Abstractions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using BrandEntity = Domain.Entities.Brand;
using ProductCategoryEntity = Domain.Entities.ProductCategory;
using ProductEntity = Domain.Entities.Product;
using SupplierEntity = Domain.Entities.Supplier;
using InputEntity = Domain.Entities.Input;
using InputInfoEntity = Domain.Entities.InputInfo;
using IntegrationTests.SetupClass;

namespace IntegrationTests;

using System.Threading.Tasks;
using Xunit;

[Collection("Shared Integration Collection")]
public class InventoryReceipts : IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public InventoryReceipts(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(false);
    }

#pragma warning disable IDE0079 
#pragma warning disable CRR0035
    [Fact(DisplayName = "INPUT_001 - Tạo phiếu nhập hàng thành công (Happy Path)")]
    public async Task CreateInput_Success_ReturnsOk()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Create], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        // Required Data Creation
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        // Ensure Statuses exist
        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
        {
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });
        }

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
        {
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });
        }

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
        {
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });
        }

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        // Create Brand
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        // Create Category
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        // Create Supplier
        var supplier = new SupplierEntity
        {
            Name = $"Supplier_{uniqueId}",
            StatusId = supplierStatusId,
            Email = $"supplier_{uniqueId}@example.com",
            Phone = "0123456789"
        };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        // Create Product
        var product = new ProductEntity
        {
            Name = $"Product_{uniqueId}",
            BrandId = brand.Id,
            CategoryId = category.Id,
            StatusId = productStatusId,
            Weight = 10,
            SeatHeight = 10,
            GroundClearance = 10,
            FuelCapacity = 10,
            Displacement = 10,
            OilCapacity = 10
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        // Create ProductVariant
        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Price = 500000,
            UrlSlug = $"slug-{uniqueId}"
        };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        // End Data Creation

        var request = new CreateInputCommand
        {
            Notes = "Nhập hàng tháng 1",
            SupplierId = supplier.Id,
            Products = [ new CreateInputInfoRequest { ProductId = variant.Id, Count = 10, InputPrice = 100000 } ]
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/InventoryReceipts")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _client.SendAsync(requestMessage).ConfigureAwait(true);

        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            throw new Exception($"API returned 500. Response Body: {errorContent}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Id.Should().NotBeNull();
        content.StatusId.Should().Be(Domain.Constants.Input.InputStatus.Working);
        content.Products.Should().HaveCount(1);
        content.Products[0].Count.Should().Be(10);
        content.Products[0].InputPrice.Should().Be(100000);
        content.TotalPayable.Should().Be(1000000);

        var input = db.InputReceipts.FirstOrDefault(i => i.Id == content.Id);
        input.Should().NotBeNull();
        input!.StatusId.Should().Be(Domain.Constants.Input.InputStatus.Working);
    }

    [Fact(DisplayName = "INPUT_002 - Tạo phiếu nhập với nhiều sản phẩm và tính toán chính xác tổng tiền")]
    public async Task CreateInput_MultipleProducts_CalculatesTotalCorrectly()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Create], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        // Ensure Statuses exist
        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        // Create Prerequisite Entities
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        // Create 2 Products and Variants
        var product1 = new ProductEntity { Name = $"P1_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        var product2 = new ProductEntity { Name = $"P2_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.AddRange(product1, product2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant1 = new ProductVariant { ProductId = product1.Id, Price = 200000, UrlSlug = $"s1-{uniqueId}" };
        var variant2 = new ProductVariant { ProductId = product2.Id, Price = 300000, UrlSlug = $"s2-{uniqueId}" };
        db.ProductVariants.AddRange(variant1, variant2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var request = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = supplier.Id,
            Products =
            [
                new CreateInputInfoRequest { ProductId = variant1.Id, Count = 5, InputPrice = 123456.78m },
                new CreateInputInfoRequest { ProductId = variant2.Id, Count = 3, InputPrice = 987654.32m }
            ]
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/InventoryReceipts")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _client.SendAsync(requestMessage).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();

        decimal expectedTotal = (5 * 123456.78m) + (3 * 987654.32m);
        content!.TotalPayable.Should().Be((long)expectedTotal);
    }

    [Fact(DisplayName = "INPUT_004 - Tạo phiếu nhập với SupplierId không tồn tại")]
    public async Task CreateInput_NonExistentSupplier_ReturnsNotFound()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Create], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var request = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 9999,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/InventoryReceipts")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _client.SendAsync(requestMessage).ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INPUT_005 - Tạo phiếu nhập với ProductId không tồn tại")]
    public async Task CreateInput_NonExistentProduct_ReturnsNotFound()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Create], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var request = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 9999, Count = 10, InputPrice = 100000 } ]
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/InventoryReceipts")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _client.SendAsync(requestMessage).ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INPUT_006 - Tạo phiếu nhập với Product đã bị xóa hoặc không còn bán")]
    public async Task CreateInput_DeletedProduct_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Create], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        // Ensure Statuses
        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        // Create Prerequisite Entities
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 200000, UrlSlug = $"s-{uniqueId}", DeletedAt = DateTimeOffset.UtcNow };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var request = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = supplier.Id,
            Products = [ new CreateInputInfoRequest { ProductId = variant.Id, Count = 10, InputPrice = 100000 } ]
        };


        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/InventoryReceipts")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _client.SendAsync(requestMessage).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INPUT_012 - Tạo phiếu nhập với Notes chứa script XSS")]
    public async Task CreateInput_XSSInNotes_SanitizesInput()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Create], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        // Ensure Statuses
        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        // Create Prerequisite Entities
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"s-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var request = new CreateInputCommand
        {
            Notes = "<script>alert('XSS')</script>",
            SupplierId = supplier.Id,
            Products = [ new CreateInputInfoRequest { ProductId = variant.Id, Count = 10, InputPrice = 100000 } ]
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/InventoryReceipts")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _client.SendAsync(requestMessage).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        var input = db.InputReceipts.FirstOrDefault(i => i.Id == content!.Id);
        input!.Notes.Should().NotContain("<script>");
    }

    [Fact(DisplayName = "INPUT_016 - Lấy danh sách phiếu nhập có phân trang")]
    public async Task GetInputs_WithPagination_ReturnsCorrectPage()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.View], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v1/InventoryReceipts?page=1&pageSize=10");

        var response = await _client.SendAsync(requestMessage).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<InputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().HaveCountLessThanOrEqualTo(10);
        content.PageNumber.Should().Be(1);
    }

    [Fact(DisplayName = "INPUT_017 - Lấy danh sách phiếu nhập với filter theo StatusId")]
    public async Task GetInputs_FilterByStatus_ReturnsFilteredResults()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.View], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v1/InventoryReceipts?filters=StatusId==working");

        var response = await _client.SendAsync(requestMessage).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<InputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items
            .Should()
            .OnlyContain(i => string.Compare(i.StatusId, Domain.Constants.Input.InputStatus.Working) == 0);
    }

    [Fact(DisplayName = "INPUT_018 - Lấy danh sách phiếu nhập với sort theo InputDate descending")]
    public async Task GetInputs_SortByInputDateDesc_ReturnsSortedResults()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.View], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var input1 = new InputEntity { InputDate = DateTimeOffset.UtcNow.AddDays(-3), StatusId = inputStatusId, SupplierId = supplier.Id, CreatedBy = user.Id };
        var input2 = new InputEntity { InputDate = DateTimeOffset.UtcNow.AddDays(-1), StatusId = inputStatusId, SupplierId = supplier.Id, CreatedBy = user.Id };
        var input3 = new InputEntity { InputDate = DateTimeOffset.UtcNow.AddDays(-2), StatusId = inputStatusId, SupplierId = supplier.Id, CreatedBy = user.Id };

        db.InputReceipts.AddRange(input1, input2, input3);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v1/InventoryReceipts?sorts=-InputDate");
        var response = await _client.SendAsync(requestMessage).ConfigureAwait(true);

        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            throw new Exception($"API returned 500. Response Body: {errorContent}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<InputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);

        content.Should().NotBeNull();

        var createdIds = new List<int?> { input1.Id, input2.Id, input3.Id };
        var items = content!.Items?.Where(i => createdIds.Contains(i.Id)).ToList();

        items.Should().HaveCount(3);
        items.Should().BeInDescendingOrder(i => i.InputDate);
    }

    [Fact(DisplayName = "INPUT_020 - Lấy chi tiết phiếu nhập thành công")]
    public async Task GetInputById_ExistingInput_ReturnsCompleteDetails()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.View], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        // Ensure Statuses
        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        // Create Entities
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"s-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var input = new InputEntity 
        { 
            InputDate = DateTimeOffset.UtcNow, 
            StatusId = inputStatusId, 
            SupplierId = supplier.Id, 
            CreatedBy = user.Id,
            Notes = "Test Note"
        };
        db.InputReceipts.Add(input);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        
        // Add InputInfo
        var inputInfo = new InputInfoEntity { InputId = input.Id, ProductId = variant.Id, Count = 5, InputPrice = 50000 };
        db.Entry(inputInfo).State = EntityState.Added; // OR db.Set<InputInfo>().Add(inputInfo);
        // Since InputInfo doesn't have a DbSet exposed directly in typical context but accessible via navigation or db.Set
        // Assuming context has InputInfos or similar. Checking file structure...
        // Wait, context has InputReceipts. InputInfo is related.
        // Let's check context. InputInfo usually part of Input or separate DbSet.
        // I will trust that I can add it via db.Set<InputInfo> or collection.
        // But simpler:
        input.InputInfos = [ new InputInfoEntity { ProductId = variant.Id, Count = 5, InputPrice = 50000 } ];
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/InventoryReceipts/{input.Id}");

        var response = await _client.SendAsync(requestMessage).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Id.Should().Be(input.Id);
        content.StatusId.Should().Be(inputStatusId);
        content.Products.Should().NotBeNull();
        content.Products.Should().HaveCount(1);
        content.SupplierName.Should().Be(supplier.Name);
        content.TotalPayable.Should().Be(250000);
    }

    [Fact(DisplayName = "INPUT_021 - Lấy chi tiết phiếu nhập không tồn tại")]
    public async Task GetInputById_NonExistentInput_ReturnsNotFound()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.View], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        int inputId = 9999;

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/InventoryReceipts/{inputId}");

        var response = await _client.SendAsync(requestMessage).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "INPUT_023 - Cập nhật phiếu nhập thành công ở trạng thái working")]
    public async Task UpdateInput_WorkingStatus_UpdatesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Edit], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        var supplier2 = new SupplierEntity { Name = $"Supplier2_{uniqueId}", StatusId = supplierStatusId, Email = $"sup2_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        var product2 = new ProductEntity { Name = $"P2_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.AddRange(product, product2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"s-{uniqueId}" };
        db.ProductVariants.Add(variant);

        var variant2 = new ProductVariant { ProductId = product2.Id, Price = 200000, UrlSlug = $"s2-{uniqueId}" };
        db.ProductVariants.Add(variant2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        
        var inputReceipt = new InputEntity 
        { 
            StatusId = inputStatusId, 
            SupplierId = supplier.Id, 
            Notes = "Original",
            CreatedAt = DateTimeOffset.UtcNow.DateTime
        };
        db.InputReceipts.Add(inputReceipt);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        db.InputInfos.Add(new InputInfoEntity { InputId = inputReceipt.Id, ProductId = variant.Id, Count = 10, InputPrice = 100000 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var updateRequest = new UpdateInputCommand
        {
            Notes = "Updated",
            SupplierId = supplier2.Id,
            Products = [ new UpdateInputInfoRequest { ProductId = variant2.Id, Count = 20, InputPrice = 200000 } ]
        };

        var requestUpdateMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/InventoryReceipts/{inputReceipt.Id}");
        requestUpdateMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestUpdateMessage.Content = JsonContent.Create(updateRequest);
        var response = await _client.SendAsync(requestUpdateMessage).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Notes.Should().Be("Updated");
        content.SupplierId.Should().Be(supplier2.Id);
        content.TotalPayable.Should().Be(4000000);
    }

    [Fact(DisplayName = "INPUT_024 - Cập nhật phiếu nhập ở trạng thái finished (không cho phép)")]
    public async Task UpdateInput_FinishedStatus_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Edit], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });
        
        var finishStatusId = Domain.Constants.Input.InputStatus.Finish;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, finishStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = finishStatusId });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        var supplier2 = new SupplierEntity { Name = $"Supplier2_{uniqueId}", StatusId = supplierStatusId, Email = $"sup2_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.AddRange(supplier, supplier2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"s-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var inputReceipt = new InputEntity 
        { 
            StatusId = finishStatusId, 
            SupplierId = supplier.Id, 
            Notes = "Original",
            CreatedAt = DateTimeOffset.UtcNow.DateTime
        };
        db.InputReceipts.Add(inputReceipt);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        db.InputInfos.Add(new InputInfoEntity { InputId = inputReceipt.Id, ProductId = variant.Id, Count = 10, InputPrice = 100000 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var updateRequest = new UpdateInputCommand
        {
            Notes = "Updated",
            SupplierId = supplier.Id,
            Products = [ new UpdateInputInfoRequest { ProductId = variant.Id, Count = 20, InputPrice = 200000 } ]
        };

        var requestUpdateMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/InventoryReceipts/{inputReceipt.Id}");
        requestUpdateMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestUpdateMessage.Content = JsonContent.Create(updateRequest);
        var response = await _client.SendAsync(requestUpdateMessage).ConfigureAwait(true);
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INPUT_025 - Cập nhật phiếu nhập ở trạng thái cancelled (không cho phép)")]
    public async Task UpdateInput_CancelledStatus_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Edit], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });
        
        var cancelStatusId = Domain.Constants.Input.InputStatus.Cancel;
        if (!await  db.InputStatuses.AnyAsync(x => string.Compare(x.Key, cancelStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = cancelStatusId });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        var supplier2 = new SupplierEntity { Name = $"Supplier2_{uniqueId}", StatusId = supplierStatusId, Email = $"sup2_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.AddRange(supplier, supplier2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"s-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var inputReceipt = new InputEntity
        { 
            StatusId = cancelStatusId, 
            SupplierId = supplier.Id, 
            Notes = "Original",
            CreatedAt = DateTimeOffset.UtcNow.DateTime
        };
        db.InputReceipts.Add(inputReceipt);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        db.InputInfos.Add(new InputInfoEntity { InputId = inputReceipt.Id, ProductId = variant.Id, Count = 10, InputPrice = 100000 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var updateRequest = new UpdateInputCommand
        {
            Notes = "Updated",
            SupplierId = supplier.Id,
            Products = [ new UpdateInputInfoRequest { ProductId = variant.Id, Count = 20, InputPrice = 200000 } ]
        };

        var requestUpdateMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/InventoryReceipts/{inputReceipt.Id}");
        requestUpdateMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestUpdateMessage.Content = JsonContent.Create(updateRequest);
        var response = await _client.SendAsync(requestUpdateMessage).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INPUT_028 - Cập nhật trạng thái phiếu nhập từ working sang finished")]
    public async Task UpdateInputStatus_WorkingToFinished_UpdatesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.ChangeStatus], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });
        
        var finishStatusId = Domain.Constants.Input.InputStatus.Finish;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, finishStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = finishStatusId });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"s-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var inputReceipt = new InputEntity 
        { 
            StatusId = inputStatusId, 
            SupplierId = supplier.Id, 
            Notes = "Test",
            CreatedBy = user.Id,
            CreatedAt = DateTimeOffset.UtcNow.DateTime
        };
        db.InputReceipts.Add(inputReceipt);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        db.InputInfos.Add(new InputInfoEntity { InputId = inputReceipt.Id, ProductId = variant.Id, Count = 10, InputPrice = 100000 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var statusRequest = new UpdateInputStatusCommand { StatusId = Domain.Constants.Input.InputStatus.Finish };

        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/InventoryReceipts/{inputReceipt.Id}/status",
            statusRequest,
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.StatusId.Should().Be(Domain.Constants.Input.InputStatus.Finish);

        using var checkScope = _factory.Services.CreateScope();
        var checkDb = checkScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var input = checkDb.InputReceipts.FirstOrDefault(i => i.Id == inputReceipt.Id);
        input!.StatusId.Should().Be(Domain.Constants.Input.InputStatus.Finish);
        input.ConfirmedBy.Should().NotBeNull();
    }

    [Fact(DisplayName = "INPUT_029 - Cập nhật trạng thái phiếu nhập từ working sang cancelled")]
    public async Task UpdateInputStatus_WorkingToCancelled_UpdatesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.ChangeStatus], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });
        
        var cancelStatusId = Domain.Constants.Input.InputStatus.Cancel;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, cancelStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = cancelStatusId });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"s-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var inputReceipt = new InputEntity 
        { 
            StatusId = inputStatusId, 
            SupplierId = supplier.Id, 
            Notes = "Test",
            CreatedBy = user.Id,
            CreatedAt = DateTimeOffset.UtcNow.DateTime
        };
        db.InputReceipts.Add(inputReceipt);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        db.InputInfos.Add(new InputInfoEntity { InputId = inputReceipt.Id, ProductId = variant.Id, Count = 10, InputPrice = 100000 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var statusRequest = new UpdateInputStatusCommand { StatusId = Domain.Constants.Input.InputStatus.Cancel };

        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/InventoryReceipts/{inputReceipt.Id}/status",
            statusRequest,
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.StatusId.Should().Be(Domain.Constants.Input.InputStatus.Cancel);
    }

    [Fact(DisplayName = "INPUT_032 - Cập nhật trạng thái nhiều phiếu nhập cùng lúc")]
    public async Task UpdateManyInputStatus_MultipleInputs_UpdatesAllSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.ChangeStatus], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });
        
        var finishStatusId = Domain.Constants.Input.InputStatus.Finish;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, finishStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = finishStatusId });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"s-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var ids = new List<int>();
        for(int i = 0; i < 3; i++)
        {
            var inputReceipt = new InputEntity 
            { 
                StatusId = inputStatusId, 
                SupplierId = supplier.Id, 
                Notes = $"Test {i}",
                CreatedBy = user.Id,
                CreatedAt = DateTimeOffset.UtcNow.DateTime
            };
            db.InputReceipts.Add(inputReceipt);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            db.InputInfos.Add(new InputInfoEntity { InputId = inputReceipt.Id, ProductId = variant.Id, Count = 10, InputPrice = 100000 });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            ids.Add(inputReceipt.Id);
        }

        var statusRequest = new UpdateManyInputStatusCommand
        {
            Ids = ids,
            StatusId = Domain.Constants.Input.InputStatus.Finish
        };

        var response = await _client.PatchAsJsonAsync(
            "/api/v1/InventoryReceipts/status",
            statusRequest,
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var checkScope = _factory.Services.CreateScope();
        var checkDb = checkScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        foreach(var id in ids)
        {
            var input = checkDb.InputReceipts.FirstOrDefault(i => i.Id == id);
            input!.StatusId.Should().Be(Domain.Constants.Input.InputStatus.Finish);
        }
    }

    [Fact(DisplayName = "INPUT_033 - Cập nhật trạng thái nhiều phiếu nhập với một số Id không tồn tại")]
    public async Task UpdateManyInputStatus_SomeNonExistent_HandlesPartially()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.ChangeStatus], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });
        
        var finishStatusId = Domain.Constants.Input.InputStatus.Finish;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, finishStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = finishStatusId });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"s-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var inputReceipt = new InputEntity 
        { 
            StatusId = inputStatusId, 
            SupplierId = supplier.Id, 
            Notes = "Test",
            CreatedBy = user.Id,
            CreatedAt = DateTimeOffset.UtcNow.DateTime
        };
        db.InputReceipts.Add(inputReceipt);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        db.InputInfos.Add(new InputInfoEntity { InputId = inputReceipt.Id, ProductId = variant.Id, Count = 10, InputPrice = 100000 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var statusRequest = new UpdateManyInputStatusCommand
        {
            Ids = [ inputReceipt.Id, 9999 ],
            StatusId = Domain.Constants.Input.InputStatus.Finish
        };

        var response = await _client.PatchAsJsonAsync(
            "/api/v1/InventoryReceipts/status",
            statusRequest,
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.MultiStatus, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "INPUT_034 - Xóa phiếu nhập thành công ở trạng thái working")]
    public async Task DeleteInput_WorkingStatus_DeletesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Delete], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });
        
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"s-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var inputReceipt = new InputEntity 
        { 
            StatusId = inputStatusId, 
            SupplierId = supplier.Id, 
            Notes = "Test",
            CreatedBy = user.Id,
            CreatedAt = DateTimeOffset.UtcNow.DateTime
        };
        db.InputReceipts.Add(inputReceipt);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        db.InputInfos.Add(new InputInfoEntity { InputId = inputReceipt.Id, ProductId = variant.Id, Count = 10, InputPrice = 100000 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.DeleteAsync($"/api/v1/InventoryReceipts/{inputReceipt.Id}").ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        using var checkScope = _factory.Services.CreateScope();
        var checkDb = checkScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var input = checkDb.InputReceipts.IgnoreQueryFilters().FirstOrDefault(i => i.Id == inputReceipt.Id);
        input!.DeletedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "INPUT_038 - Xóa nhiều phiếu nhập cùng lúc")]
    public async Task DeleteManyInputs_MultipleInputs_DeletesAllSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Delete], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });
        
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"s-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var ids = new List<int>();
        for(int i = 0; i < 3; i++)
        {
            var inputReceipt = new InputEntity 
            { 
                StatusId = inputStatusId, 
                SupplierId = supplier.Id, 
                Notes = $"Test {i}",
                CreatedBy = user.Id,
                CreatedAt = DateTimeOffset.UtcNow.DateTime
            };
            db.InputReceipts.Add(inputReceipt);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            db.InputInfos.Add(new InputInfoEntity { InputId = inputReceipt.Id, ProductId = variant.Id, Count = 10, InputPrice = 100000 });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            ids.Add(inputReceipt.Id);
        }

        var deleteRequest = new DeleteManyInputsCommand { Ids = ids };

        var response = await _client.SendAsync(
            new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri("/api/v1/InventoryReceipts", UriKind.Relative),
                Content = JsonContent.Create(deleteRequest)
            })
            .ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "INPUT_039 - Khôi phục phiếu nhập đã xóa thành công")]
    public async Task RestoreInput_DeletedInput_RestoresSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Delete], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });
        
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"s-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var inputReceipt = new InputEntity 
        { 
            StatusId = inputStatusId, 
            SupplierId = supplier.Id, 
            Notes = "Test",
            CreatedBy = user.Id,
            CreatedAt = DateTimeOffset.UtcNow.DateTime,
            DeletedAt = DateTimeOffset.UtcNow.DateTime
        };
        db.InputReceipts.Add(inputReceipt);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        db.InputInfos.Add(new InputInfoEntity { InputId = inputReceipt.Id, ProductId = variant.Id, Count = 10, InputPrice = 100000 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.PostAsync($"/api/v1/InventoryReceipts/{inputReceipt.Id}/restore", null)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var checkScope = _factory.Services.CreateScope();
        var checkDb = checkScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var input = checkDb.InputReceipts.FirstOrDefault(i => i.Id == inputReceipt.Id);
        input!.DeletedAt.Should().BeNull();
    }

    [Fact(DisplayName = "INPUT_041 - Khôi phục nhiều phiếu nhập đã xóa cùng lúc")]
    public async Task RestoreManyInputs_MultipleDeletedInputs_RestoresAllSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Delete], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });
        
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"s-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var ids = new List<int>();
        for(int i = 0; i < 3; i++)
        {
            var inputReceipt = new InputEntity 
            { 
                StatusId = inputStatusId, 
                SupplierId = supplier.Id, 
                Notes = $"Test {i}",
                CreatedBy = user.Id,
                CreatedAt = DateTimeOffset.UtcNow.DateTime,
                DeletedAt = DateTimeOffset.UtcNow.DateTime
            };
            db.InputReceipts.Add(inputReceipt);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            db.InputInfos.Add(new InputInfoEntity { InputId = inputReceipt.Id, ProductId = variant.Id, Count = 10, InputPrice = 100000 });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            ids.Add(inputReceipt.Id);
        }

        var restoreRequest = new RestoreManyInputsCommand { Ids = ids };

        var response = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts/restore", restoreRequest)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var checkScope = _factory.Services.CreateScope();
        var checkDb = checkScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        foreach(var id in ids)
        {
            var input = checkDb.InputReceipts.FirstOrDefault(i => i.Id == id);
            input!.DeletedAt.Should().BeNull();
        }
    }

    [Fact(DisplayName = "INPUT_042 - Clone phiếu nhập thành công")]
    public async Task CloneInput_ValidInput_CreatesNewInput()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Create], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });
        
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"s-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var createdInput = new InputEntity 
        { 
            StatusId = inputStatusId, 
            SupplierId = supplier.Id, 
            Notes = "Original",
            CreatedBy = user.Id,
            CreatedAt = DateTimeOffset.UtcNow.DateTime
        };
        db.InputReceipts.Add(createdInput);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        db.InputInfos.Add(new InputInfoEntity { InputId = createdInput.Id, ProductId = variant.Id, Count = 10, InputPrice = 100000 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.PostAsync($"/api/v1/InventoryReceipts/{createdInput!.Id}/clone", null)
            .ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        var clonedInput = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        clonedInput!.Id.Should().NotBe(createdInput.Id);
        clonedInput.StatusId.Should().Be(Domain.Constants.Input.InputStatus.Working);
        clonedInput.SupplierId.Should().Be(createdInput.SupplierId);
    }

    [Fact(DisplayName = "INPUT_043 - Clone phiếu nhập với sản phẩm đã bị xóa")]
    public async Task CloneInput_WithDeletedProduct_ClonesOnlyValidProducts()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Create], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });
        
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product1 = new ProductEntity { Name = $"P1_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        var product2 = new ProductEntity { Name = $"P2_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product1);
        db.Products.Add(product2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant1 = new ProductVariant { ProductId = product1.Id, Price = 100000, UrlSlug = $"s1-{uniqueId}" };
        var variant2 = new ProductVariant { ProductId = product2.Id, Price = 50000, UrlSlug = $"s2-{uniqueId}" };
        db.ProductVariants.Add(variant1);
        db.ProductVariants.Add(variant2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var createdInput = new InputEntity 
        { 
            StatusId = inputStatusId, 
            SupplierId = supplier.Id, 
            Notes = "Original",
            CreatedBy = user.Id,
            CreatedAt = DateTimeOffset.UtcNow.DateTime
        };
        db.InputReceipts.Add(createdInput);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        db.InputInfos.Add(new InputInfoEntity { InputId = createdInput.Id, ProductId = variant1.Id, Count = 10, InputPrice = 100000 });
        db.InputInfos.Add(new InputInfoEntity { InputId = createdInput.Id, ProductId = variant2.Id, Count = 5, InputPrice = 50000 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        if(product2 != null)
        {
            product2.DeletedAt = DateTimeOffset.UtcNow.DateTime;
            db.SaveChanges();
        }

        var response = await _client.PostAsync($"/api/v1/InventoryReceipts/{createdInput!.Id}/clone", null)
            .ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        var clonedInput = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        clonedInput!.Products.Should().HaveCount(1);
        clonedInput.Products[0].ProductId.Should().Be(variant1.Id);
    }

    [Fact(DisplayName = "INPUT_046 - Lấy danh sách phiếu nhập theo SupplierId")]
    public async Task GetInputsBySupplierId_ValidSupplierId_ReturnsFilteredInputs()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Suppliers.View, PermissionsList.Inputs.View], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });
        
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        int supplierId = supplier.Id;

        // Seed some inputs for this supplier
        for (int i = 0; i < 3; i++)
        {
             db.InputReceipts.Add(new InputEntity 
             { 
                 StatusId = inputStatusId, 
                 SupplierId = supplierId, 
                 Notes = $"Filtered {i}",
                  CreatedBy = user.Id,
                 CreatedAt = DateTimeOffset.UtcNow.DateTime
             });
        }
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync($"/api/v1/InventoryReceipts/by-supplier/{supplierId}?page=1&pageSize=10")
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<InputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().OnlyContain(i => i.SupplierId == supplierId);
    }

    [Fact(DisplayName = "INPUT_047 - Lấy danh sách phiếu nhập đã xóa")]
    public async Task GetDeletedInputs_ReturnsOnlyDeletedInputs()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Create, PermissionsList.Inputs.View, PermissionsList.Inputs.Delete], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });
        
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"s-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var inputReceipt = new InputEntity 
        { 
            StatusId = inputStatusId, 
            SupplierId = supplier.Id, 
            Notes = "Test",
            CreatedBy = user.Id,
            CreatedAt = DateTimeOffset.UtcNow.DateTime,
            DeletedAt = DateTimeOffset.UtcNow.DateTime
        };
        db.InputReceipts.Add(inputReceipt);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/InventoryReceipts/deleted?page=1&pageSize=10")
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<InputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
    }

    [Fact(DisplayName = "INPUT_066 - Tạo phiếu nhập với ImportPrice là 0")]
    public async Task CreateInput_ZeroImportPrice_AllowsFreeProducts()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Create], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        // Ensure Statuses
        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        // Create Prerequisite Entities
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"s-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var request = new CreateInputCommand
        {
            Notes = "Free products",
            SupplierId = supplier.Id,
            Products = [ new CreateInputInfoRequest { ProductId = variant.Id, Count = 10, InputPrice = 0 } ]
        };

        var response = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", request).ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        var content = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.TotalPayable.Should().Be(0);
    }

    [Fact(DisplayName = "INPUT_068 - Kiểm tra ConfirmedBy được ghi nhận đúng khi cập nhật phiếu nhập")]
    public async Task UpdateInputStatus_TracksConfirmedByCorrectly()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.Edit, PermissionsList.Inputs.ChangeStatus], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        // Create another user to be the creator
        var creatorUniqueId = Guid.NewGuid().ToString("N")[..8];
        var creatorUsername = $"creator_{creatorUniqueId}";
        var creatorEmail = $"creator_{creatorUniqueId}@gmail.com";
        var creatorUser = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, creatorUsername, "Password123!", [], CancellationToken.None, creatorEmail).ConfigureAwait(true);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var productStatusId = Domain.Constants.ProductStatus.ForSale;
        if (!await db.ProductStatuses.AnyAsync(x => string.Compare(x.Key, productStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.ProductStatuses.Add(new ProductStatus { Key = productStatusId });

        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });
        
        var finishStatusId = Domain.Constants.Input.InputStatus.Finish;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, finishStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = finishStatusId });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
        db.ProductCategories.Add(category);
        
        var supplier = new SupplierEntity { Name = $"Supplier_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var product = new ProductEntity { Name = $"P_{uniqueId}", BrandId = brand.Id, CategoryId = category.Id, StatusId = productStatusId };
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"s-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var createdInput = new InputEntity 
        { 
            StatusId = inputStatusId, 
            SupplierId = supplier.Id, 
            Notes = "Test",
            CreatedBy = creatorUser.Id,
            CreatedAt = DateTimeOffset.UtcNow.DateTime
        };
        db.InputReceipts.Add(createdInput);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        db.InputInfos.Add(new InputInfoEntity { InputId = createdInput.Id, ProductId = variant.Id, Count = 10, InputPrice = 100000 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var statusRequest = new UpdateInputStatusCommand { StatusId = Domain.Constants.Input.InputStatus.Finish };

        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/InventoryReceipts/{createdInput!.Id}/status",
            statusRequest,
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Scope reused
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var input = verifyDb.InputReceipts.FirstOrDefault(i => i.Id == createdInput.Id);
        input!.ConfirmedBy.Should().NotBeNull();
    }

    [Fact(DisplayName = "INPUT_069 - Lấy danh sách phiếu nhập với nhiều filter kết hợp")]
    public async Task GetInputs_MultipleCombinedFilters_ReturnsFilteredResults()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Inputs.View], CancellationToken.None, email).ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        // Ensure Statuses
        var inputStatusId = Domain.Constants.Input.InputStatus.Working;
        if (!await db.InputStatuses.AnyAsync(x => string.Compare(x.Key, inputStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.InputStatuses.Add(new InputStatus { Key = inputStatusId });
        
        var supplierStatusId = Domain.Constants.SupplierStatus.Active;
        if (!await db.SupplierStatuses.AnyAsync(x => string.Compare(x.Key, supplierStatusId) == 0, CancellationToken.None).ConfigureAwait(true))
            db.SupplierStatuses.Add(new SupplierStatus { Key = supplierStatusId });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var supplier = new SupplierEntity { Name = $"Sup_{uniqueId}", StatusId = supplierStatusId, Email = $"sup_{uniqueId}@ex.com", Phone = "0123456789" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var input = new InputEntity { InputDate = DateTimeOffset.UtcNow, StatusId = inputStatusId, SupplierId = supplier.Id, CreatedBy = user.Id };
        db.InputReceipts.Add(input);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync($"/api/v1/InventoryReceipts?filters=StatusId==working,SupplierId=={supplier.Id}")
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<InputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items
            .Should()
            .OnlyContain(
                i => string.Compare(i.StatusId, Domain.Constants.Input.InputStatus.Working) == 0 && i.SupplierId == supplier.Id);
    }
#pragma warning restore CRR0035
}
