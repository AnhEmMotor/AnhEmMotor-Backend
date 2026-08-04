using Application.Common.Models;
using Application.Features.NewsComments.Commands.CreateNewsComment;
using Application.Features.NewsComments.Queries.GetNewsComments;
using Asp.Versioning;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý bình luận bài viết.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý bình luận bài viết")]
[Route("api/v{version:apiVersion}/news-comments")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class NewsCommentsController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách tất cả bình luận bài viết (có phân trang).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpGet]
    [HasPermission(Permissions.Marketing.NewsManagement.View)]
    [ProducesResponseType(typeof(List<NewsCommentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNewsCommentsQuery(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách bình luận của một bài viết cụ thể theo ID bài viết.
    /// </summary>
    /// <param name="newsId">ID của bài viết.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpGet("news/{newsId:int}")]
    [HasPermission(Permissions.Marketing.NewsManagement.View)]
    [ProducesResponseType(typeof(List<NewsCommentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByNewsId(int newsId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNewsCommentsQuery(newsId), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách bình luận công khai theo loại và slug (dành cho Store).
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<NewsCommentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicComments(
        [FromQuery] string? articleType,
        [FromQuery] string? articleSlug,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNewsCommentsQuery(null, articleType, articleSlug), cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo mới một bình luận công khai (dành cho Store).
    /// </summary>
    [HttpPost("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreatePublicComment(
        [FromBody] CreateNewsCommentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
