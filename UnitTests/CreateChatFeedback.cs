using Application.Features.ManagerChat.Commands.CreateChatFeedback;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Services;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class CreateChatFeedback
{
    private readonly Mock<IChatReadRepository> _chatReadRepositoryMock = new();
    private readonly Mock<IChatInsertRepository> _chatInsertRepositoryMock = new();
    private readonly Mock<ICurrentUserContext> _currentUserContextMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact(DisplayName = "CHATFEEDBACK_01 - Unit - Tạo feedback thành công khi run thuộc về người dùng")]
    public async Task Create_RunBelongsToUser_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        _currentUserContextMock.Setup(c => c.GetUserId()).Returns(userId);
        _chatReadRepositoryMock.Setup(r => r.GetRunByIdAsync(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatRun { Id = runId, Session = new ChatSession { UserId = userId } });
        var handler = new CreateChatFeedbackCommandHandler(
            _chatReadRepositoryMock.Object,
            _chatInsertRepositoryMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var result = await handler.Handle(new CreateChatFeedbackCommand(runId, "Số liệu sai"), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        _chatInsertRepositoryMock.Verify(
            r => r.AddFeedback(It.Is<ChatFeedback>(f => f.ChatRunId == runId && f.ReportedBy == userId)),
            Times.Once);
    }

    [Fact(DisplayName = "CHATFEEDBACK_02 - Unit - Từ chối khi run không thuộc về người dùng")]
    public async Task Create_RunBelongsToAnotherUser_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        _currentUserContextMock.Setup(c => c.GetUserId()).Returns(userId);
        _chatReadRepositoryMock.Setup(r => r.GetRunByIdAsync(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatRun { Id = runId, Session = new ChatSession { UserId = Guid.NewGuid() } });
        var handler = new CreateChatFeedbackCommandHandler(
            _chatReadRepositoryMock.Object,
            _chatInsertRepositoryMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var result = await handler.Handle(new CreateChatFeedbackCommand(runId, null), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        _chatInsertRepositoryMock.Verify(r => r.AddFeedback(It.IsAny<ChatFeedback>()), Times.Never);
    }
}
