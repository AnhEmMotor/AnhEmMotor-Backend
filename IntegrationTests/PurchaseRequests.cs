using Application.ApiContracts.PurchaseRequest.Requests;
using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Application.Features.PurchaseRequests.Commands.CreatePurchaseRequest;
using Domain.Constants.Permission.Permissions;
using Domain.Primitives;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using ProductEntities = Domain.Entities.Product;
using ProductVariantEntities = Domain.Entities.ProductVariant;
using PurchaseRequestEntities = Domain.Entities.PurchaseRequest;

namespace IntegrationTests;

public class PurchaseRequests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public PurchaseRequests(IntegrationTestWebAppFactory factory)
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

    private async Task<int> SeedProductVariantAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var product = new ProductEntities { Name = "Integration Test Product" };
        db.Products.Add(product);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);

        var variant = new ProductVariantEntities { ProductId = product.Id, VariantName = "Integration Test Variant" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);

        return variant.Id;
    }

    #pragma warning disable IDE0079 
    #pragma warning disable CRR0035

    [Fact(DisplayName = "PR_031 - Integration: Tạo PR thành công lưu dữ liệu thực tế vào DB")]
    public async Task PR_031_CreatePurchaseRequest_Integration_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.PurchaseRequests.Create, Domain.Constants.Permission.Permissions.PurchaseRequests.View],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        var variantId = await SeedProductVariantAsync().ConfigureAwait(true);

        var command = new CreatePurchaseRequestCommand
        {
            Note = "Integration Test PR Note",
            Items = [new CreatePurchaseRequestItemRequest { ProductVariantId = variantId, Quantity = 15 }]
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/purchase-requests");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(command);

        var response = await _client.SendAsync(requestMessage, TestContext.Current.CancellationToken).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseData = await response.Content
            .ReadFromJsonAsync<PurchaseRequestDetailResponse>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        responseData.Should().NotBeNull();
        string.Compare(responseData!.Note, "Integration Test PR Note").Should().Be(0);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var prInDb = await verifyDb.Set<PurchaseRequestEntities>().FindAsync(new object[] { responseData.Id }, TestContext.Current.CancellationToken).ConfigureAwait(true);
        prInDb.Should().NotBeNull();
        string.Compare(prInDb!.Note, "Integration Test PR Note").Should().Be(0);
    }

    [Fact(DisplayName = "PR_032 - Integration: Lấy danh sách PR có phân trang & lọc")]
    public async Task PR_032_GetPurchaseRequests_Integration_Pagination()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.PurchaseRequests.View],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Set<PurchaseRequestEntities>().RemoveRange(db.Set<PurchaseRequestEntities>());
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

            var list = new List<PurchaseRequestEntities>();
            for (int i = 1; i <= 15; i++)
            {
                list.Add(new PurchaseRequestEntities { Note = $"PR Note {i}", Status = "draft" });
            }
            await db.Set<PurchaseRequestEntities>().AddRangeAsync(list, TestContext.Current.CancellationToken).ConfigureAwait(true);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v1/purchase-requests?Page=1&PageSize=10");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.SendAsync(requestMessage, TestContext.Current.CancellationToken).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content
            .ReadFromJsonAsync<PagedResult<PurchaseRequestListResponse>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
    }

    [Fact(DisplayName = "PR_033 - Integration: Lấy chi tiết PR theo ID thành công")]
    public async Task PR_033_GetPurchaseRequestById_Integration_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.PurchaseRequests.View],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        int prId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var pr = new PurchaseRequestEntities { Note = "Specific GetById Note", Status = "draft" };
            db.Set<PurchaseRequestEntities>().Add(pr);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            prId = pr.Id;
        }

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/purchase-requests/{prId}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.SendAsync(requestMessage, TestContext.Current.CancellationToken).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseData = await response.Content
            .ReadFromJsonAsync<PurchaseRequestDetailResponse>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        responseData.Should().NotBeNull();
        responseData!.Id.Should().Be(prId);
        string.Compare(responseData.Note, "Specific GetById Note").Should().Be(0);
    }

    [Fact(DisplayName = "PR_034 - Integration: Gửi PR (Send PR) thành công thay đổi trạng thái trong DB")]
    public async Task PR_034_SendPurchaseRequest_Integration_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.PurchaseRequests.Send],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        int prId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var pr = new PurchaseRequestEntities { Note = "To Be Sent Note", Status = "draft" };
            db.Set<PurchaseRequestEntities>().Add(pr);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            prId = pr.Id;
        }

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/purchase-requests/{prId}/send");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.SendAsync(requestMessage, TestContext.Current.CancellationToken).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var prInDb = await verifyDb.Set<PurchaseRequestEntities>().FindAsync(new object[] { prId }, TestContext.Current.CancellationToken).ConfigureAwait(true);
        prInDb.Should().NotBeNull();
        string.Compare(prInDb!.Status, "sent").Should().Be(0);
    }

    [Fact(DisplayName = "PR_035 - Integration: Duyệt PR lưu đúng ApprovedBy")]
    public async Task PR_035_ApprovePurchaseRequest_Integration_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";

        var adminUser = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.PurchaseRequests.ApproveReject],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        int prId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var pr = new PurchaseRequestEntities { Note = "To Be Approved Note", Status = "sent" };
            db.Set<PurchaseRequestEntities>().Add(pr);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            prId = pr.Id;
        }

        var request = new ApproveRejectPurchaseRequestRequest { Status = "approve" };
        var requestMessage = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/purchase-requests/{prId}/status");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(request);

        var response = await _client.SendAsync(requestMessage, TestContext.Current.CancellationToken).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var prInDb = await verifyDb.Set<PurchaseRequestEntities>().FindAsync(new object[] { prId }, TestContext.Current.CancellationToken).ConfigureAwait(true);
        prInDb.Should().NotBeNull();
        string.Compare(prInDb!.Status, "approve").Should().Be(0);
        prInDb.ApprovedBy.Should().Be(adminUser.Id);
    }

    [Fact(DisplayName = "PR_036 - Integration: Xóa PR hoàn toàn khỏi DB")]
    public async Task PR_036_DeletePurchaseRequest_Integration_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.PurchaseRequests.Delete],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        int prId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var pr = new PurchaseRequestEntities { Note = "To Be Deleted Note", Status = "draft" };
            db.Set<PurchaseRequestEntities>().Add(pr);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            prId = pr.Id;
        }

        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/purchase-requests/{prId}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.SendAsync(requestMessage, TestContext.Current.CancellationToken).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var prInDb = await verifyDb.Set<PurchaseRequestEntities>().FindAsync(new object[] { prId }, TestContext.Current.CancellationToken).ConfigureAwait(true);
        prInDb.Should().BeNull();
    }

    [Fact(DisplayName = "PR_037 - Integration: Lấy danh sách yêu cầu mua hàng hiển thị đúng số mặt hàng và tên người tạo")]
    public async Task PR_037_GetPurchaseRequests_List_CorrectTotalItemsAndCreatedByName()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.PurchaseRequests.View],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var variantId = await SeedProductVariantAsync().ConfigureAwait(true);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var purchaseRequest = new PurchaseRequestEntities
            {
                Status = "draft",
                Note = "List Test PR Note",
                CreatedBy = user.Id,
                PurchaseRequestItems =
                [
                    new Domain.Entities.PurchaseRequestItem
                    {
                        ProductVariantId = variantId,
                        Quantity = 5
                    }
                ]
            };
            db.Set<PurchaseRequestEntities>().Add(purchaseRequest);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v1/purchase-requests?Page=1&PageSize=10");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.SendAsync(requestMessage, TestContext.Current.CancellationToken).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content
            .ReadFromJsonAsync<PagedResult<PurchaseRequestListResponse>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        var item = result.Items[0];
        item.TotalItems.Should().Be(1);
        string.Compare(item.CreatedByName, user.FullName).Should().Be(0);
    }

    [Fact(DisplayName = "PR_038 - Integration: Lấy chi tiết yêu cầu mua hàng hiển thị đúng tên người tạo và người duyệt")]
    public async Task PR_038_GetPurchaseRequestById_Details_CorrectCreatedByNameAndApprovedByName()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.PurchaseRequests.View],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);
        var adminUniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminUsername = $"admin_{adminUniqueId}";
        var adminEmail = $"admin_{adminUniqueId}@gmail.com";
        var adminPassword = "ThisIsStrongPassword1@";
        var adminUser = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminUsername,
            adminPassword,
            [Domain.Constants.Permission.Permissions.PurchaseRequests.View],
            TestContext.Current.CancellationToken,
            adminEmail)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var variantId = await SeedProductVariantAsync().ConfigureAwait(true);
        int purchaseRequestId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var purchaseRequest = new PurchaseRequestEntities
            {
                Status = "approved",
                Note = "Details Test PR Note",
                CreatedBy = user.Id,
                ApprovedBy = adminUser.Id,
                PurchaseRequestItems =
                [
                    new Domain.Entities.PurchaseRequestItem
                    {
                        ProductVariantId = variantId,
                        Quantity = 10
                    }
                ]
            };
            db.Set<PurchaseRequestEntities>().Add(purchaseRequest);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            purchaseRequestId = purchaseRequest.Id;
        }
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/purchase-requests/{purchaseRequestId}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.SendAsync(requestMessage, TestContext.Current.CancellationToken).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content
            .ReadFromJsonAsync<PurchaseRequestDetailResponse>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        responseData.Should().NotBeNull();
        string.Compare(responseData!.CreatedByName, user.FullName).Should().Be(0);
        string.Compare(responseData.ApprovedByName, adminUser.FullName).Should().Be(0);
    }

    [Fact(DisplayName = "PR_039 - Integration: Lấy chi tiết PR đã duyệt thành công")]
    public async Task PR_039_GetApprovedPurchaseRequestById_Integration_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.InventoryReceipts.Create],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        var variantId = await SeedProductVariantAsync().ConfigureAwait(true);
        int purchaseRequestId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var purchaseRequest = new PurchaseRequestEntities
            {
                Status = "approve",
                Note = "Approved Details Test PR Note",
                CreatedBy = user.Id,
                ApprovedBy = user.Id,
                PurchaseRequestItems =
                [
                    new Domain.Entities.PurchaseRequestItem
                    {
                        ProductVariantId = variantId,
                        Quantity = 10
                    }
                ]
            };
            db.Set<PurchaseRequestEntities>().Add(purchaseRequest);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            purchaseRequestId = purchaseRequest.Id;
        }

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/purchase-requests/approved/{purchaseRequestId}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.SendAsync(requestMessage, TestContext.Current.CancellationToken).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseData = await response.Content
            .ReadFromJsonAsync<ApprovedPurchaseRequestDetailResponse>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        responseData.Should().NotBeNull();
        responseData!.Id.Should().Be(purchaseRequestId);
        string.Compare(responseData.Note, "Approved Details Test PR Note").Should().Be(0);
        responseData.Items.Should().ContainSingle();
        responseData.Items[0].UnimportedQuantity.Should().Be(10);
    }

    [Fact(DisplayName = "PR_040 - Integration: Lấy chi tiết PR chưa duyệt báo lỗi")]
    public async Task PR_040_GetApprovedPurchaseRequestById_NotApproved_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.InventoryReceipts.Create],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        int purchaseRequestId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var purchaseRequest = new PurchaseRequestEntities
            {
                Status = "draft",
                Note = "Draft PR Note",
                CreatedBy = user.Id
            };
            db.Set<PurchaseRequestEntities>().Add(purchaseRequest);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            purchaseRequestId = purchaseRequest.Id;
        }

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/purchase-requests/approved/{purchaseRequestId}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.SendAsync(requestMessage, TestContext.Current.CancellationToken).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "PR_041 - Integration: Lấy chi tiết PR không tồn tại báo lỗi")]
    public async Task PR_041_GetApprovedPurchaseRequestById_NotFound_ReturnsNotFound()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.InventoryReceipts.Create],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v1/purchase-requests/approved/-1");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.SendAsync(requestMessage, TestContext.Current.CancellationToken).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #pragma warning restore CRR0035
    #pragma warning restore IDE0079
}
