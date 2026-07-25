using Application.Common.Models;
using Application.Features.ManagerChat.Queries.GetManagerChatSessions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class ManagerChatControllerTests
{
    [Fact]
    public async Task GetSessions_ReturnsOk_WithSessions()
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var userId = Guid.NewGuid();
        
        var sessions = new List<ChatSession> { new ChatSession { Id = Guid.NewGuid(), Title = "Test" } };
        mockSender.Setup(x => x.Send(It.IsAny<GetManagerChatSessionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<ChatSession>>.Success(sessions));

        var controller = new ManagerChatController(mockSender.Object);
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        ], "mock"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await controller.GetSessions(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSessions = Assert.IsType<List<ChatSession>>(okResult.Value);
        Assert.Single(returnedSessions);
    }
}
