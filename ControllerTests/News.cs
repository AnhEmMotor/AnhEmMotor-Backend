using Application.Common.Models;
using Application.Features.News.Commands.UpdateNews;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class News
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly NewsController _controller;

    public News()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new NewsController(_mediatorMock.Object);
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
    }

    [Fact(DisplayName = "NEWS_014 - Cập nhật nội dung bài viết chuyên sâu")]
    public async Task UpdateNews_ValidRequest_ReturnsOk()
    {
        var authorId = Guid.NewGuid();
        var command = new UpdateNewsCommand
        {
            Id = 1,
            Title = "Updated Title",
            Content = "Deep dive content",
            CoverImageUrl = "http://image.com/img.png",
            AuthorId = authorId
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateNewsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success(Unit.Value));
        var result = await _controller.UpdateAsync(1, command, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<UpdateNewsCommand>(
                    c => c.Id == 1 && string.Compare(c.Content, "Deep dive content") == 0 && c.AuthorId == authorId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
