using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Domain.Constants.Permission;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public class ChatFeedback : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public ChatFeedback(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(true);
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "CHATFEEDBACK_INT_01 - Gửi phản hồi thành công cho run của chính mình, lưu đúng ReportedBy")]
    public async Task CreateFeedback_OwnRun_PersistsWithReporterId()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, $"user_{uniqueId}", "Password123!", [Permissions.Marketing.BannerManagement.Create],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client, $"user_{uniqueId}", "Password123!", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        Guid runId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var session = new ChatSession { UserId = user.Id, Title = "Test" };
            db.ChatSessions.Add(session);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            var run = new Domain.Entities.ChatRun { SessionId = session.Id, UserMessage = "hoi" };
            db.ChatRuns.Add(run);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            runId = run.Id;
        }

        var response = await HttpClientJsonExtensions.PostAsJsonAsync(
            _client, $"/api/v1/manager-chat/runs/{runId}/feedback", new { comment = "Số liệu sai" },
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var saved = await verifyDb.ChatFeedbacks.FirstOrDefaultAsync(
            f => f.ChatRunId == runId, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        saved.Should().NotBeNull();
        saved!.ReportedBy.Should().Be(user.Id);
        saved.Comment.Should().Be("Số liệu sai");
    }

    [Fact(DisplayName = "CHATFEEDBACK_INT_02 - Không thể gửi phản hồi cho run của người khác")]
    public async Task CreateFeedback_OtherUsersRun_ReturnsNotFound()
    {
        var ownerId = Guid.NewGuid().ToString("N")[..8];
        var owner = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, $"owner_{ownerId}", "Password123!", [Permissions.Marketing.BannerManagement.Create],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        Guid runId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var session = new ChatSession { UserId = owner.Id, Title = "Test" };
            db.ChatSessions.Add(session);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            var run = new Domain.Entities.ChatRun { SessionId = session.Id, UserMessage = "hoi" };
            db.ChatRuns.Add(run);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            runId = run.Id;
        }

        var otherId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, $"other_{otherId}", "Password123!", [Permissions.Marketing.BannerManagement.Create],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client, $"other_{otherId}", "Password123!", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var response = await HttpClientJsonExtensions.PostAsJsonAsync(
            _client, $"/api/v1/manager-chat/runs/{runId}/feedback", new { comment = "test" },
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
