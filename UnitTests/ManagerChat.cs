using Application.Features.ManagerChat.Commands.CreateManagerChatSession;
using Application.Features.ManagerChat.Queries.GetManagerChatSessions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Moq;

namespace UnitTests;

public class ManagerChatTests
{
    [Fact]
    public async Task GetManagerChatSessions_ReturnsSessions_WhenHasPermission()
    {
        var mockChatRepo = new Mock<IChatReadRepository>();
        var mockPermissionRepo = new Mock<IPermissionReadRepository>();
        var mockCurrentUserContext = new Mock<ICurrentUserContext>();
        var userId = Guid.NewGuid();
        mockCurrentUserContext.Setup(x => x.GetUserId()).Returns(userId);
        mockPermissionRepo.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sessions = new List<ChatSession> { new ChatSession { Id = Guid.NewGuid(), Title = "Test" } };
        mockChatRepo.Setup(x => x.GetSessionsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);
        var handler = new GetManagerChatSessionsQueryHandler(
            mockChatRepo.Object,
            mockPermissionRepo.Object,
            mockCurrentUserContext.Object);
        var query = new GetManagerChatSessionsQuery();
        var result = await handler.Handle(query, CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("Test", result.Value[0].Title);
    }

    [Fact]
    public async Task GetManagerChatSessions_ReturnsForbidden_WhenNoPermission()
    {
        var mockChatRepo = new Mock<IChatReadRepository>();
        var mockPermissionRepo = new Mock<IPermissionReadRepository>();
        var mockCurrentUserContext = new Mock<ICurrentUserContext>();
        var userId = Guid.NewGuid();
        mockCurrentUserContext.Setup(x => x.GetUserId()).Returns(userId);
        mockPermissionRepo.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = new GetManagerChatSessionsQueryHandler(
            mockChatRepo.Object,
            mockPermissionRepo.Object,
            mockCurrentUserContext.Object);
        var query = new GetManagerChatSessionsQuery();
        var result = await handler.Handle(query, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal("Forbidden", result.Error?.Code);
    }

    [Fact]
    public async Task CreateManagerChatSession_ReturnsSession_WhenHasPermission()
    {
        var mockChatInsertRepo = new Mock<IChatInsertRepository>();
        var mockPermissionRepo = new Mock<IPermissionReadRepository>();
        var mockCurrentUserContext = new Mock<ICurrentUserContext>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockConfig = new Mock<IConfiguration>();
        var mockUrlProvider = new Mock<IAiSidecarUrlProvider>();
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var userId = Guid.NewGuid();
        mockCurrentUserContext.Setup(x => x.GetUserId()).Returns(userId);
        mockPermissionRepo.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        mockUrlProvider.Setup(x => x.GetSidecarUrl()).Returns("http://localhost:8000");
        var handler = new CreateManagerChatSessionCommandHandler(
            mockChatInsertRepo.Object,
            mockPermissionRepo.Object,
            mockCurrentUserContext.Object,
            mockConfig.Object,
            mockUrlProvider.Object,
            mockHttpClientFactory.Object,
            mockUnitOfWork.Object);
        var command = new CreateManagerChatSessionCommand("New Title");
        var result = await handler.Handle(command, CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal("New Title", result.Value.Title);
        Assert.Equal(userId, result.Value.UserId);
        mockChatInsertRepo.Verify(x => x.AddSession(It.IsAny<ChatSession>()), Times.Once);
        mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
