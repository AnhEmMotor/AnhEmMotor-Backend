using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Application.Features.News.Commands.CreateNews;
using Application.Features.News.Commands.DeleteNews;
using Application.Features.News.Commands.UpdateNews;
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
/// Quản lý bài viết &amp; tin tức (CMS)
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý bài viết & tin tức (CMS)")]
[Route("api/v{version:apiVersion}/news")]
public class NewsController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Tạo bài viết mới
    /// </summary>
    /// <param name="command">Dữ liệu bài viết</param>
    /// <returns>Kết quả tạo</returns>
    [HttpPost]
    [HasPermission("Permissions.News.Create")]
    [SwaggerOperation(Summary = "Tạo bài viết mới")]
    public async Task<IActionResult> Create([FromBody] CreateNewsCommand command)
    {
        var result = await mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách tin tức
    /// </summary>
    /// <param name="sieveModel">Bộ lọc và phân trang</param>
    /// <returns>Danh sách tin tức phân trang</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lấy danh sách tin tức")]
    [ProducesResponseType(typeof(PagedResult<NewsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] SieveModel sieveModel)
    {
        var result = await mediator.Send(new GetNewsListQuery { SieveModel = sieveModel });
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật bài viết
    /// </summary>
    /// <param name="id">Mã bài viết</param>
    /// <param name="command">Dữ liệu cập nhật</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPut("{id}")]
    [HasPermission("Permissions.News.Update")]
    [SwaggerOperation(Summary = "Cập nhật bài viết")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateNewsCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");
        var result = await mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa bài viết
    /// </summary>
    /// <param name="id">Mã bài viết</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("{id}")]
    [HasPermission("Permissions.News.Delete")]
    [SwaggerOperation(Summary = "Xóa bài viết")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await mediator.Send(new DeleteNewsCommand(id));
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy chi tiết tin tức theo slug
    /// </summary>
    /// <param name="slug">Đường dẫn tĩnh bài viết</param>
    /// <returns>Chi tiết bài viết</returns>
    [HttpGet("{slug}")]
    [SwaggerOperation(Summary = "Lấy chi tiết tin tức theo slug")]
    [ProducesResponseType(typeof(NewsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var result = await mediator.Send(new GetNewsBySlugQuery { Slug = slug });
        return HandleResult(result);
    }
}

