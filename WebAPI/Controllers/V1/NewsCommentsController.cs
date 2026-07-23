using Application.Common.Models;
using Application.Features.NewsComments.Queries.GetNewsComments;
using Asp.Versioning;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using MediatR;
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
}
