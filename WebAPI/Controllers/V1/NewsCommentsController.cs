using Application.Features.NewsComments.Queries.GetNewsComments;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[SwaggerTag("Quản lý bình luận bài viết")]
[Route("api/v{version:apiVersion}/news-comments")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class NewsCommentsController(IMediator mediator) : ApiController
{
    [HttpGet]
    [HasPermission(News.View)]
    [ProducesResponseType(typeof(List<NewsCommentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNewsCommentsQuery(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpGet("news/{newsId:int}")]
    [HasPermission(News.View)]
    [ProducesResponseType(typeof(List<NewsCommentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByNewsId(int newsId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNewsCommentsQuery(newsId), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
