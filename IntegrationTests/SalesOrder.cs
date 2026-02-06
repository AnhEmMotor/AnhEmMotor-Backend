using Application.ApiContracts.Output.Responses;
using Application.Features.Outputs.Commands.DeleteManyOutputs;
using Application.Features.Outputs.Commands.RestoreManyOutputs;
using Application.Features.Outputs.Commands.UpdateManyOutputStatus;
using Application.Features.Outputs.Commands.UpdateOutputForManager;
using Application.Features.Outputs.Commands.UpdateOutputStatus;
using Domain.Constants.Order;
using Domain.Constants.Permission;
using Domain.Entities;
using Domain.Primitives;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BrandEntity = Domain.Entities.Brand;
using InputEntity = Domain.Entities.Input;
using InputStatusEntity = Domain.Entities.InputStatus;
using OutputEntity = Domain.Entities.Output;
using OutputStatusEntity = Domain.Entities.OutputStatus;
using ProductCategoryEntity = Domain.Entities.ProductCategory;
using ProductEntity = Domain.Entities.Product;

namespace IntegrationTests;

using System.Threading.Tasks;
using Xunit;

[Collection("Shared Integration Collection")]
public class SalesOrder : IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public SalesOrder(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() { await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(true); }

    private async Task<int> SeedProductVariantAsync(string uniqueId, CancellationToken cancellationToken = default)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        if(!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, "for-sale") == 0, cancellationToken)
            .ConfigureAwait(false))
        {
            db.ProductStatuses.Add(new ProductStatus { Key = "for-sale" });
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Cat_{uniqueId}" };
        db.ProductCategories.Add(category);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var product = new ProductEntity
        {
            Name = $"Prod_{uniqueId}",
            BrandId = brand.Id,
            CategoryId = category.Id,
            StatusId = "for-sale"
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var variant = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"slug-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return variant.Id;
    }

    private async Task SeedInventoryAsync(
        int variantId,
        int quantity,
        string uniqueId,
        CancellationToken cancellationToken = default)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        if(!await db.InputStatuses
            .AnyAsync(x => string.Compare(x.Key, "finished") == 0, cancellationToken)
            .ConfigureAwait(false))
        {
            db.InputStatuses.Add(new InputStatusEntity { Key = "finished" });
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var input = new InputEntity
        {
            InputDate = DateTimeOffset.UtcNow,
            Notes = $"Test inventory for {uniqueId}",
            StatusId = "finished"
        };
        db.InputReceipts.Add(input);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var inputInfo = new InputInfo
        {
            InputId = input.Id,
            ProductId = variantId,
            Count = quantity,
            RemainingCount = quantity,
            InputPrice = 50000
        };
        db.InputInfos.Add(inputInfo);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

#pragma warning disable IDE0079
#pragma warning disable CRR0035
    [Fact(DisplayName = "SO_061 - Tạo đơn hàng với BuyerId tự động từ token JWT")]
    public async Task CreateOutput_WithAuthenticatedUser_SetsBuyerIdFromToken()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
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

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if(!await db.OutputStatuses
                .AnyAsync(x => string.Compare(x.Key, OrderStatus.Pending) == 0, CancellationToken.None)
                .ConfigureAwait(true))
            {
                db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Pending });
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            }
        }

        var variantId = await SeedProductVariantAsync(uniqueId, CancellationToken.None).ConfigureAwait(true);

        var jsonContent = System.Text.Json.JsonSerializer
            .Serialize(
                new { buyerId = user.Id, notes = "Test", products = new[] { new { productId = variantId, count = 1 } } });
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/SalesOrders", httpContent).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.BuyerId.Should().NotBeNull();
    }

    [Fact(DisplayName = "SO_062 - Tạo đơn hàng COD và kiểm tra trạng thái ban đầu")]
    public async Task CreateOutput_CODOrder_InitialStatusIsPending()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
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

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if(!await db.OutputStatuses
                .AnyAsync(x => string.Compare(x.Key, OrderStatus.Pending) == 0, CancellationToken.None)
                .ConfigureAwait(true))
            {
                db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Pending });
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            }
        }

        var variantId = await SeedProductVariantAsync(uniqueId, CancellationToken.None).ConfigureAwait(true);

        var jsonContent = System.Text.Json.JsonSerializer
            .Serialize(
                new
                {
                    buyerId = user.Id,
                    notes = "COD Order",
                    products = new[] { new { productId = variantId, count = 1 } }
                });
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/SalesOrders", httpContent).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        order.Should().NotBeNull();
        order!.StatusId.Should().Be(OrderStatus.Pending);
    }

    [Fact(DisplayName = "SO_063 - Luồng COD đầy đủ: Pending -> ConfirmedCod -> Delivering -> Completed")]
    public async Task UpdateOutputStatus_CODFlow_CompletesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Outputs.Create, PermissionsList.Outputs.ChangeStatus ],
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

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var statuses = new[]
            {
                OrderStatus.Pending,
                OrderStatus.ConfirmedCod,
                OrderStatus.Delivering,
                OrderStatus.Completed
            };
            foreach(var s in statuses)
            {
                if(!await db.OutputStatuses
                    .AnyAsync(x => string.Compare(x.Key, s) == 0, CancellationToken.None)
                    .ConfigureAwait(true))
                    db.OutputStatuses.Add(new OutputStatusEntity { Key = s });
            }
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var variantId = await SeedProductVariantAsync(uniqueId, CancellationToken.None).ConfigureAwait(true);

        await SeedInventoryAsync(variantId, 10, uniqueId, CancellationToken.None).ConfigureAwait(true);

        var jsonContent = System.Text.Json.JsonSerializer
            .Serialize(new { buyerId = user.Id, products = new[] { new { productId = variantId, count = 1 } } });
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/v1/SalesOrders", httpContent).ConfigureAwait(true);
        var order = await createResponse.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        int orderId = order!.Id!.Value;

        var updateRequest1 = new UpdateOutputStatusCommand { StatusId = OrderStatus.ConfirmedCod };
        var response1 = await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            updateRequest1,
            CancellationToken.None)
            .ConfigureAwait(true);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateRequest2 = new UpdateOutputStatusCommand { StatusId = OrderStatus.Delivering };
        var response2 = await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            updateRequest2,
            CancellationToken.None)
            .ConfigureAwait(true);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateRequest3 = new UpdateOutputStatusCommand { StatusId = OrderStatus.Completed };
        var response3 = await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            updateRequest3,
            CancellationToken.None)
            .ConfigureAwait(true);
        response3.StatusCode.Should().Be(HttpStatusCode.OK);

        var finalOrder = await response3.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        finalOrder!.StatusId.Should().Be(OrderStatus.Completed);
    }

    [Fact(DisplayName = "SO_064 - Luồng Deposit đầy đủ: Pending -> Deposit50 -> Confirmed50 -> Delivering -> Completed")]
    public async Task UpdateOutputStatus_DepositFlow_CompletesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Outputs.Create, PermissionsList.Outputs.ChangeStatus ],
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

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var statuses = new[]
            {
                OrderStatus.Pending,
                OrderStatus.WaitingDeposit,
                OrderStatus.DepositPaid,
                OrderStatus.Delivering,
                OrderStatus.Completed
            };
            foreach(var s in statuses)
            {
                if(!await db.OutputStatuses
                    .AnyAsync(x => string.Compare(x.Key, s) == 0, CancellationToken.None)
                    .ConfigureAwait(true))
                    db.OutputStatuses.Add(new OutputStatusEntity { Key = s });
            }
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var variantId = await SeedProductVariantAsync(uniqueId, CancellationToken.None).ConfigureAwait(true);

        await SeedInventoryAsync(variantId, 10, uniqueId, CancellationToken.None).ConfigureAwait(true);

        var jsonContent = System.Text.Json.JsonSerializer
            .Serialize(new { buyerId = user.Id, products = new[] { new { productId = variantId, count = 1 } } });
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/v1/SalesOrders", httpContent).ConfigureAwait(true);
        var order = await createResponse.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        int orderId = order!.Id!.Value;

        await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            new UpdateOutputStatusCommand { StatusId = OrderStatus.WaitingDeposit },
            CancellationToken.None)
            .ConfigureAwait(true);

        await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            new UpdateOutputStatusCommand { StatusId = OrderStatus.DepositPaid },
            CancellationToken.None)
            .ConfigureAwait(true);

        await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            new UpdateOutputStatusCommand { StatusId = OrderStatus.Delivering },
            CancellationToken.None)
            .ConfigureAwait(true);

        var finalResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            new UpdateOutputStatusCommand { StatusId = OrderStatus.Completed },
            CancellationToken.None)
            .ConfigureAwait(true);

        finalResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "SO_065 - Luồng Refund: Pending -> ConfirmedCod -> Refund")]
    public async Task UpdateOutputStatus_RefundFlow_CompletesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Outputs.Create, PermissionsList.Outputs.ChangeStatus ],
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

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var statuses = new[]
            {
                OrderStatus.Pending,
                OrderStatus.PaidProcessing,
                OrderStatus.Refunding,
                OrderStatus.Refunded
            };
            foreach(var s in statuses)
            {
                if(!await db.OutputStatuses
                    .AnyAsync(x => string.Compare(x.Key, s) == 0, CancellationToken.None)
                    .ConfigureAwait(true))
                    db.OutputStatuses.Add(new OutputStatusEntity { Key = s });
            }
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var variantId = await SeedProductVariantAsync(uniqueId, CancellationToken.None).ConfigureAwait(true);

        await SeedInventoryAsync(variantId, 10, uniqueId, CancellationToken.None).ConfigureAwait(true);

        var jsonContent = System.Text.Json.JsonSerializer
            .Serialize(new { buyerId = user.Id, products = new[] { new { productId = variantId, count = 1 } } });
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/v1/SalesOrders", httpContent).ConfigureAwait(true);
        var order = await createResponse.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        int orderId = order!.Id!.Value;

        await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            new UpdateOutputStatusCommand { StatusId = OrderStatus.PaidProcessing },
            CancellationToken.None)
            .ConfigureAwait(true);

        var refundResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            new UpdateOutputStatusCommand { StatusId = OrderStatus.Refunding },
            CancellationToken.None)
            .ConfigureAwait(true);

        refundResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "SO_066 - CreateOutputForAdmin được gọi bởi Manager")]
    public async Task CreateOutputForAdmin_ByManager_CreatesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
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

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if(!await db.OutputStatuses
                .AnyAsync(x => string.Compare(x.Key, OrderStatus.Pending) == 0, CancellationToken.None)
                .ConfigureAwait(true))
            {
                db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Pending });
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            }
        }

        var variantId = await SeedProductVariantAsync(uniqueId, CancellationToken.None).ConfigureAwait(true);

        var jsonContent = System.Text.Json.JsonSerializer
            .Serialize(new { buyerId = user.Id, products = new[] { new { productId = variantId, count = 1 } } });
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/SalesOrders/by-manager", httpContent).ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact(DisplayName = "SO_067 - Tìm kiếm với Filters")]
    public async Task GetOutputs_WithFilters_ReturnsFilteredResults()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Outputs.View ],
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

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if(!await db.OutputStatuses
                .AnyAsync(x => string.Compare(x.Key, OrderStatus.Pending) == 0, CancellationToken.None)
                .ConfigureAwait(true))
            {
                db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Pending });
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            }
            db.OutputOrders
                .Add(
                    new OutputEntity
                    {
                        StatusId = OrderStatus.Pending,
                        Notes = $"SearchMe_{uniqueId}",
                        CreatedAt = DateTime.UtcNow
                    });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var response = await _client.GetAsync(
            $"/api/v1/SalesOrders?filters=status=={OrderStatus.Pending},Notes@={uniqueId}")
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<OutputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().Contain(x => x.Notes != null && x.Notes.Contains(uniqueId));
    }

    [Fact(DisplayName = "SO_068 - Tạo đơn hàng với nhiều sản phẩm")]
    public async Task CreateOutput_WithMultipleProducts_CreatesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
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

        int variantId1, variantId2;
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if(!await db.OutputStatuses
                .AnyAsync(x => string.Compare(x.Key, OrderStatus.Pending) == 0, CancellationToken.None)
                .ConfigureAwait(true))
            {
                db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Pending });
            }
            if(!await db.ProductStatuses
                .AnyAsync(x => string.Compare(x.Key, "for-sale") == 0, CancellationToken.None)
                .ConfigureAwait(true))
                db.ProductStatuses.Add(new ProductStatus { Key = "for-sale" });

            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
            db.Brands.Add(brand);
            var category = new ProductCategoryEntity { Name = $"Category_{uniqueId}" };
            db.ProductCategories.Add(category);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            var product = new ProductEntity
            {
                Name = $"Product_{uniqueId}",
                BrandId = brand.Id,
                CategoryId = category.Id,
                StatusId = "for-sale"
            };
            db.Products.Add(product);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            var v1 = new ProductVariant { ProductId = product.Id, Price = 100000, UrlSlug = $"slug1-{uniqueId}" };
            var v2 = new ProductVariant { ProductId = product.Id, Price = 200000, UrlSlug = $"slug2-{uniqueId}" };
            db.ProductVariants.AddRange(v1, v2);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            variantId1 = v1.Id;
            variantId2 = v2.Id;
        }

        var jsonContent = System.Text.Json.JsonSerializer
            .Serialize(
                new
                {
                    buyerId = user.Id,
                    products = new[]
                    {
                        new { productId = variantId1, count = 1 },
                        new { productId = variantId2, count = 2 }
                    }
                });
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/SalesOrders", httpContent).ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact(DisplayName = "SO_069 - Sắp xếp theo CreatedAt DESC")]
    public async Task GetOutputs_SortByCreatedAtDesc_ReturnsSortedResults()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Outputs.View ],
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

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if(!await db.OutputStatuses
                .AnyAsync(x => string.Compare(x.Key, OrderStatus.Pending) == 0, CancellationToken.None)
                .ConfigureAwait(true))
            {
                db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Pending });
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            }
            db.OutputOrders
                .AddRange(
                    new OutputEntity
                    {
                        StatusId = OrderStatus.Pending,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                        Notes = $"{uniqueId}_1"
                    },
                    new OutputEntity
                    {
                        StatusId = OrderStatus.Pending,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                        Notes = $"{uniqueId}_2"
                    },
                    new OutputEntity
                    {
                        StatusId = OrderStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        Notes = $"{uniqueId}_3"
                    });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var response = await _client.GetAsync($"/api/v1/SalesOrders?sorts=-createdAt&filters=Notes@={uniqueId}")
            .ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<OutputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
    }

    [Fact(DisplayName = "SO_070 - Phân trang với Page và PageSize")]
    public async Task GetOutputs_WithPagination_ReturnsPagedResults()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Outputs.View ],
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

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if(!await db.OutputStatuses
                .AnyAsync(x => string.Compare(x.Key, OrderStatus.Pending) == 0, CancellationToken.None)
                .ConfigureAwait(true))
            {
                db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Pending });
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            }
            for(int i = 0; i < 15; i++)
            {
                db.OutputOrders.Add(new OutputEntity { StatusId = OrderStatus.Pending, Notes = $"{uniqueId}_{i}" });
            }
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var response = await _client.GetAsync($"/api/v1/SalesOrders?page=1&pageSize=10&filters=Notes@={uniqueId}")
            .ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<OutputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Items.Should().HaveCount(10);
    }

    [Fact(DisplayName = "SO_071 - Soft delete đơn hàng")]
    public async Task DeleteOutput_ValidId_SetsDeletedAt()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Outputs.Delete, PermissionsList.Outputs.Create ],
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

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if(!await db.OutputStatuses
                .AnyAsync(x => string.Compare(x.Key, OrderStatus.Pending) == 0, CancellationToken.None)
                .ConfigureAwait(true))
            {
                db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Pending });
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            }
        }

        var variantId = await SeedProductVariantAsync(uniqueId, CancellationToken.None).ConfigureAwait(true);

        var jsonContent = System.Text.Json.JsonSerializer
            .Serialize(
                new
                {
                    buyerId = user.Id,
                    notes = "DeleteMe",
                    products = new[] { new { productId = variantId, count = 1 } }
                });
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/v1/SalesOrders", httpContent).ConfigureAwait(true);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var order = await createResponse.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        var deleteResponse = await _client.DeleteAsync($"/api/v1/SalesOrders/{order!.Id}", CancellationToken.None)
            .ConfigureAwait(true);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "SO_072 - Restore đơn hàng")]
    public async Task RestoreOutput_ValidId_ClearsDeletedAt()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Outputs.Delete, PermissionsList.Outputs.Create ],
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

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if(!await db.OutputStatuses
                .AnyAsync(x => string.Compare(x.Key, OrderStatus.Pending) == 0, CancellationToken.None)
                .ConfigureAwait(true))
            {
                db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Pending });
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            }
        }

        var variantId = await SeedProductVariantAsync(uniqueId, CancellationToken.None).ConfigureAwait(true);

        var jsonContent = System.Text.Json.JsonSerializer
            .Serialize(new { buyerId = user.Id, products = new[] { new { productId = variantId, count = 1 } } });
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/v1/SalesOrders", httpContent).ConfigureAwait(true);
        var order = await createResponse.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        await _client.DeleteAsync($"/api/v1/SalesOrders/{order!.Id}", CancellationToken.None).ConfigureAwait(true);

        var restoreResponse = await _client.PostAsync($"/api/v1/SalesOrders/{order.Id}/restore", null)
            .ConfigureAwait(true);
        restoreResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "SO_073 - GetDeletedOutputs chỉ trả về đơn đã xóa")]
    public async Task GetDeletedOutputs_ReturnsOnlyDeletedOrders()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Outputs.View ],
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

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if(!await db.OutputStatuses
                .AnyAsync(x => string.Compare(x.Key, OrderStatus.Pending) == 0, CancellationToken.None)
                .ConfigureAwait(true))
            {
                db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Pending });
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            }
            db.OutputOrders
                .Add(
                    new OutputEntity
                    {
                        StatusId = OrderStatus.Pending,
                        Notes = $"Deleted_{uniqueId}",
                        DeletedAt = DateTime.UtcNow
                    });
            db.OutputOrders
                .Add(
                    new OutputEntity { StatusId = OrderStatus.Pending, Notes = $"Active_{uniqueId}", DeletedAt = null });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var response = await _client.GetAsync($"/api/v1/SalesOrders/deleted?filters=Notes@={uniqueId}")
            .ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<OutputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Items.Should().Contain(x => string.Compare(x.Notes, $"Deleted_{uniqueId}") == 0);
        content.Items.Should().NotContain(x => x.Notes != null && string.Compare(x.Notes, $"Active_{uniqueId}") == 0);
    }

    [Fact(DisplayName = "SO_074 - UpdateOutput chỉ khi có quyền")]
    public async Task UpdateOutput_WithoutPermission_ReturnsForbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Outputs.View ],
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

        var request = new UpdateOutputForManagerCommand();
        var response = await _client.PutAsJsonAsync("/api/v1/SalesOrders/for-manager/1", request).ConfigureAwait(true);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "SO_075 - UpdateOutputStatus chuyển đổi không hợp lệ")]
    public async Task UpdateOutputStatus_InvalidTransition_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Outputs.Create, PermissionsList.Outputs.ChangeStatus ],
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

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if(!await db.OutputStatuses
                .AnyAsync(x => string.Compare(x.Key, OrderStatus.Pending) == 0, CancellationToken.None)
                .ConfigureAwait(true))
                db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Pending });

            if(!await db.OutputStatuses
                .AnyAsync(x => string.Compare(x.Key, OrderStatus.Completed) == 0, CancellationToken.None)
                .ConfigureAwait(true))
                db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Completed });

            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var variantId = await SeedProductVariantAsync(uniqueId, CancellationToken.None).ConfigureAwait(true);

        var jsonContent = System.Text.Json.JsonSerializer
            .Serialize(new { buyerId = user.Id, products = new[] { new { productId = variantId, count = 1 } } });
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var createResponse = await _client.PostAsync("/api/v1/SalesOrders", httpContent).ConfigureAwait(true);
        var order = await createResponse.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        var request = new UpdateOutputStatusCommand { StatusId = OrderStatus.Completed };
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{order!.Id}/status",
            request,
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "SO_076 - DeleteManyOutputs xóa nhiều đơn")]
    public async Task DeleteManyOutputs_ValidIds_DeletesAllOrders()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Outputs.Delete, PermissionsList.Outputs.Create ],
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

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if(!await db.OutputStatuses
                .AnyAsync(x => string.Compare(x.Key, OrderStatus.Pending) == 0, CancellationToken.None)
                .ConfigureAwait(true))
            {
                db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Pending });
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            }
        }

        var variantId = await SeedProductVariantAsync(uniqueId, CancellationToken.None).ConfigureAwait(true);

        var jsonContent = System.Text.Json.JsonSerializer
            .Serialize(new { buyerId = user.Id, products = new[] { new { productId = variantId, count = 1 } } });
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var r1 = await _client.PostAsync("/api/v1/SalesOrders", httpContent).ConfigureAwait(true);
        var o1 = await r1.Content.ReadFromJsonAsync<OutputResponse>(CancellationToken.None).ConfigureAwait(true);

        var httpContent2 = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        var r2 = await _client.PostAsync("/api/v1/SalesOrders", httpContent2).ConfigureAwait(true);
        var o2 = await r2.Content.ReadFromJsonAsync<OutputResponse>(CancellationToken.None).ConfigureAwait(true);

        var request = new DeleteManyOutputsCommand { Ids = [ o1!.Id!.Value, o2!.Id!.Value ] };
        var deleteJson = System.Text.Json.JsonSerializer.Serialize(request);
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/SalesOrders")
        {
            Content = new StringContent(deleteJson, System.Text.Encoding.UTF8, "application/json")
        };
        var response = await _client.SendAsync(deleteRequest).ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "SO_077 - RestoreManyOutputs khôi phục nhiều đơn")]
    public async Task RestoreManyOutputs_ValidIds_RestoresAllOrders()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Outputs.Delete, PermissionsList.Outputs.Create ],
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

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if(!await db.OutputStatuses
                .AnyAsync(x => string.Compare(x.Key, OrderStatus.Pending) == 0, CancellationToken.None)
                .ConfigureAwait(true))
            {
                db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Pending });
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            }
        }

        var variantId = await SeedProductVariantAsync(uniqueId, CancellationToken.None).ConfigureAwait(true);

        var jsonContent = System.Text.Json.JsonSerializer
            .Serialize(new { buyerId = user.Id, products = new[] { new { productId = variantId, count = 1 } } });

        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        var r1 = await _client.PostAsync("/api/v1/SalesOrders", httpContent).ConfigureAwait(true);
        var o1 = await r1.Content.ReadFromJsonAsync<OutputResponse>(CancellationToken.None).ConfigureAwait(true);
        await _client.DeleteAsync($"/api/v1/SalesOrders/{o1!.Id}", CancellationToken.None).ConfigureAwait(true);

        var httpContent2 = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        var r2 = await _client.PostAsync("/api/v1/SalesOrders", httpContent2).ConfigureAwait(true);
        var o2 = await r2.Content.ReadFromJsonAsync<OutputResponse>(CancellationToken.None).ConfigureAwait(true);
        await _client.DeleteAsync($"/api/v1/SalesOrders/{o2!.Id}", CancellationToken.None).ConfigureAwait(true);

        var request = new RestoreManyOutputsCommand { Ids = [ o1.Id!.Value, o2.Id!.Value ] };
        var response = await _client.PostAsJsonAsync("/api/v1/SalesOrders/restore", request).ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "SO_078 - UpdateManyOutputStatus cập nhật trạng thái nhiều đơn")]
    public async Task UpdateManyOutputStatus_ValidIds_UpdatesAllStatuses()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Outputs.Create, PermissionsList.Outputs.ChangeStatus ],
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

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var statuses = new[] { OrderStatus.Pending, OrderStatus.ConfirmedCod };
            foreach(var s in statuses)
            {
                if(!await db.OutputStatuses
                    .AnyAsync(x => string.Compare(x.Key, s) == 0, CancellationToken.None)
                    .ConfigureAwait(true))
                    db.OutputStatuses.Add(new OutputStatusEntity { Key = s });
            }
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var variantId = await SeedProductVariantAsync(uniqueId, CancellationToken.None).ConfigureAwait(true);

        await SeedInventoryAsync(variantId, 10, uniqueId, CancellationToken.None).ConfigureAwait(true);

        var jsonContent = System.Text.Json.JsonSerializer
            .Serialize(new { buyerId = user.Id, products = new[] { new { productId = variantId, count = 1 } } });
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var r1 = await _client.PostAsync("/api/v1/SalesOrders", httpContent).ConfigureAwait(true);
        var o1 = await r1.Content.ReadFromJsonAsync<OutputResponse>(CancellationToken.None).ConfigureAwait(true);

        var httpContent2 = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        var r2 = await _client.PostAsync("/api/v1/SalesOrders", httpContent2).ConfigureAwait(true);
        var o2 = await r2.Content.ReadFromJsonAsync<OutputResponse>(CancellationToken.None).ConfigureAwait(true);

        var request = new UpdateManyOutputStatusCommand
        {
            Ids = [ o1!.Id!.Value, o2!.Id!.Value ],
            StatusId = OrderStatus.ConfirmedCod
        };
        var response = await _client.PatchAsJsonAsync("/api/v1/SalesOrders/status", request, CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "SO_079 - GetMyPurchases chỉ trả về đơn của user đăng nhập")]
    public async Task GetMyPurchases_AuthenticatedUser_ReturnsUserOrders()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
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

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if(!await db.OutputStatuses
                .AnyAsync(x => string.Compare(x.Key, OrderStatus.Pending) == 0, CancellationToken.None)
                .ConfigureAwait(true))
            {
                db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Pending });
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            }
            db.OutputOrders.Add(new OutputEntity { StatusId = OrderStatus.Pending, });
        }

        var variantId = await SeedProductVariantAsync(uniqueId, CancellationToken.None).ConfigureAwait(true);

        var jsonContent = System.Text.Json.JsonSerializer
            .Serialize(
                new
                {
                    buyerId = user.Id,
                    notes = $"Mine_{uniqueId}",
                    products = new[] { new { productId = variantId, count = 1 } }
                });
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var createResponse = await _client.PostAsync("/api/v1/SalesOrders", httpContent).ConfigureAwait(true);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await _client.GetAsync("/api/v1/SalesOrders/my-purchases").ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<OutputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Items.Should().Contain(x => string.Compare(x.Notes, $"Mine_{uniqueId}") == 0);
    }

    [Fact(DisplayName = "SO_080 - GetPurchasesByID lấy đơn theo BuyerId (manager)")]
    public async Task GetPurchasesByID_ByManager_ReturnsUserOrders()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var managerName = $"manager_{uniqueId}";
        var managerEmail = $"manager_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            managerName,
            password,
            [ PermissionsList.Outputs.View ],
            CancellationToken.None,
            managerEmail)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            managerName,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var buyer = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"buyer_{uniqueId}",
            password,
            [],
            CancellationToken.None,
            $"buyer_{uniqueId}@gmail.com")
            .ConfigureAwait(true);
        var buyerId = buyer.Id;

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if(!await db.OutputStatuses
                .AnyAsync(x => string.Compare(x.Key, OrderStatus.Pending) == 0, CancellationToken.None)
                .ConfigureAwait(true))
            {
                db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Pending });
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            }
            db.OutputOrders
                .Add(
                    new OutputEntity
                    {
                        StatusId = OrderStatus.Pending,
                        BuyerId = buyerId,
                        Notes = $"PurchasesOf_{uniqueId}"
                    });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var response = await _client.GetAsync($"/api/v1/SalesOrders/get-purchases/{buyerId}").ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<OutputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Items.Should().Contain(x => string.Compare(x.Notes, $"PurchasesOf_{uniqueId}") == 0);
    }
#pragma warning restore CRR0035
#pragma warning restore IDE0079
}
