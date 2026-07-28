using System.Net;
using System.Text;
using Application.Features.ManagerChat.Commands.StreamManagerChatMessage;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace UnitTests;

public class ManagerChatStream
{
    private const string JwtKey = "test-jwt-key-0123456789";

    private readonly Mock<IChatReadRepository> _chatRead = new();
    private readonly Mock<IChatInsertRepository> _chatInsert = new();
    private readonly Mock<IPermissionReadRepository> _permissions = new();
    private readonly Mock<IAiSidecarUrlProvider> _sidecarUrl = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly List<ChatMessage> _savedMessages = [];

    private readonly Dictionary<string, List<string>> _capturedHeaders = [];

    private StreamManagerChatMessageCommandHandler CreateHandler(string sidecarBody = "Xin chào")
    {
        _chatInsert.Setup(x => x.AddMessage(It.IsAny<ChatMessage>()))
                   .Callback<ChatMessage>(_savedMessages.Add);

        _sidecarUrl.Setup(x => x.GetSidecarUrl()).Returns("http://127.0.0.1:8000");

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                foreach (var header in req.Headers)
                    _capturedHeaders[header.Key] = header.Value.ToList();
            })
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(sidecarBody, Encoding.UTF8, "text/plain"),
            });

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient(It.IsAny<string>()))
               .Returns(new HttpClient(handlerMock.Object));

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:Key"] = JwtKey })
            .Build();

        return new StreamManagerChatMessageCommandHandler(
            _chatRead.Object, _chatInsert.Object, _permissions.Object,
            _sidecarUrl.Object, _unitOfWork.Object, factory.Object, config);
    }

    private void GivenSessionOwnedBy(Guid userId, Guid sessionId)
    {
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
        _chatRead.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatSession { Id = sessionId, UserId = userId });
    }

    private static async Task<List<string>> Drain(IAsyncEnumerable<string> stream)
    {
        var chunks = new List<string>();
        await foreach (var c in stream) chunks.Add(c);
        return chunks;
    }

    [Fact(DisplayName = "STREAM_01 - Không có quyền thì ném UnauthorizedAccessException")]
    public async Task Handle_ThrowsUnauthorized_WhenNoPermission()
    {
        var userId = Guid.NewGuid();
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

        var handler = CreateHandler();
        var command = new StreamManagerChatMessageCommand(
            Guid.NewGuid(), "xin chào", userId, "token");

        var act = async () => await Drain(handler.Handle(command, CancellationToken.None));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _savedMessages.Should().BeEmpty("chưa có quyền thì không được lưu gì");
    }

    [Fact(DisplayName = "STREAM_02 - Session của người khác thì bị từ chối")]
    public async Task Handle_Throws_WhenSessionBelongsToAnotherUser()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
        _chatRead.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatSession { Id = sessionId, UserId = Guid.NewGuid() });

        var handler = CreateHandler();
        var command = new StreamManagerChatMessageCommand(sessionId, "xin chào", userId, "token");

        var act = async () => await Drain(handler.Handle(command, CancellationToken.None));

        await act.Should().ThrowAsync<InvalidOperationException>();
        _savedMessages.Should().BeEmpty("không sở hữu session thì không được lưu gì");
    }

    [Fact(DisplayName = "STREAM_03 - Lưu tin nhắn với ChatRole, không dùng magic string")]
    public async Task Handle_UsesChatRoleConstants()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        GivenSessionOwnedBy(userId, sessionId);

        var handler = CreateHandler("Doanh thu tháng 7 là 1,2 tỷ.");
        var command = new StreamManagerChatMessageCommand(
            sessionId, "doanh thu tháng này?", userId, "token");

        await Drain(handler.Handle(command, CancellationToken.None));

        _savedMessages.Should().HaveCount(2);
        _savedMessages[0].Role.Should().Be(ChatRole.User);
        _savedMessages[0].Message.Should().Be("doanh thu tháng này?");
        _savedMessages[1].Role.Should().Be(ChatRole.Ai);
        _savedMessages[1].Message.Should().Be("Doanh thu tháng 7 là 1,2 tỷ.");
    }

    [Fact(DisplayName = "STREAM_04 - Gửi kèm X-Internal-Secret và Authorization riêng biệt")]
    public async Task Handle_SendsInternalSecretHeader()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        GivenSessionOwnedBy(userId, sessionId);

        var handler = CreateHandler();
        var command = new StreamManagerChatMessageCommand(
            sessionId, "xin chào", userId, "user-jwt-token");

        await Drain(handler.Handle(command, CancellationToken.None));

        _capturedHeaders.Should().ContainKey("X-Internal-Secret");
        _capturedHeaders["X-Internal-Secret"].Should().ContainSingle().Which.Should().Be(JwtKey);

        _capturedHeaders.Should().ContainKey("Authorization",
            "secret nội bộ không được đè token của user");
        _capturedHeaders["Authorization"].Should().ContainSingle()
            .Which.Should().Be("Bearer user-jwt-token");
    }

    [Fact(DisplayName = "STREAM_05 - Stream trả về đủ nội dung theo từng chunk")]
    public async Task Handle_StreamsAllContent()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        GivenSessionOwnedBy(userId, sessionId);

        var longText = new string('a', 100);   // dài hơn buffer 32 ký tự
        var handler = CreateHandler(longText);
        var command = new StreamManagerChatMessageCommand(sessionId, "hỏi", userId, "token");

        var chunks = await Drain(handler.Handle(command, CancellationToken.None));

        chunks.Should().HaveCountGreaterThan(1, "phải chia thành nhiều chunk");
        string.Concat(chunks).Should().Be(longText);
    }
}
