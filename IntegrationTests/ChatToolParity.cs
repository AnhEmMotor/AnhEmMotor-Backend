using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Common.Interfaces;
using Application.Features.ChatTools.Common;
using Application.Features.ChatTools.Queries.GetSalesSummaryForChat;
using Domain.Constants.Order;
using Domain.Constants.Permission;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebAPI.Controllers;

namespace IntegrationTests;

/// <summary>
/// Stage 16.6 — tool chat phải khớp với dữ liệu thật trong DB (Postgres qua Testcontainers), không lệch
/// do bỏ sót bộ lọc trạng thái hay lệch múi giờ. Không phải mọi trường hợp biên (16.6 liệt kê 8 case) được
/// phủ ở đây — soft-delete và JOIN nhân bản đã có unit test riêng (ChatToolsSoftDeleteGuard, ChatTools.cs);
/// 4 case dưới đây là các case CHỈ kiểm chứng được với DB thật (trạng thái đơn, ranh giới giờ VN/tháng).
/// </summary>
public class ChatToolParity : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;

    public ChatToolParity(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(true);
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "PARITY_01 - Khoảng thời gian rỗng (không có đơn) trả toàn 0đ cho từng ngày, không lỗi, không nhầm 0 thành thiếu dữ liệu")]
    public async Task SalesSummary_EmptyRange_ReturnsZeroRevenuePerDay()
    {
        var client = await CreateAuthenticatedClientAsync().ConfigureAwait(true);

        var response = await HttpClientJsonExtensions.PostAsJsonAsync(
            client,
            "/internal/chat/tools/analytics/sales",
            new GetSalesSummaryForChatRequest { FromDate = new DateOnly(2020, 1, 1), ToDate = new DateOnly(2020, 1, 31), Limit = 25 },
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken).ConfigureAwait(true));
        }
        var envelope = await response.Content
            .ReadFromJsonAsync<ChatToolEnvelope<ChatDailyRevenueDto>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        // GetDailyRevenueAsync điền đủ mỗi ngày trong khoảng (kể cả không có đơn) với TotalRevenue = 0đ —
        // đây CHÍNH LÀ khoảng trống ở Stage 16.2 mục #9 (không phân biệt "chưa có dữ liệu" với "bằng 0")
        // vì ChatDailyRevenueDto.TotalRevenue là decimal không nullable. Ghi nhận làm nợ kỹ thuật, không
        // sửa trong Stage này vì cần đổi shape DTO + prompt + FE — xem GLOSSARY.md và mục 16.2 #9.
        envelope!.TotalCount.Should().Be(31);
        envelope.Items.Should().OnlyContain(i => i.TotalRevenue == 0m);
    }

    [Fact(DisplayName = "PARITY_02 - Đơn huỷ/nháp không được tính vào doanh thu")]
    public async Task SalesSummary_ExcludesCancelledOrders()
    {
        await SeedOutputStatusesAsync().ConfigureAwait(true);
        var day = new DateOnly(2026, 6, 15);
        var dayStartUtc = new DateTimeOffset(day.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(7)).UtcDateTime;

        await SeedOutputAsync(OrderStatus.Completed, dayStartUtc.AddHours(3), 1_000_000m).ConfigureAwait(true);
        await SeedOutputAsync(OrderStatus.Cancelled, dayStartUtc.AddHours(4), 9_000_000m).ConfigureAwait(true);

        var client = await CreateAuthenticatedClientAsync().ConfigureAwait(true);
        var response = await HttpClientJsonExtensions.PostAsJsonAsync(
            client,
            "/internal/chat/tools/analytics/sales",
            new GetSalesSummaryForChatRequest { FromDate = day, ToDate = day },
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.EnsureSuccessStatusCode();
        var envelope = await response.Content
            .ReadFromJsonAsync<ChatToolEnvelope<ChatDailyRevenueDto>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        envelope!.Items.Should().ContainSingle();
        envelope.Items[0].TotalRevenue.Should().Be(1_000_000m,
            "đơn huỷ (9 triệu) không được cộng vào doanh thu ngày này");
    }

    [Fact(DisplayName = "PARITY_03 - Khung giờ 00:00-07:00 giờ VN không bị lệch sang ngày hôm trước (Stage 16.2 mục #2)")]
    public async Task SalesSummary_EarlyMorningVietnamWindow_UsesCorrectVietnamDay()
    {
        await SeedOutputStatusesAsync().ConfigureAwait(true);
        // "Bây giờ" = 2026-07-26T02:00:00+07:00 = 2026-07-25T19:00:00Z — nếu tool suy "hôm nay" theo UTC
        // trần sẽ ra 25/07, đúng phải là 26/07 giờ VN.
        var fakeNowUtc = new DateTimeOffset(2026, 7, 25, 19, 0, 0, TimeSpan.Zero);
        var orderCreatedAtUtc = new DateTimeOffset(2026, 7, 25, 20, 0, 0, TimeSpan.Zero); // = 2026-07-26T03:00+07:00
        await SeedOutputAsync(OrderStatus.Completed, orderCreatedAtUtc.UtcDateTime, 2_000_000m).ConfigureAwait(true);

        using var scopedFactory = _factory.WithWebHostBuilder(
            builder => builder.ConfigureServices(
                services => services.Replace(
                    ServiceDescriptor.Singleton<IServerDateProvider>(new FakeServerDateProvider(fakeNowUtc)))));
        var client = await CreateAuthenticatedClientAsync(scopedFactory).ConfigureAwait(true);

        // Không truyền FromDate/ToDate — handler tự suy "hôm nay" từ IServerDateProvider đã fake ở trên.
        var response = await HttpClientJsonExtensions.PostAsJsonAsync(
            client,
            "/internal/chat/tools/analytics/sales",
            new GetSalesSummaryForChatRequest(),
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.EnsureSuccessStatusCode();
        var envelope = await response.Content
            .ReadFromJsonAsync<ChatToolEnvelope<ChatDailyRevenueDto>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        envelope!.Items.Should().Contain(i => i.ReportDay == new DateOnly(2026, 7, 26) && i.TotalRevenue == 2_000_000m,
            "đơn tạo lúc 03:00 giờ VN ngày 26/07 phải được tính vào ngày 26/07, không phải 25/07 theo UTC trần");
    }

    [Fact(DisplayName = "PARITY_04 - Khoảng ngày qua ranh giới tháng tính đủ cả hai tháng")]
    public async Task SalesSummary_AcrossMonthBoundary_IncludesBothMonths()
    {
        await SeedOutputStatusesAsync().ConfigureAwait(true);
        var lastDayOfJune = new DateTimeOffset(2026, 6, 30, 10, 0, 0, TimeSpan.FromHours(7)).UtcDateTime;
        var firstDayOfJuly = new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.FromHours(7)).UtcDateTime;
        await SeedOutputAsync(OrderStatus.Completed, lastDayOfJune, 500_000m).ConfigureAwait(true);
        await SeedOutputAsync(OrderStatus.Completed, firstDayOfJuly, 700_000m).ConfigureAwait(true);

        var client = await CreateAuthenticatedClientAsync().ConfigureAwait(true);
        var response = await HttpClientJsonExtensions.PostAsJsonAsync(
            client,
            "/internal/chat/tools/analytics/sales",
            new GetSalesSummaryForChatRequest
            {
                FromDate = new DateOnly(2026, 6, 29),
                ToDate = new DateOnly(2026, 7, 2),
                Limit = 25
            },
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.EnsureSuccessStatusCode();
        var envelope = await response.Content
            .ReadFromJsonAsync<ChatToolEnvelope<ChatDailyRevenueDto>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        envelope!.Items.Should().Contain(i => i.ReportDay == new DateOnly(2026, 6, 30) && i.TotalRevenue == 500_000m);
        envelope.Items.Should().Contain(i => i.ReportDay == new DateOnly(2026, 7, 1) && i.TotalRevenue == 700_000m);
    }

    private async Task SeedOutputStatusesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        foreach (var key in new[] { OrderStatus.Completed, OrderStatus.Delivering, OrderStatus.WaitingPickup, OrderStatus.Cancelled })
        {
            if (!await db.OutputStatuses.AnyAsync(s => s.Key == key, TestContext.Current.CancellationToken).ConfigureAwait(true))
            {
                db.OutputStatuses.Add(new OutputStatus { Key = key });
            }
        }
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
    }

    private async Task SeedOutputAsync(string statusId, DateTime createdAtUtc, decimal price)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        db.OutputOrders.Add(
            new Output
            {
                StatusId = statusId,
                CreatedAt = new DateTimeOffset(createdAtUtc, TimeSpan.Zero),
                OutputInfos = [new OutputInfo { Price = price, Count = 1 }]
            });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(WebApplicationFactory<Program>? factory = null)
    {
        WebApplicationFactory<Program> effectiveFactory = factory ?? _factory;
        var client = effectiveFactory.CreateClient();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            effectiveFactory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [Permissions.Admin.DashboardManagement.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            client, $"user_{uniqueId}", "Password123!", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        return client;
    }
}
