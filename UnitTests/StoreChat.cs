using Application.Features.StoreChat.Commands.CreateOrRestoreStoreChatSession;
using Application.Features.StoreChat.Commands.GenerateStoreChatAiReply;
using Application.Features.StoreChat.Commands.LinkStoreChatSessionToCustomer;
using Application.Features.StoreChat.Commands.SendStoreChatMessage;
using Application.Features.StoreChat.Queries.GetStoreChatHistory;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.StoreChat;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Moq;

namespace UnitTests;

public class StoreChatTests
{
    [Fact]
    public async Task CreateOrRestoreSession_NoExistingSession_CreatesNewSession()
    {
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockInsertRepo = new Mock<IStoreChatInsertRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockReadRepo.Setup(x => x.GetSessionByVisitorKeyAsync("visitor-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreChatSession?)null);

        var handler = new CreateOrRestoreStoreChatSessionCommandHandler(
            mockReadRepo.Object, mockInsertRepo.Object, mockUnitOfWork.Object);
        var result = await handler.Handle(new CreateOrRestoreStoreChatSessionCommand("visitor-1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("visitor-1", result.Value.VisitorKey);
        Assert.Equal("Ai", result.Value.Mode);
        mockInsertRepo.Verify(x => x.AddSession(It.IsAny<StoreChatSession>()), Times.Once);
        mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrRestoreSession_ExistingSession_RestoresWithoutCreating()
    {
        var existing = new StoreChatSession { VisitorKey = "visitor-2" };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockInsertRepo = new Mock<IStoreChatInsertRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockReadRepo.Setup(x => x.GetSessionByVisitorKeyAsync("visitor-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var handler = new CreateOrRestoreStoreChatSessionCommandHandler(
            mockReadRepo.Object, mockInsertRepo.Object, mockUnitOfWork.Object);
        var result = await handler.Handle(new CreateOrRestoreStoreChatSessionCommand("visitor-2"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(existing.Id, result.Value.Id);
        mockInsertRepo.Verify(x => x.AddSession(It.IsAny<StoreChatSession>()), Times.Never);
        mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrRestoreSession_EmptyVisitorKey_ReturnsValidationError()
    {
        var handler = new CreateOrRestoreStoreChatSessionCommandHandler(
            new Mock<IStoreChatReadRepository>().Object,
            new Mock<IStoreChatInsertRepository>().Object,
            new Mock<IUnitOfWork>().Object);

        var result = await handler.Handle(new CreateOrRestoreStoreChatSessionCommand(" "), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Validation", result.Error?.Code);
    }

    [Fact]
    public async Task GetHistory_SessionExists_ReturnsMessages()
    {
        var sessionId = Guid.NewGuid();
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreChatSession { Id = sessionId });
        mockReadRepo.Setup(x => x.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new StoreChatMessage { SessionId = sessionId, Sender = "Visitor", Content = "Hi" }]);

        var handler = new GetStoreChatHistoryQueryHandler(mockReadRepo.Object);
        var result = await handler.Handle(new GetStoreChatHistoryQuery(sessionId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("Hi", result.Value[0].Content);
    }

    [Fact]
    public async Task GetHistory_SessionMissing_ReturnsNotFound()
    {
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreChatSession?)null);

        var handler = new GetStoreChatHistoryQueryHandler(mockReadRepo.Object);
        var result = await handler.Handle(new GetStoreChatHistoryQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error?.Code);
    }

    [Fact]
    public async Task LinkToCustomer_UsesUserIdFromJwt_NotFromCaller()
    {
        var sessionId = Guid.NewGuid();
        var jwtUserId = Guid.NewGuid();
        var session = new StoreChatSession { Id = sessionId };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockUpdateRepo = new Mock<IStoreChatUpdateRepository>();
        var mockCurrentUserContext = new Mock<ICurrentUserContext>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        mockCurrentUserContext.Setup(x => x.GetUserId()).Returns(jwtUserId);

        var handler = new LinkStoreChatSessionToCustomerCommandHandler(
            mockReadRepo.Object, mockUpdateRepo.Object, mockCurrentUserContext.Object, mockUnitOfWork.Object);
        var result = await handler.Handle(new LinkStoreChatSessionToCustomerCommand(sessionId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(jwtUserId, session.CustomerUserId);
        mockUpdateRepo.Verify(x => x.UpdateSession(session), Times.Once);
    }

    [Fact]
    public async Task SendMessage_SessionExists_PersistsAsVisitorSender()
    {
        var sessionId = Guid.NewGuid();
        var session = new StoreChatSession { Id = sessionId };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockInsertRepo = new Mock<IStoreChatInsertRepository>();
        var mockUpdateRepo = new Mock<IStoreChatUpdateRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var handler = new SendStoreChatMessageCommandHandler(
            mockReadRepo.Object, mockInsertRepo.Object, mockUpdateRepo.Object, mockUnitOfWork.Object);
        var result = await handler.Handle(new SendStoreChatMessageCommand(sessionId, "Xin chào"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Visitor", result.Value.Sender);
        Assert.Equal("Xin chào", result.Value.Content);
        mockInsertRepo.Verify(x => x.AddMessage(It.IsAny<StoreChatMessage>()), Times.Once);
    }

    [Fact]
    public async Task SendMessage_SessionMissing_ReturnsNotFound()
    {
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreChatSession?)null);

        var handler = new SendStoreChatMessageCommandHandler(
            mockReadRepo.Object,
            new Mock<IStoreChatInsertRepository>().Object,
            new Mock<IStoreChatUpdateRepository>().Object,
            new Mock<IUnitOfWork>().Object);
        var result = await handler.Handle(new SendStoreChatMessageCommand(Guid.NewGuid(), "Hi"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error?.Code);
    }

    [Fact]
    public async Task GenerateAiReply_SessionExists_PersistsCardsJsonFromAiClient()
    {
        var sessionId = Guid.NewGuid();
        var session = new StoreChatSession { Id = sessionId };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockInsertRepo = new Mock<IStoreChatInsertRepository>();
        var mockUpdateRepo = new Mock<IStoreChatUpdateRepository>();
        var mockAiClient = new Mock<IStoreChatAiClient>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        mockReadRepo.Setup(x => x.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new StoreChatMessage { SessionId = sessionId, Sender = StoreChatSender.Visitor, Content = "Còn SH không?" }]);
        var cardsJson = "[{\"kind\":\"product-cards\",\"items\":[]}]";
        mockAiClient
            .Setup(x => x.GetReplyAsync(sessionId, "Còn SH không?", It.IsAny<IReadOnlyList<StoreChatHistoryItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreChatAiReplyResult("Dạ shop còn SH ạ", cardsJson));

        var handler = new GenerateStoreChatAiReplyCommandHandler(
            mockReadRepo.Object, mockInsertRepo.Object, mockUpdateRepo.Object, mockAiClient.Object, mockUnitOfWork.Object);
        var result = await handler.Handle(new GenerateStoreChatAiReplyCommand(sessionId, "Còn SH không?"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(StoreChatSender.Ai, result.Value.Sender);
        Assert.Equal("Dạ shop còn SH ạ", result.Value.Content);
        Assert.Equal(cardsJson, result.Value.CardsJson);
        mockInsertRepo.Verify(x => x.AddMessage(It.Is<StoreChatMessage>(m => m.Sender == StoreChatSender.Ai && m.CardsJson == cardsJson)), Times.Once);
        mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateAiReply_WithOnChunk_ForwardsSameCallbackToAiClient()
    {
        var sessionId = Guid.NewGuid();
        var session = new StoreChatSession { Id = sessionId };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockInsertRepo = new Mock<IStoreChatInsertRepository>();
        var mockUpdateRepo = new Mock<IStoreChatUpdateRepository>();
        var mockAiClient = new Mock<IStoreChatAiClient>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        mockReadRepo.Setup(x => x.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        Func<string, Task> onChunk = _ => Task.CompletedTask;
        mockAiClient
            .Setup(x => x.GetReplyAsync(sessionId, "Hi", It.IsAny<IReadOnlyList<StoreChatHistoryItem>>(), It.IsAny<CancellationToken>(), onChunk))
            .ReturnsAsync(new StoreChatAiReplyResult("Chào bạn", null));

        var handler = new GenerateStoreChatAiReplyCommandHandler(
            mockReadRepo.Object, mockInsertRepo.Object, mockUpdateRepo.Object, mockAiClient.Object, mockUnitOfWork.Object);
        var result = await handler.Handle(new GenerateStoreChatAiReplyCommand(sessionId, "Hi", onChunk), CancellationToken.None);

        Assert.True(result.IsSuccess);
        mockAiClient.Verify(
            x => x.GetReplyAsync(sessionId, "Hi", It.IsAny<IReadOnlyList<StoreChatHistoryItem>>(), It.IsAny<CancellationToken>(), onChunk),
            Times.Once);
    }

    [Fact]
    public async Task GenerateAiReply_SessionMissing_ReturnsNotFound()
    {
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreChatSession?)null);

        var handler = new GenerateStoreChatAiReplyCommandHandler(
            mockReadRepo.Object,
            new Mock<IStoreChatInsertRepository>().Object,
            new Mock<IStoreChatUpdateRepository>().Object,
            new Mock<IStoreChatAiClient>().Object,
            new Mock<IUnitOfWork>().Object);
        var result = await handler.Handle(new GenerateStoreChatAiReplyCommand(Guid.NewGuid(), "Hi"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error?.Code);
    }

    /// <summary>
    /// Khoá bảo mật: handler Store Chat không được tham chiếu entity/repository của Manager Chat
    /// (ChatSession, ChatRun, ChatPlan) — StoreChat phải hoàn toàn tách biệt như 00-OVERVIEW.md mục 1 yêu cầu.
    /// </summary>
    [Fact]
    public void StoreChatHandlers_DoNotReferenceManagerChatTypes()
    {
        var forbiddenTypeNames = new[] { "ChatSession", "ChatRun", "ChatPlan", "IChatReadRepository", "IChatInsertRepository", "IChatUpdateRepository", "IChatDeleteRepository" };
        var storeChatTypes = typeof(CreateOrRestoreStoreChatSessionCommandHandler).Assembly.GetTypes()
            .Where(t => t.Namespace != null && t.Namespace.StartsWith("Application.Features.StoreChat"));

        foreach (var type in storeChatTypes)
        {
            var referencedTypeNames = type.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType.Name)
                .Concat(type.GetMethods().SelectMany(m => m.GetParameters()).Select(p => p.ParameterType.Name));

            Assert.Empty(referencedTypeNames.Intersect(forbiddenTypeNames));
        }
    }
}
