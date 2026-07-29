using Application.Common.Models;
using Application.Features.ManagerChat.Queries.GetChatToolCatalog;
using Application.Features.ManagerChat.Queries.GetManagerChatSessions;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using System.Reflection;
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
        
        var sessions = new List<ManagerChatSessionDto> { new ManagerChatSessionDto { Id = Guid.NewGuid(), Title = "Test" } };
        mockSender.Setup(x => x.Send(It.IsAny<GetManagerChatSessionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<ManagerChatSessionDto>>.Success(sessions));

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
        var returnedSessions = Assert.IsType<List<ManagerChatSessionDto>>(okResult.Value);
        Assert.Single(returnedSessions);
    }

    [Fact(DisplayName = "MCHATC_03 - GetToolCatalog trả về danh sách label tool cho FE")]
    public async Task GetToolCatalog_ReturnsOk_WithLabels()
    {
        var mockSender = new Mock<ISender>();
        var labels = new List<ChatToolLabelDto> { new("search_products", "Tìm sản phẩm") };
        mockSender.Setup(x => x.Send(It.IsAny<GetChatToolCatalogQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<ChatToolLabelDto>>.Success(labels));

        var controller = new ManagerChatController(mockSender.Object);

        var result = await controller.GetToolCatalog(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<List<ChatToolLabelDto>>(okResult.Value);
        Assert.Single(returned);
        Assert.Equal("search_products", returned[0].Name);
    }

    [Fact(DisplayName = "MCHATC_01 - Endpoint gửi tin nhắn REST đã bị loại bỏ (Hướng A)")]
    public void Controller_KhongConRoute_GuiTinNhan()
    {
        var actions = typeof(ManagerChatController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToList();

        actions.Should().NotBeEmpty("controller phải còn các action khác");

        // Dò theo ROUTE chứ không theo tên method: thêm lại endpoint dưới tên `PostMessage`
        // vẫn phải bị bắt.
        var templates = actions
            .SelectMany(m => m.GetCustomAttributes<HttpMethodAttribute>(inherit: true))
            .Select(a => a.Template ?? string.Empty)
            .ToList();

        templates.Should().NotContain(
            t => t.EndsWith("/message", StringComparison.OrdinalIgnoreCase),
            "Stage 1.1 Hướng A đã bỏ đường REST gửi tin nhắn, chỉ dùng SignalR");

        actions.Select(m => m.Name).Should().NotContain("SendMessage");
    }

    [Fact(DisplayName = "MCHATC_02 - Controller yêu cầu xác thực")]
    public void Controller_CoAuthorizeAttribute()
    {
        typeof(ManagerChatController)
            .GetCustomAttributes<AuthorizeAttribute>(inherit: true)
            .Should().NotBeEmpty();
    }
}
