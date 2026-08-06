using Application.Common.Models;
using Application.DTOs.StoreChat;
using Application.Features.StoreChat.Commands.CreateOrRestoreStoreChatSession;
using Application.Features.StoreChat.Commands.DeleteStoreChatSession;
using Application.Features.StoreChat.Commands.GenerateStoreChatAiReply;
using Application.Features.StoreChat.Commands.LinkStoreChatSessionToCustomer;
using Application.Features.StoreChat.Commands.ReleaseStoreChatSession;
using Application.Features.StoreChat.Commands.RequestHandoff;
using Application.Features.StoreChat.Commands.SendStoreChatMessage;
using Application.Features.StoreChat.Commands.SendStoreChatStaffMessage;
using Application.Features.StoreChat.Commands.SetStoreChatContactInfo;
using Application.Features.StoreChat.Queries.GetProductVariantsForStaff;
using Application.Features.StoreChat.Queries.GetStoreChatHistory;
using Application.Features.StoreChat.Queries.GetStoreChatSessionsForStaff;
using Application.Features.StoreChat.Queries.SearchProductsForStaff;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.StoreChat;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Microsoft.Extensions.Logging;
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
            mockReadRepo.Object,
            mockInsertRepo.Object,
            mockUnitOfWork.Object);
        var result = await handler.Handle(
            new CreateOrRestoreStoreChatSessionCommand("visitor-1"),
            CancellationToken.None);
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
            mockReadRepo.Object,
            mockInsertRepo.Object,
            mockUnitOfWork.Object);
        var result = await handler.Handle(
            new CreateOrRestoreStoreChatSessionCommand("visitor-2"),
            CancellationToken.None);
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
    public async Task CreateOrRestoreSession_HasAssignedStaff_ResolvesStaffName()
    {
        var staffId = Guid.NewGuid();
        var existing = new StoreChatSession
        {
            VisitorKey = "visitor-3",
            Mode = StoreChatMode.Human,
            AssignedStaffId = staffId
        };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        mockReadRepo.Setup(x => x.GetSessionByVisitorKeyAsync("visitor-3", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        mockReadRepo.Setup(x => x.GetStaffNameAsync(staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Nguyễn Văn B");
        var handler = new CreateOrRestoreStoreChatSessionCommandHandler(
            mockReadRepo.Object,
            new Mock<IStoreChatInsertRepository>().Object,
            new Mock<IUnitOfWork>().Object);
        var result = await handler.Handle(
            new CreateOrRestoreStoreChatSessionCommand("visitor-3"),
            CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal("Nguyễn Văn B", result.Value.AssignedStaffName);
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
    public async Task CreateOrRestoreSession_WithPreviousSessionId_LinksAndCopiesContactInfo()
    {
        var previousId = Guid.NewGuid();
        var previousSession = new StoreChatSession
        {
            Id = previousId,
            VisitorKey = "old-visitor",
            ContactName = "Nguyễn Văn A",
            ContactPhone = "0901234567"
        };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockInsertRepo = new Mock<IStoreChatInsertRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockReadRepo.Setup(x => x.GetSessionByVisitorKeyAsync("new-visitor", It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreChatSession?)null);
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(previousId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(previousSession);
        var handler = new CreateOrRestoreStoreChatSessionCommandHandler(
            mockReadRepo.Object,
            mockInsertRepo.Object,
            mockUnitOfWork.Object);
        var result = await handler.Handle(
            new CreateOrRestoreStoreChatSessionCommand("new-visitor", previousId),
            CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal("Nguyễn Văn A", result.Value.ContactName);
        Assert.Equal("0901234567", result.Value.ContactPhone);
        mockInsertRepo.Verify(
            x => x.AddSession(It.Is<StoreChatSession>(s => s.PreviousSessionId == previousId)),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrRestoreSession_PreviousSessionIdNotFound_StillCreatesSessionWithoutLink()
    {
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockInsertRepo = new Mock<IStoreChatInsertRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockReadRepo.Setup(x => x.GetSessionByVisitorKeyAsync("new-visitor-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreChatSession?)null);
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreChatSession?)null);
        var handler = new CreateOrRestoreStoreChatSessionCommandHandler(
            mockReadRepo.Object,
            mockInsertRepo.Object,
            mockUnitOfWork.Object);
        var result = await handler.Handle(
            new CreateOrRestoreStoreChatSessionCommand("new-visitor-2", Guid.NewGuid()),
            CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.ContactName);
    }

    [Fact]
    public async Task CreateOrRestoreSession_VisitorKeyBelongsToSoftDeletedSession_ReleasesKeyBeforeCreating()
    {
        var deletedSession = new StoreChatSession { VisitorKey = "visitor-4", DeletedAt = DateTimeOffset.UtcNow };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockInsertRepo = new Mock<IStoreChatInsertRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockReadRepo.Setup(x => x.GetSessionByVisitorKeyAsync("visitor-4", It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreChatSession?)null);
        mockReadRepo.Setup(x => x.GetDeletedSessionByVisitorKeyAsync("visitor-4", It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedSession);
        var handler = new CreateOrRestoreStoreChatSessionCommandHandler(
            mockReadRepo.Object,
            mockInsertRepo.Object,
            mockUnitOfWork.Object);
        var result = await handler.Handle(
            new CreateOrRestoreStoreChatSessionCommand("visitor-4"),
            CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal("visitor-4", result.Value.VisitorKey);
        Assert.NotEqual("visitor-4", deletedSession.VisitorKey);
        Assert.True(deletedSession.VisitorKey.Length <= 64, "VisitorKey column is nvarchar(64)");
        mockInsertRepo.Verify(x => x.AddSession(It.Is<StoreChatSession>(s => s.VisitorKey == "visitor-4")), Times.Once);
    }

    [Fact]
    public async Task SetContactInfo_SessionExists_UpdatesNameAndPhone()
    {
        var sessionId = Guid.NewGuid();
        var session = new StoreChatSession { Id = sessionId };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockUpdateRepo = new Mock<IStoreChatUpdateRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var handler = new SetStoreChatContactInfoCommandHandler(
            mockReadRepo.Object,
            mockUpdateRepo.Object,
            mockUnitOfWork.Object);
        var result = await handler.Handle(
            new SetStoreChatContactInfoCommand(sessionId, "Nguyễn Văn A", "0901234567"),
            CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal("Nguyễn Văn A", session.ContactName);
        Assert.Equal("0901234567", session.ContactPhone);
        mockUpdateRepo.Verify(x => x.UpdateSession(session), Times.Once);
    }

    [Fact]
    public async Task SetContactInfo_SessionMissing_ReturnsNotFound()
    {
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreChatSession?)null);
        var handler = new SetStoreChatContactInfoCommandHandler(
            mockReadRepo.Object,
            new Mock<IStoreChatUpdateRepository>().Object,
            new Mock<IUnitOfWork>().Object);
        var result = await handler.Handle(
            new SetStoreChatContactInfoCommand(Guid.NewGuid(), "A", "0901234567"),
            CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error?.Code);
    }

    [Fact]
    public async Task DeleteSession_SessionExists_DeletesAndSaves()
    {
        var sessionId = Guid.NewGuid();
        var session = new StoreChatSession { Id = sessionId };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockDeleteRepo = new Mock<IStoreChatDeleteRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var handler = new DeleteStoreChatSessionCommandHandler(
            mockReadRepo.Object,
            mockDeleteRepo.Object,
            mockUnitOfWork.Object);
        var result = await handler.Handle(new DeleteStoreChatSessionCommand(sessionId), CancellationToken.None);
        Assert.True(result.IsSuccess);
        mockDeleteRepo.Verify(x => x.DeleteSessionAsync(session, It.IsAny<CancellationToken>()), Times.Once);
        mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSession_SessionMissing_ReturnsNotFound()
    {
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreChatSession?)null);
        var handler = new DeleteStoreChatSessionCommandHandler(
            mockReadRepo.Object,
            new Mock<IStoreChatDeleteRepository>().Object,
            new Mock<IUnitOfWork>().Object);
        var result = await handler.Handle(new DeleteStoreChatSessionCommand(Guid.NewGuid()), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error?.Code);
    }

    [Theory]
    [InlineData("", "0901234567", false)]
    [InlineData("Nguyễn Văn A", "", false)]
    [InlineData("Nguyễn Văn A", "091234", false)]
    [InlineData("Nguyễn Văn A", "0901234567", true)]
    public void SetContactInfoValidator_ValidatesNameAndPhone(string name, string phone, bool expectedValid)
    {
        var validator = new SetStoreChatContactInfoCommandValidator();
        var result = validator.Validate(new SetStoreChatContactInfoCommand(Guid.NewGuid(), name, phone));
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("Xin chào", true)]
    public void SendMessageValidator_TuChoiTinRong(string content, bool expectedValid)
    {
        var validator = new SendStoreChatMessageCommandValidator();
        var result = validator.Validate(new SendStoreChatMessageCommand(Guid.NewGuid(), content));
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Fact]
    public void SendMessageValidator_TuChoiTinQua2000KyTu()
    {
        var validator = new SendStoreChatMessageCommandValidator();
        var result = validator.Validate(new SendStoreChatMessageCommand(Guid.NewGuid(), new string('a', 2001)));
        Assert.False(result.IsValid);
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
            mockReadRepo.Object,
            mockUpdateRepo.Object,
            mockCurrentUserContext.Object,
            mockUnitOfWork.Object);
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
            mockReadRepo.Object,
            mockInsertRepo.Object,
            mockUpdateRepo.Object,
            mockUnitOfWork.Object);
        var result = await handler.Handle(
            new SendStoreChatMessageCommand(sessionId, "Xin chào"),
            CancellationToken.None);
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
            .ReturnsAsync(
                [new StoreChatMessage
                {
                    SessionId = sessionId,
                    Sender = StoreChatSender.Visitor,
                    Content = "Còn SH không?"
                }]);
        var cardsJson = "[{\"kind\":\"product-cards\",\"items\":[]}]";
        mockAiClient
            .Setup(
                x => x.GetReplyAsync(
                    sessionId,
                    "Còn SH không?",
                    It.IsAny<IReadOnlyList<StoreChatHistoryItem>>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreChatAiReplyResult("Dạ shop còn SH ạ", cardsJson));
        var handler = new GenerateStoreChatAiReplyCommandHandler(
            mockReadRepo.Object,
            mockInsertRepo.Object,
            mockUpdateRepo.Object,
            mockAiClient.Object,
            mockUnitOfWork.Object);
        var result = await handler.Handle(
            new GenerateStoreChatAiReplyCommand(sessionId, "Còn SH không?"),
            CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal(StoreChatSender.Ai, result.Value!.Sender);
        Assert.Equal("Dạ shop còn SH ạ", result.Value.Content);
        Assert.Equal(cardsJson, result.Value.CardsJson);
        mockInsertRepo.Verify(
            x => x.AddMessage(It.Is<StoreChatMessage>(m => m.Sender == StoreChatSender.Ai && m.CardsJson == cardsJson)),
            Times.Once);
        mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockUpdateRepo.Verify(x => x.UpdateSession(It.IsAny<StoreChatSession>()), Times.Never);
        mockUpdateRepo.Verify(
            x => x.TouchLastMessageAtAsync(sessionId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);
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
        mockReadRepo.Setup(x => x.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        Func<string, Task> onChunk = _ => Task.CompletedTask;
        mockAiClient
            .Setup(
                x => x.GetReplyAsync(
                    sessionId,
                    "Hi",
                    It.IsAny<IReadOnlyList<StoreChatHistoryItem>>(),
                    It.IsAny<CancellationToken>(),
                    onChunk))
            .ReturnsAsync(new StoreChatAiReplyResult("Chào bạn", null));
        var handler = new GenerateStoreChatAiReplyCommandHandler(
            mockReadRepo.Object,
            mockInsertRepo.Object,
            mockUpdateRepo.Object,
            mockAiClient.Object,
            mockUnitOfWork.Object);
        var result = await handler.Handle(
            new GenerateStoreChatAiReplyCommand(sessionId, "Hi", onChunk),
            CancellationToken.None);
        Assert.True(result.IsSuccess);
        mockAiClient.Verify(
            x => x.GetReplyAsync(
                sessionId,
                "Hi",
                It.IsAny<IReadOnlyList<StoreChatHistoryItem>>(),
                It.IsAny<CancellationToken>(),
                onChunk),
            Times.Once);
    }

    [Fact]
    public async Task GenerateAiReply_EmptyTextAndNoCards_ReturnsNullWithoutPersistingMessage()
    {
        var sessionId = Guid.NewGuid();
        var session = new StoreChatSession { Id = sessionId };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockInsertRepo = new Mock<IStoreChatInsertRepository>();
        var mockUpdateRepo = new Mock<IStoreChatUpdateRepository>();
        var mockAiClient = new Mock<IStoreChatAiClient>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        mockReadRepo.Setup(x => x.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        mockAiClient
            .Setup(
                x => x.GetReplyAsync(
                    sessionId,
                    "Cho tôi gặp nhân viên",
                    It.IsAny<IReadOnlyList<StoreChatHistoryItem>>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreChatAiReplyResult(string.Empty, null));
        var handler = new GenerateStoreChatAiReplyCommandHandler(
            mockReadRepo.Object,
            mockInsertRepo.Object,
            mockUpdateRepo.Object,
            mockAiClient.Object,
            mockUnitOfWork.Object);
        var result = await handler.Handle(
            new GenerateStoreChatAiReplyCommand(sessionId, "Cho tôi gặp nhân viên"),
            CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        mockInsertRepo.Verify(x => x.AddMessage(It.IsAny<StoreChatMessage>()), Times.Never);
        mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
        var result = await handler.Handle(
            new GenerateStoreChatAiReplyCommand(Guid.NewGuid(), "Hi"),
            CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error?.Code);
    }

    [Fact]
    public async Task RequestHandoff_FromAi_TransitionsToWaiting()
    {
        var sessionId = Guid.NewGuid();
        var session = new StoreChatSession { Id = sessionId, Mode = StoreChatMode.Ai };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockUpdateRepo = new Mock<IStoreChatUpdateRepository>();
        var mockInsertRepo = new Mock<IStoreChatInsertRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var handler = new RequestHandoffCommandHandler(
            mockReadRepo.Object,
            mockUpdateRepo.Object,
            mockInsertRepo.Object,
            mockUnitOfWork.Object,
            new Mock<ILogger<RequestHandoffCommandHandler>>().Object);
        var result = await handler.Handle(
            new RequestHandoffCommand(sessionId, "Nguyễn Văn A", "0901234567", "Customer"),
            CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal(StoreChatMode.Waiting, session.Mode);
        Assert.Equal("Nguyễn Văn A", session.ContactName);
        Assert.Equal("0901234567", session.ContactPhone);
        mockUpdateRepo.Verify(x => x.UpdateSession(session), Times.Once);
        Assert.Null(result.Value.SystemMessage);
        mockInsertRepo.Verify(x => x.AddMessage(It.IsAny<StoreChatMessage>()), Times.Never);
    }

    [Fact]
    public async Task RequestHandoff_TriggeredByAi_InsertsSystemMessageAndReturnsIt()
    {
        var sessionId = Guid.NewGuid();
        var session = new StoreChatSession { Id = sessionId, Mode = StoreChatMode.Ai };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockInsertRepo = new Mock<IStoreChatInsertRepository>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var handler = new RequestHandoffCommandHandler(
            mockReadRepo.Object,
            new Mock<IStoreChatUpdateRepository>().Object,
            mockInsertRepo.Object,
            new Mock<IUnitOfWork>().Object,
            new Mock<ILogger<RequestHandoffCommandHandler>>().Object);
        var result = await handler.Handle(
            new RequestHandoffCommand(sessionId, null, null, "Ai"),
            CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal(StoreChatMode.Waiting, session.Mode);
        Assert.NotNull(result.Value.SystemMessage);
        Assert.Equal(StoreChatSender.System, result.Value.SystemMessage!.Sender);
        mockInsertRepo.Verify(
            x => x.AddMessage(
                It.Is<StoreChatMessage>(m => m.Sender == StoreChatSender.System && m.SessionId == sessionId)),
            Times.Once);
    }

    [Fact]
    public async Task RequestHandoff_AlreadyHuman_DoesNotDowngradeMode()
    {
        var sessionId = Guid.NewGuid();
        var session = new StoreChatSession { Id = sessionId, Mode = StoreChatMode.Human };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var handler = new RequestHandoffCommandHandler(
            mockReadRepo.Object,
            new Mock<IStoreChatUpdateRepository>().Object,
            new Mock<IStoreChatInsertRepository>().Object,
            new Mock<IUnitOfWork>().Object,
            new Mock<ILogger<RequestHandoffCommandHandler>>().Object);
        var result = await handler.Handle(
            new RequestHandoffCommand(sessionId, null, null, "Customer"),
            CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal(StoreChatMode.Human, session.Mode);
    }

    [Fact]
    public async Task Release_TryReleaseReturnsFalse_ReturnsConflict()
    {
        var mockUpdateRepo = new Mock<IStoreChatUpdateRepository>();
        mockUpdateRepo.Setup(x => x.TryReleaseAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = new ReleaseStoreChatSessionCommandHandler(
            mockUpdateRepo.Object,
            new Mock<ILogger<ReleaseStoreChatSessionCommandHandler>>().Object);
        var result = await handler.Handle(new ReleaseStoreChatSessionCommand(Guid.NewGuid()), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal("Conflict", result.Error?.Code);
    }

    [Fact]
    public async Task Release_TryReleaseReturnsTrue_ReturnsSuccess()
    {
        var mockUpdateRepo = new Mock<IStoreChatUpdateRepository>();
        mockUpdateRepo.Setup(x => x.TryReleaseAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new ReleaseStoreChatSessionCommandHandler(
            mockUpdateRepo.Object,
            new Mock<ILogger<ReleaseStoreChatSessionCommandHandler>>().Object);
        var result = await handler.Handle(new ReleaseStoreChatSessionCommand(Guid.NewGuid()), CancellationToken.None);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetSessionsForStaff_SortsWaitingThenHumanThenAi()
    {
        var waiting = new StoreChatSessionListItemDto
        {
            Id = Guid.NewGuid(),
            Mode = StoreChatMode.Waiting,
            LastMessageAt = DateTime.UtcNow
        };
        var human = new StoreChatSessionListItemDto
        {
            Id = Guid.NewGuid(),
            Mode = StoreChatMode.Human,
            LastMessageAt = DateTime.UtcNow
        };
        var ai = new StoreChatSessionListItemDto
        {
            Id = Guid.NewGuid(),
            Mode = StoreChatMode.Ai,
            LastMessageAt = DateTime.UtcNow
        };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        mockReadRepo.Setup(x => x.GetSessionsForStaffAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([ai, human, waiting]);
        var handler = new GetStoreChatSessionsForStaffQueryHandler(mockReadRepo.Object);
        var result = await handler.Handle(new GetStoreChatSessionsForStaffQuery(), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal([waiting.Id, human.Id, ai.Id], result.Value.Select(s => s.Id));
    }

    [Theory]
    [InlineData(StoreChatMode.Ai)]
    [InlineData(StoreChatMode.Waiting)]
    public async Task StaffSendMessage_FromAiOrWaiting_AutoAssignsAndPersistsAsStaffSender(string initialMode)
    {
        var sessionId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var session = new StoreChatSession { Id = sessionId, Mode = initialMode };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockInsertRepo = new Mock<IStoreChatInsertRepository>();
        var mockUpdateRepo = new Mock<IStoreChatUpdateRepository>();
        var mockCurrentUserContext = new Mock<ICurrentUserContext>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        mockCurrentUserContext.Setup(x => x.GetUserId()).Returns(staffId);
        mockUpdateRepo.Setup(x => x.TryAssignStaffAsync(sessionId, staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mockReadRepo.Setup(x => x.GetStaffNameAsync(staffId, It.IsAny<CancellationToken>())).ReturnsAsync("Trần Thị C");
        var handler = new SendStoreChatStaffMessageCommandHandler(
            mockReadRepo.Object,
            mockInsertRepo.Object,
            mockUpdateRepo.Object,
            mockCurrentUserContext.Object,
            mockUnitOfWork.Object);
        var result = await handler.Handle(
            new SendStoreChatStaffMessageCommand(sessionId, "Dạ shop nghe ạ"),
            CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal(StoreChatSender.Staff, result.Value.Message.Sender);
        Assert.Equal("Trần Thị C", result.Value.StaffName);
        mockInsertRepo.Verify(
            x => x.AddMessage(It.Is<StoreChatMessage>(m => m.Sender == StoreChatSender.Staff)),
            Times.Once);
    }

    [Fact]
    public async Task StaffSendMessage_WithCardsJson_PersistsCardsJsonOnMessage()
    {
        var sessionId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var session = new StoreChatSession { Id = sessionId, Mode = StoreChatMode.Ai };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockInsertRepo = new Mock<IStoreChatInsertRepository>();
        var mockUpdateRepo = new Mock<IStoreChatUpdateRepository>();
        var mockCurrentUserContext = new Mock<ICurrentUserContext>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var cardsJson = "[{\"kind\":\"variant-cards\",\"items\":[{\"variantId\":456}]}]";
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        mockCurrentUserContext.Setup(x => x.GetUserId()).Returns(staffId);
        mockUpdateRepo.Setup(x => x.TryAssignStaffAsync(sessionId, staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = new SendStoreChatStaffMessageCommandHandler(
            mockReadRepo.Object,
            mockInsertRepo.Object,
            mockUpdateRepo.Object,
            mockCurrentUserContext.Object,
            mockUnitOfWork.Object);
        var result = await handler.Handle(
            new SendStoreChatStaffMessageCommand(sessionId, "Đây là mẫu xe anh hỏi ạ", cardsJson),
            CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal(cardsJson, result.Value.Message.CardsJson);
        mockInsertRepo.Verify(x => x.AddMessage(It.Is<StoreChatMessage>(m => m.CardsJson == cardsJson)), Times.Once);
    }

    [Fact]
    public async Task StaffSendMessage_EmptyContentWithCardsJson_StillSucceeds()
    {
        var sessionId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var session = new StoreChatSession { Id = sessionId, Mode = StoreChatMode.Ai };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockInsertRepo = new Mock<IStoreChatInsertRepository>();
        var mockUpdateRepo = new Mock<IStoreChatUpdateRepository>();
        var mockCurrentUserContext = new Mock<ICurrentUserContext>();
        var cardsJson = "[{\"kind\":\"variant-cards\",\"items\":[{\"variantId\":456}]}]";
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        mockCurrentUserContext.Setup(x => x.GetUserId()).Returns(staffId);
        mockUpdateRepo.Setup(x => x.TryAssignStaffAsync(sessionId, staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = new SendStoreChatStaffMessageCommandHandler(
            mockReadRepo.Object,
            mockInsertRepo.Object,
            mockUpdateRepo.Object,
            mockCurrentUserContext.Object,
            new Mock<IUnitOfWork>().Object);
        var result = await handler.Handle(
            new SendStoreChatStaffMessageCommand(sessionId, string.Empty, cardsJson),
            CancellationToken.None);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task StaffSendMessage_AlreadyMineHumanMode_PersistsAsStaffSender()
    {
        var sessionId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var session = new StoreChatSession { Id = sessionId, Mode = StoreChatMode.Human, AssignedStaffId = staffId };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockInsertRepo = new Mock<IStoreChatInsertRepository>();
        var mockUpdateRepo = new Mock<IStoreChatUpdateRepository>();
        var mockCurrentUserContext = new Mock<ICurrentUserContext>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        mockCurrentUserContext.Setup(x => x.GetUserId()).Returns(staffId);
        mockUpdateRepo.Setup(x => x.TryAssignStaffAsync(sessionId, staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = new SendStoreChatStaffMessageCommandHandler(
            mockReadRepo.Object,
            mockInsertRepo.Object,
            mockUpdateRepo.Object,
            mockCurrentUserContext.Object,
            mockUnitOfWork.Object);
        var result = await handler.Handle(
            new SendStoreChatStaffMessageCommand(sessionId, "Dạ shop nghe ạ"),
            CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal(StoreChatSender.Staff, result.Value.Message.Sender);
    }

    [Fact]
    public async Task StaffSendMessage_NotAssignedStaff_ReturnsForbidden()
    {
        var sessionId = Guid.NewGuid();
        var session = new StoreChatSession
        {
            Id = sessionId,
            Mode = StoreChatMode.Human,
            AssignedStaffId = Guid.NewGuid()
        };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockCurrentUserContext = new Mock<ICurrentUserContext>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        mockCurrentUserContext.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());
        var handler = new SendStoreChatStaffMessageCommandHandler(
            mockReadRepo.Object,
            new Mock<IStoreChatInsertRepository>().Object,
            new Mock<IStoreChatUpdateRepository>().Object,
            mockCurrentUserContext.Object,
            new Mock<IUnitOfWork>().Object);
        var result = await handler.Handle(
            new SendStoreChatStaffMessageCommand(sessionId, "Xin chào"),
            CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal("Forbidden", result.Error?.Code);
    }

    [Fact]
    public async Task StaffSendMessage_LostRaceToAnotherStaff_ReturnsConflict()
    {
        var sessionId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var session = new StoreChatSession { Id = sessionId, Mode = StoreChatMode.Waiting };
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        var mockUpdateRepo = new Mock<IStoreChatUpdateRepository>();
        var mockCurrentUserContext = new Mock<ICurrentUserContext>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        mockCurrentUserContext.Setup(x => x.GetUserId()).Returns(staffId);
        mockUpdateRepo.Setup(x => x.TryAssignStaffAsync(sessionId, staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = new SendStoreChatStaffMessageCommandHandler(
            mockReadRepo.Object,
            new Mock<IStoreChatInsertRepository>().Object,
            mockUpdateRepo.Object,
            mockCurrentUserContext.Object,
            new Mock<IUnitOfWork>().Object);
        var result = await handler.Handle(
            new SendStoreChatStaffMessageCommand(sessionId, "Xin chào"),
            CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal("Conflict", result.Error?.Code);
    }

    [Fact]
    public async Task StaffSendMessage_SessionMissing_ReturnsNotFound()
    {
        var mockReadRepo = new Mock<IStoreChatReadRepository>();
        mockReadRepo.Setup(x => x.GetSessionByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreChatSession?)null);
        var handler = new SendStoreChatStaffMessageCommandHandler(
            mockReadRepo.Object,
            new Mock<IStoreChatInsertRepository>().Object,
            new Mock<IStoreChatUpdateRepository>().Object,
            new Mock<ICurrentUserContext>().Object,
            new Mock<IUnitOfWork>().Object);
        var result = await handler.Handle(
            new SendStoreChatStaffMessageCommand(Guid.NewGuid(), "Hi"),
            CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error?.Code);
    }

    [Fact]
    public async Task SearchProductsForStaff_ReturnsMappedItems()
    {
        var product = new Domain.Entities.Product
        {
            Id = 42,
            Name = "Honda SH 2024",
            ProductVariants =
                [new ProductVariant { Id = 1, Price = 90000000, CoverImageUrl = "sh.jpg" }, new ProductVariant
                {
                    Id = 2,
                    Price = 95000000
                }]
        };
        var mockProductRepo = new Mock<IProductReadRepository>();
        mockProductRepo.Setup(
            r => r.GetPagedProductsAsync(
                "SH",
                It.IsAny<List<string>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<int>>(),
                null,
                null,
                1,
                10,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(([product], 1, new List<FilterGroup>()));
        var handler = new SearchProductsForStaffQueryHandler(mockProductRepo.Object);
        var result = await handler.Handle(new SearchProductsForStaffQuery("SH"), CancellationToken.None);
        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value);
        Assert.Equal(42, item.ProductId);
        Assert.Equal("Honda SH 2024", item.ProductName);
        Assert.Equal("sh.jpg", item.ImageUrl);
        Assert.Equal(90000000, item.PriceFrom);
        Assert.Equal(95000000, item.PriceTo);
    }

    [Fact]
    public async Task GetProductVariantsForStaff_ProductExists_ReturnsVariantsWithColors()
    {
        var product = new Domain.Entities.Product
        {
            Id = 42,
            Name = "Honda SH 2024",
            ProductVariants =
                [new ProductVariant
                {
                    Id = 456,
                    VariantName = "Đỏ đen",
                    SKU = "SH24-RB",
                    Price = 91000000,
                    UrlSlug = "sh-2024-do-den",
                    ProductVariantColors =
                        [new ProductVariantColor
                            {
                                Id = 9,
                                ColorName = "Đỏ đen",
                                ColorCode = "#c00",
                                CoverImageUrl = "red.jpg"
                            }]
                }]
        };
        var mockProductRepo = new Mock<IProductReadRepository>();
        mockProductRepo.Setup(
            r => r.GetByIdWithDetailsAsync(42, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(product);
        var handler = new GetProductVariantsForStaffQueryHandler(mockProductRepo.Object);
        var result = await handler.Handle(new GetProductVariantsForStaffQuery(42), CancellationToken.None);
        Assert.True(result.IsSuccess);
        var variant = Assert.Single(result.Value);
        Assert.Equal(456, variant.VariantId);
        Assert.Equal("Honda SH 2024", variant.ProductName);
        Assert.Equal("sh-2024-do-den", variant.Slug);
        var color = Assert.Single(variant.Colors);
        Assert.Equal("Đỏ đen", color.ColorName);
        Assert.Equal("#c00", color.ColorCode);
    }

    [Fact]
    public async Task GetProductVariantsForStaff_ProductMissing_ReturnsNotFound()
    {
        var mockProductRepo = new Mock<IProductReadRepository>();
        mockProductRepo.Setup(
            r => r.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync((Domain.Entities.Product?)null);
        var handler = new GetProductVariantsForStaffQueryHandler(mockProductRepo.Object);
        var result = await handler.Handle(new GetProductVariantsForStaffQuery(999), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error?.Code);
    }

    /// <summary>
    /// Khoá bảo mật: handler Store Chat không được tham chiếu entity/repository của Manager Chat (ChatSession, ChatRun,
    /// ChatPlan) — StoreChat phải hoàn toàn tách biệt như 00-OVERVIEW.md mục 1 yêu cầu.
    /// </summary>
    [Fact]
    public void StoreChatHandlers_DoNotReferenceManagerChatTypes()
    {
        var forbiddenTypeNames = new[]
        {
            "ChatSession",
            "ChatRun",
            "ChatPlan",
            "IChatReadRepository",
            "IChatInsertRepository",
            "IChatUpdateRepository",
            "IChatDeleteRepository"
        };
        var storeChatTypes = typeof(CreateOrRestoreStoreChatSessionCommandHandler).Assembly
            .GetTypes()
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
