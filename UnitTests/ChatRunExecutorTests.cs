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
        writer.Setup(x => x.CancelAsync(runId, It.IsAny<string>(), It.IsAny<DateTime>()))
              .Callback<Guid, string, DateTime>((_, finalOutput, _) =>
              {
                  savedOutputOnCancel = finalOutput;
                  cancelled.TrySetResult();
              })
              .Returns(Task.CompletedTask);

        var tokenManager = new Mock<ITokenManagerService>();
        var configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton(readRepo.Object);
        services.AddSingleton(streamClient.Object);
        services.AddSingleton(tokenStore.Object);
        services.AddSingleton(writer.Object);
        services.AddSingleton(tokenManager.Object);
        services.AddSingleton(configuration.Object);
        var provider = services.BuildServiceProvider();

        var queue = new ChatRunQueue();
        var cancellationRegistry = new Mock<IChatRunCancellationRegistry>();
        var executor = new ChatRunExecutor(queue, provider, cancellationRegistry.Object, NullLogger<ChatRunExecutor>.Instance);

        await executor.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            await queue.EnqueueAsync(runId, TestContext.Current.CancellationToken);

            var finished = await Task.WhenAny(cancelled.Task, Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
            finished.Should().Be(cancelled.Task, "CancelAsync phải được gọi trong vòng 5s sau khi stream bị huỷ giữa chừng");
        }
        finally
        {
            await executor.StopAsync(TestContext.Current.CancellationToken);
        }

        savedOutputOnCancel.Should().Be(
            "Đây là phần đã sinh ra trước khi bị huỷ.",
            "phần AI đã sinh ra trước khi bị huỷ phải được lưu lại, không phải chuỗi rỗng");

        writer.Verify(x => x.CompleteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    private static async IAsyncEnumerable<SidecarEvent> StreamWithTurnBoundary()
    {
        yield return new SidecarEvent(ChatRunEventType.TextDelta, "Đoạn 1 phần a. ");
        yield return new SidecarEvent(ChatRunEventType.TextDelta, "Đoạn 1 phần b.");
        await Task.Yield();
        yield return new SidecarEvent(ChatRunEventType.TurnBoundary, "");
        yield return new SidecarEvent(ChatRunEventType.TextDelta, "Đoạn 2 sau khi hấp thụ steering.");
        yield return new SidecarEvent("done", "");
    }

    [Fact(DisplayName = "EXEC_02 - Gặp turn_boundary thì chốt đoạn hiện tại thành 1 message riêng, đoạn sau tính lại từ đầu")]
    public async Task ProcessRun_SplitsSegments_OnTurnBoundary()
    {
        var runId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var run = new ChatRun { Id = runId, SessionId = sessionId, UserMessage = "hỏi gì đó", Status = ChatRunStatus.Pending };

        var readRepo = new Mock<IChatReadRepository>();
        readRepo.Setup(x => x.GetRunByIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(run);

        var streamClient = new Mock<ISidecarStreamClient>();
        streamClient
            .Setup(x => x.StreamAsync(runId, sessionId, run.UserMessage, "user-token", It.IsAny<CancellationToken>()))
            .Returns(StreamWithTurnBoundary());

        var tokenStore = new Mock<IChatRunTokenStore>();
        tokenStore.Setup(x => x.Take(runId)).Returns("user-token");

        var writer = new Mock<IChatRunWriter>();
        writer.Setup(x => x.AppendAsync(runId, It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(0L);

        string? firstSegment = null;
        writer.Setup(x => x.AppendSegmentAsync(runId, It.IsAny<string>(), It.IsAny<DateTime>()))
              .Callback<Guid, string, DateTime>((_, segment, _) => firstSegment = segment)
              .ReturnsAsync(0L);

        string? finalSegment = null;
        var completed = new TaskCompletionSource();
        writer.Setup(x => x.CompleteAsync(runId, It.IsAny<string>(), It.IsAny<DateTime>()))
              .Callback<Guid, string, DateTime>((_, output, _) =>
              {
                  finalSegment = output;
                  completed.TrySetResult();
              })
              .Returns(Task.CompletedTask);

        var tokenManager = new Mock<ITokenManagerService>();
        var configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton(readRepo.Object);
        services.AddSingleton(streamClient.Object);
        services.AddSingleton(tokenStore.Object);
        services.AddSingleton(writer.Object);
        services.AddSingleton(tokenManager.Object);
        services.AddSingleton(configuration.Object);
        var provider = services.BuildServiceProvider();

        var queue = new ChatRunQueue();
        var cancellationRegistry = new Mock<IChatRunCancellationRegistry>();
        var executor = new ChatRunExecutor(queue, provider, cancellationRegistry.Object, NullLogger<ChatRunExecutor>.Instance);

        await executor.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            await queue.EnqueueAsync(runId, TestContext.Current.CancellationToken);

            var finished = await Task.WhenAny(completed.Task, Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
            finished.Should().Be(completed.Task, "CompleteAsync phải được gọi sau khi stream kết thúc");
        }
        finally
        {
            await executor.StopAsync(TestContext.Current.CancellationToken);
        }

        firstSegment.Should().Be("Đoạn 1 phần a. Đoạn 1 phần b.", "đoạn trước ranh giới phải được chốt thành message riêng");
        finalSegment.Should().Be("Đoạn 2 sau khi hấp thụ steering.", "đoạn sau ranh giới không được lẫn nội dung đoạn trước");
    }

    private static async IAsyncEnumerable<SidecarEvent> StreamWithMessageCorrection()
    {
        yield return new SidecarEvent(ChatRunEventType.TextDelta, "Tôi sẽ kiểm tra ");
        yield return new SidecarEvent(ChatRunEventType.TextDelta, "doanh thu cho bạn.");
        await Task.Yield();
        yield return new SidecarEvent(ChatRunEventType.MessageCorrection,
            "Tôi không có đủ quyền hoặc công cụ để tra dữ liệu này.");
        yield return new SidecarEvent("done", "");
    }

    [Fact(DisplayName = "EXEC_03 - message_correction thay TOÀN BỘ nội dung đã stream, không phải append")]
    public async Task ProcessRun_ReplacesOutput_OnMessageCorrection()
    {
        var runId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var run = new ChatRun { Id = runId, SessionId = sessionId, UserMessage = "hỏi gì đó", Status = ChatRunStatus.Pending };

        var readRepo = new Mock<IChatReadRepository>();
        readRepo.Setup(x => x.GetRunByIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(run);

        var streamClient = new Mock<ISidecarStreamClient>();
        streamClient
            .Setup(x => x.StreamAsync(runId, sessionId, run.UserMessage, "user-token", It.IsAny<CancellationToken>()))
            .Returns(StreamWithMessageCorrection());

        var tokenStore = new Mock<IChatRunTokenStore>();
        tokenStore.Setup(x => x.Take(runId)).Returns("user-token");

        var writer = new Mock<IChatRunWriter>();
        writer.Setup(x => x.AppendAsync(runId, It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(0L);

        var partialOutputs = new List<string>();
        writer.Setup(x => x.FlushPartialOutputAsync(runId, It.IsAny<string>()))
              .Callback<Guid, string>((_, partial) => partialOutputs.Add(partial))
              .Returns(Task.CompletedTask);

        string? finalOutput = null;
        var completed = new TaskCompletionSource();
        writer.Setup(x => x.CompleteAsync(runId, It.IsAny<string>(), It.IsAny<DateTime>()))
              .Callback<Guid, string, DateTime>((_, output, _) =>
              {
                  finalOutput = output;
                  completed.TrySetResult();
              })
              .Returns(Task.CompletedTask);

        var tokenManager = new Mock<ITokenManagerService>();
        var configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton(readRepo.Object);
        services.AddSingleton(streamClient.Object);
        services.AddSingleton(tokenStore.Object);
        services.AddSingleton(writer.Object);
        services.AddSingleton(tokenManager.Object);
        services.AddSingleton(configuration.Object);
        var provider = services.BuildServiceProvider();

        var queue = new ChatRunQueue();
        var cancellationRegistry = new Mock<IChatRunCancellationRegistry>();
        var executor = new ChatRunExecutor(queue, provider, cancellationRegistry.Object, NullLogger<ChatRunExecutor>.Instance);

        await executor.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            await queue.EnqueueAsync(runId, TestContext.Current.CancellationToken);

            var finished = await Task.WhenAny(completed.Task, Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
            finished.Should().Be(completed.Task, "CompleteAsync phải được gọi sau khi stream kết thúc");
        }
        finally
        {
            await executor.StopAsync(TestContext.Current.CancellationToken);
        }

        finalOutput.Should().Be(
            "Tôi không có đủ quyền hoặc công cụ để tra dữ liệu này.",
            "message_correction phải THAY nội dung đã stream trước đó, không phải nối thêm vào");
        partialOutputs.Should().Contain("Tôi không có đủ quyền hoặc công cụ để tra dữ liệu này.",
            "bảng khôi phục partial output cũng phải phản ánh bản đã sửa, để reload giữa chừng không thấy câu sai");

        writer.Verify(x => x.AppendAsync(runId, ChatRunEventType.MessageCorrection,
            "Tôi không có đủ quyền hoặc công cụ để tra dữ liệu này."), Times.Once,
            "FE cần nhận được event message_correction để thay nội dung đã hiện");
    }

    private static async IAsyncEnumerable<SidecarEvent> StreamWithManySmallDeltas()
    {
        foreach (var word in new[] { "Xin ", "chào, ", "tôi ", "có ", "thể ", "giúp ", "gì?" })
        {
            yield return new SidecarEvent(ChatRunEventType.TextDelta, word);
            await Task.Yield();
        }
        yield return new SidecarEvent("done", "");
    }

    [Fact(DisplayName = "EXEC_04 - Không batching: mỗi text_delta forward ngay, không gom lại")]
    public async Task ProcessRun_ForwardsEachTextDelta_WithoutBatching()
    {
        var runId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var run = new ChatRun { Id = runId, SessionId = sessionId, UserMessage = "hỏi gì đó", Status = ChatRunStatus.Pending };

        var readRepo = new Mock<IChatReadRepository>();
        readRepo.Setup(x => x.GetRunByIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(run);

        var streamClient = new Mock<ISidecarStreamClient>();
        streamClient
            .Setup(x => x.StreamAsync(runId, sessionId, run.UserMessage, "user-token", It.IsAny<CancellationToken>()))
            .Returns(StreamWithManySmallDeltas());

        var tokenStore = new Mock<IChatRunTokenStore>();
        tokenStore.Setup(x => x.Take(runId)).Returns("user-token");

        var writer = new Mock<IChatRunWriter>();
        var forwardedDeltas = new List<string>();
        writer.Setup(x => x.AppendAsync(runId, It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(0L);
        writer.Setup(x => x.AppendAsync(runId, ChatRunEventType.TextDelta, It.IsAny<object>()))
              .Callback<Guid, string, object>((_, _, payload) => forwardedDeltas.Add((string)payload))
              .ReturnsAsync(0L);

        var completed = new TaskCompletionSource();
        writer.Setup(x => x.CompleteAsync(runId, It.IsAny<string>(), It.IsAny<DateTime>()))
              .Callback(() => completed.TrySetResult())
              .Returns(Task.CompletedTask);

        var tokenManager = new Mock<ITokenManagerService>();
        var configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton(readRepo.Object);
        services.AddSingleton(streamClient.Object);
        services.AddSingleton(tokenStore.Object);
        services.AddSingleton(writer.Object);
        services.AddSingleton(tokenManager.Object);
        services.AddSingleton(configuration.Object);
        var provider = services.BuildServiceProvider();

        var queue = new ChatRunQueue();
        var cancellationRegistry = new Mock<IChatRunCancellationRegistry>();
        var executor = new ChatRunExecutor(queue, provider, cancellationRegistry.Object, NullLogger<ChatRunExecutor>.Instance);

        await executor.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            await queue.EnqueueAsync(runId, TestContext.Current.CancellationToken);

            var finished = await Task.WhenAny(completed.Task, Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
            finished.Should().Be(completed.Task, "CompleteAsync phải được gọi sau khi stream kết thúc");
        }
        finally
        {
            await executor.StopAsync(TestContext.Current.CancellationToken);
        }

        forwardedDeltas.Should().Equal(
            new[] { "Xin ", "chào, ", "tôi ", "có ", "thể ", "giúp ", "gì?" },
            "mỗi chunk model sinh ra phải forward riêng, không được gom nhiều chunk thành 1 lần " +
            "flush — đó là điều làm FE thấy chữ nhảy cục thay vì stream tự nhiên theo tốc độ model");
    }

    private static async IAsyncEnumerable<SidecarEvent> StreamWithThinkingEvent()
    {
        yield return new SidecarEvent(ChatRunEventType.Thinking, "{\"text\":\"Cần tra doanh thu.\"}");
        yield return new SidecarEvent(ChatRunEventType.TextDelta, "Doanh thu tháng này là 0.");
        await Task.Yield();
        yield return new SidecarEvent("done", "");
    }

    [Fact(DisplayName = "EXEC_05 - thinking được ghi lại nhưng không lẫn vào fullOutput/ChatMessage")]
    public async Task ProcessRun_PersistsThinking_WithoutTouchingFullOutput()
    {
        var runId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var run = new ChatRun { Id = runId, SessionId = sessionId, UserMessage = "hỏi gì đó", Status = ChatRunStatus.Pending };

        var readRepo = new Mock<IChatReadRepository>();
        readRepo.Setup(x => x.GetRunByIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(run);

        var streamClient = new Mock<ISidecarStreamClient>();
        streamClient
            .Setup(x => x.StreamAsync(runId, sessionId, run.UserMessage, "user-token", It.IsAny<CancellationToken>()))
            .Returns(StreamWithThinkingEvent());

        var tokenStore = new Mock<IChatRunTokenStore>();
        tokenStore.Setup(x => x.Take(runId)).Returns("user-token");

        var writer = new Mock<IChatRunWriter>();
        var appendedThinking = new List<string>();
        writer.Setup(x => x.AppendAsync(runId, It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(0L);
        writer.Setup(x => x.AppendAsync(runId, ChatRunEventType.Thinking, It.IsAny<object>()))
              .Callback<Guid, string, object>((_, _, payload) => appendedThinking.Add((string)payload))
              .ReturnsAsync(0L);

        string? finalOutput = null;
        var completed = new TaskCompletionSource();
        writer.Setup(x => x.CompleteAsync(runId, It.IsAny<string>(), It.IsAny<DateTime>()))
              .Callback<Guid, string, DateTime>((_, output, _) =>
              {
                  finalOutput = output;
                  completed.TrySetResult();
              })
              .Returns(Task.CompletedTask);

        var tokenManager = new Mock<ITokenManagerService>();
        var configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton(readRepo.Object);
        services.AddSingleton(streamClient.Object);
        services.AddSingleton(tokenStore.Object);
        services.AddSingleton(writer.Object);
        services.AddSingleton(tokenManager.Object);
        services.AddSingleton(configuration.Object);
        var provider = services.BuildServiceProvider();

        var queue = new ChatRunQueue();
        var cancellationRegistry = new Mock<IChatRunCancellationRegistry>();
        var executor = new ChatRunExecutor(queue, provider, cancellationRegistry.Object, NullLogger<ChatRunExecutor>.Instance);

        await executor.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            await queue.EnqueueAsync(runId, TestContext.Current.CancellationToken);

            var finished = await Task.WhenAny(completed.Task, Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
            finished.Should().Be(completed.Task, "CompleteAsync phải được gọi sau khi stream kết thúc");
        }
        finally
        {
            await executor.StopAsync(TestContext.Current.CancellationToken);
        }

        appendedThinking.Should().Equal(new[] { "{\"text\":\"Cần tra doanh thu.\"}" },
            "event thinking phải được ghi lại làm ChatRunEvent riêng để replay được");
        finalOutput.Should().Be("Doanh thu tháng này là 0.",
            "nội dung thinking tuyệt đối không được lẫn vào fullOutput/ChatMessage cuối cùng");
    }
}
