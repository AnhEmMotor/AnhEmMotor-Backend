using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Features.InventoryReceipts.Commands.DeleteManyInventoryReceipts;
using Application.Features.InventoryReceipts.Commands.RestoreManyInventoryReceipts;
using Domain.Constants.Permission.Permissions;
using Domain.Constants;
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
using System.Threading.Tasks;
using Xunit;
using InventoryReceiptEntities = Domain.Entities.InventoryReceipt;
using InventoryReceiptStatusConstants = Domain.Constants.InventoryReceiptStatus;
using InventoryReceiptStatusEntity = Domain.Entities.InventoryReceiptStatus;

namespace IntegrationTests;

public class InventoryReceipts : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public InventoryReceipts(IntegrationTestWebAppFactory factory)
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

    private static async Task EnsureDraftInventoryReceiptStatusAsync(ApplicationDBContext db)
    {
        var hasDraftStatus = await db.InventoryReceiptStatuses
            .AnyAsync(x => string.Compare(x.Key, InventoryReceiptStatusConstants.Draft) == 0, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        if (!hasDraftStatus)
        {
            db.InventoryReceiptStatuses.Add(new InventoryReceiptStatusEntity { Key = InventoryReceiptStatusConstants.Draft });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
    }
    #pragma warning disable IDE0079 
    #pragma warning disable CRR0035

    [Fact(DisplayName = "IR_014 - Xóa tạm thời một phiếu nhập kho chưa được duyệt thành công.")]
    public async Task IR_014_DeleteInventoryReceipt_Integration_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.InventoryReceipts.Delete],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        int receiptId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await EnsureDraftInventoryReceiptStatusAsync(db).ConfigureAwait(true);
            var receipt = new InventoryReceiptEntities { StatusId = "draft", Notes = "To Be Deleted Single" };
            db.Set<InventoryReceiptEntities>().Add(receipt);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            receiptId = receipt.Id;
        }

        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/InventoryReceipts/{receiptId}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.SendAsync(requestMessage, TestContext.Current.CancellationToken).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var receiptInDb = await verifyDb.Set<InventoryReceiptEntities>().FindAsync([receiptId], TestContext.Current.CancellationToken).ConfigureAwait(true);
        receiptInDb.Should().BeNull();
    }

    [Fact(DisplayName = "IR_015 - Xóa tạm thời nhiều phiếu nhập kho cùng lúc thành công.")]
    public async Task IR_015_DeleteManyInventoryReceipts_Integration_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.InventoryReceipts.Delete],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        var ids = new List<int>();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await EnsureDraftInventoryReceiptStatusAsync(db).ConfigureAwait(true);
            var r1 = new InventoryReceiptEntities { StatusId = "draft", Notes = "To Be Deleted Many 1" };
            var r2 = new InventoryReceiptEntities { StatusId = "draft", Notes = "To Be Deleted Many 2" };
            db.Set<InventoryReceiptEntities>().AddRange(r1, r2);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            ids.Add(r1.Id);
            ids.Add(r2.Id);
        }

        var command = new DeleteManyInventoryReceiptsCommand { Ids = ids };
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/InventoryReceipts");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(command);

        var response = await _client.SendAsync(requestMessage, TestContext.Current.CancellationToken).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        foreach (var id in ids)
        {
            var receiptInDb = await verifyDb.Set<InventoryReceiptEntities>().FindAsync([id], TestContext.Current.CancellationToken).ConfigureAwait(true);
            receiptInDb.Should().BeNull();
        }
    }

    [Fact(DisplayName = "IR_016 - Khôi phục thành công một phiếu nhập kho đã bị xóa tạm trước đó.")]
    public async Task IR_016_RestoreInventoryReceipt_Integration_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.InventoryReceipts.Delete],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        int receiptId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var hasDraftStatus = await db.InventoryReceiptStatuses
                .AnyAsync(x => string.Compare(x.Key, InventoryReceiptStatusConstants.Draft) == 0, TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            if (!hasDraftStatus)
            {
                db.InventoryReceiptStatuses.Add(new InventoryReceiptStatusEntity { Key = InventoryReceiptStatusConstants.Draft });
            }
            var receipt = new InventoryReceiptEntities { StatusId = "draft", Notes = "Deleted Receipt", DeletedAt = DateTimeOffset.UtcNow };
            db.Set<InventoryReceiptEntities>().Add(receipt);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            receiptId = receipt.Id;
        }

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/InventoryReceipts/{receiptId}/restore");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.SendAsync(requestMessage, TestContext.Current.CancellationToken).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var receiptInDb = await verifyDb.Set<InventoryReceiptEntities>().FindAsync([receiptId], TestContext.Current.CancellationToken).ConfigureAwait(true);
        receiptInDb.Should().NotBeNull();
        receiptInDb!.DeletedAt.Should().BeNull();
    }

    [Fact(DisplayName = "IR_017 - Khôi phục thành công nhiều phiếu nhập kho đã bị xóa tạm cùng lúc.")]
    public async Task IR_017_RestoreManyInventoryReceipts_Integration_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.InventoryReceipts.Delete],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        var ids = new List<int>();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await EnsureDraftInventoryReceiptStatusAsync(db).ConfigureAwait(true);
            var r1 = new InventoryReceiptEntities { StatusId = "draft", Notes = "Deleted Many 1", DeletedAt = DateTimeOffset.UtcNow };
            var r2 = new InventoryReceiptEntities { StatusId = "draft", Notes = "Deleted Many 2", DeletedAt = DateTimeOffset.UtcNow };
            db.Set<InventoryReceiptEntities>().AddRange(r1, r2);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            ids.Add(r1.Id);
            ids.Add(r2.Id);
        }

        var command = new RestoreManyInventoryReceiptsCommand { Ids = ids };
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/InventoryReceipts/restore");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(command);

        var response = await _client.SendAsync(requestMessage, TestContext.Current.CancellationToken).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        foreach (var id in ids)
        {
            var receiptInDb = await verifyDb.Set<InventoryReceiptEntities>().FindAsync([id], TestContext.Current.CancellationToken).ConfigureAwait(true);
            receiptInDb.Should().NotBeNull();
            receiptInDb!.DeletedAt.Should().BeNull();
        }
    }
    #pragma warning restore CRR0035
    #pragma warning restore IDE0079
}
