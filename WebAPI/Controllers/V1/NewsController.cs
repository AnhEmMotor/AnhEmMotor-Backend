using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Application.Features.News.Commands.CreateNews;
using Application.Features.News.Queries.GetNewsBySlug;
using Application.Features.News.Queries.GetNewsList;
using Asp.Versioning;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller quản lý bài viết và tin tức.
/// </summary>
/// <param name="mediator"></param>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý bài viết & tin tức (CMS)")]
[Route("api/v{version:apiVersion}/news")]
public class NewsController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Tạo bài viết mới.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [HasPermission("Permissions.News.Create")]
    [SwaggerOperation(Summary = "Tạo bài viết mới")]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateNewsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách bài viết.
    /// </summary>
    /// <param name="sieveModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lấy danh sách tin tức")]
    [ProducesResponseType(typeof(PagedResult<NewsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNewsListQuery { SieveModel = sieveModel }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy chi tiết bài viết theo Slug.
    /// </summary>
    /// <param name="slug"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{slug}")]
    [SwaggerOperation(Summary = "Lấy chi tiết tin tức theo slug")]
    [ProducesResponseType(typeof(NewsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNewsBySlugQuery { Slug = slug }, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}

