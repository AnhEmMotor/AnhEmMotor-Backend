using Application.DTOs.Chat;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Services.Ai.Runs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace UnitTests;

public class ChatRunExecutorTests
{
    private static async IAsyncEnumerable<SidecarEvent> StreamThatGetsCancelledMidway()
    {
        yield return new SidecarEvent(ChatRunEventType.TextDelta, "Đây là phần ");
        yield return new SidecarEvent(ChatRunEventType.TextDelta, "đã sinh ra trước khi bị huỷ.");
        await Task.Yield();
        // Mô phỏng đúng những gì xảy ra khi CancelChatRunCommandHandler huỷ CancellationToken
        // giữa lúc executor đang chờ chunk kế tiếp: await foreach ném OperationCanceledException.
        throw new OperationCanceledException();
    }

    [Fact(DisplayName = "EXEC_01 - Bị huỷ giữa chừng vẫn lưu được phần AI đã sinh ra, không phải chuỗi rỗng")]
    public async Task ProcessRun_SavesAccumulatedOutput_WhenCancelledMidStream()
    {
        var runId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var run = new ChatRun { Id = runId, SessionId = sessionId, UserMessage = "hỏi gì đó", Status = ChatRunStatus.Pending };

        var readRepo = new Mock<IChatReadRepository>();
        readRepo.Setup(x => x.GetRunByIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(run);

        var streamClient = new Mock<ISidecarStreamClient>();
        streamClient
            .Setup(x => x.StreamAsync(runId, sessionId, run.UserMessage, "user-token", It.IsAny<CancellationToken>()))
            .Returns(StreamThatGetsCancelledMidway());

        var tokenStore = new Mock<IChatRunTokenStore>();
        tokenStore.Setup(x => x.Take(runId)).Returns("user-token");

        var writer = new Mock<IChatRunWriter>();
        writer.Setup(x => x.AppendAsync(runId, It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(0L);

        string? savedOutputOnCancel = null;
        var cancelled = new TaskCompletionSource();
        writer.Setup(x => x.CancelAsync(runId, It.IsAny<string>()))
              .Callback<Guid, string>((_, finalOutput) =>
              {
                  savedOutputOnCancel = finalOutput;
                  cancelled.TrySetResult();
              })
              .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(readRepo.Object);
        services.AddSingleton(streamClient.Object);
        services.AddSingleton(tokenStore.Object);
        services.AddSingleton(writer.Object);
        var provider = services.BuildServiceProvider();

        var queue = new ChatRunQueue();
        var cancellationRegistry = new Mock<IChatRunCancellationRegistry>();
        var executor = new ChatRunExecutor(queue, provider, cancellationRegistry.Object, NullLogger<ChatRunExecutor>.Instance);

        await executor.StartAsync(CancellationToken.None);
        try
        {
            await queue.EnqueueAsync(runId);

            var finished = await Task.WhenAny(cancelled.Task, Task.Delay(TimeSpan.FromSeconds(5)));
            finished.Should().Be(cancelled.Task, "CancelAsync phải được gọi trong vòng 5s sau khi stream bị huỷ giữa chừng");
        }
        finally
        {
            await executor.StopAsync(CancellationToken.None);
        }

        savedOutputOnCancel.Should().Be(
            "Đây là phần đã sinh ra trước khi bị huỷ.",
            "phần AI đã sinh ra trước khi bị huỷ phải được lưu lại, không phải chuỗi rỗng");

        writer.Verify(x => x.CompleteAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }
}
