using Application.ApiContracts.News.Responses;
using Application.ApiContracts.NewsCategory.Responses;
using Application.Common.Models;
using Application.Features.News.Commands.CreateNews;
using Application.Features.News.Commands.DeleteNews;
using Application.Features.News.Commands.UpdateNews;
using Application.Features.News.Commands.UpdateNewsStatus;
using Application.Features.News.Commands.UploadNewsContentImage;
using Application.Features.News.Commands.UploadNewsCoverImage;
using Application.Features.News.Queries.GetLatestNewsPublic;
using Application.Features.News.Queries.GetNewsById;
using Application.Features.News.Queries.GetNewsBySlug;
using Application.Features.News.Queries.GetRelatedNewsPublic;
using Application.Features.News.Queries.GetNewsList;
using Application.Features.News.Queries.GetNewsListForStore;
using Application.Features.News.Queries.GetProductsForNews;
using Application.Features.NewsCategories.Queries.GetNewsCategoryList;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    /// <param name="command">The create news command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpPost]
    [HasPermission(News.Create)]
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
    /// <param name="sieveModel">The sieve model for filtering.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lấy danh sách tin tức")]
    [ProducesResponseType(typeof(PagedResult<NewsSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNewsListQuery { SieveModel = sieveModel }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách bài viết (Public API cho Store).
    /// </summary>
    /// <param name="sieveModel">The sieve model for filtering.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet("public")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Lấy danh sách tin tức (Public API)")]
    [ProducesResponseType(typeof(PagedResult<NewsSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicListAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNewsListForStoreQuery { SieveModel = sieveModel }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy 5 tin tức mới nhất (Public API cho Store).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet("public/latest")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Lấy 5 tin tức mới nhất (Public API)")]
    [ProducesResponseType(typeof(List<NewsSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLatestPublicAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLatestNewsPublicQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy 5 tin tức liên quan theo slug bài viết (Public API cho Store).
    /// </summary>
    /// <param name="slug">Slug của bài viết hiện tại</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet("public/{slug}/related")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Lấy 5 tin tức liên quan (Public API)")]
    [ProducesResponseType(typeof(List<NewsSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRelatedPublicAsync(string slug, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRelatedNewsPublicQuery(slug), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách thể loại tin tức.
    /// </summary>
    /// <param name="sieveModel">The sieve model for filtering.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet("categories")]
    [SwaggerOperation(Summary = "Lấy danh sách thể loại tin tức")]
    [ProducesResponseType(typeof(PagedResult<NewsCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoryListAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNewsCategoryListQuery { SieveModel = sieveModel }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm (biến thể &amp; màu) để chọn gắn vào bài viết
    /// </summary>
    /// <returns></returns>
    [HttpGet("products-for-selection")]
    [SwaggerOperation(Summary = "Lấy danh sách sản phẩm (biến thể & màu) để chọn gắn vào bài viết")]
    public async Task<IActionResult> GetProductsForSelectionAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProductsForNewsQuery { SieveModel = sieveModel }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật bài viết
    /// </summary>
    /// <param name="id">Mã bài viết</param>
    /// <param name="request">Dữ liệu cập nhật</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPut("{id}")]
    [HasPermission(News.Edit)]
    [SwaggerOperation(Summary = "Cập nhật bài viết")]
    public async Task<IActionResult> UpdateAsync(
        int id,
        [FromBody] UpdateNewsCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateNewsCommand>() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa bài viết
    /// </summary>
    /// <param name="id">Mã bài viết</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("{id}")]
    [HasPermission(News.Delete)]
    [SwaggerOperation(Summary = "Xóa bài viết")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteNewsCommand(id), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy chi tiết tin tức theo id
    /// </summary>
    /// <param name="id">Mã bài viết</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Chi tiết bài viết</returns>
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Lấy chi tiết tin tức theo id")]
    [ProducesResponseType(typeof(NewsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNewsByIdQuery { Id = id }, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy chi tiết tin tức theo slug
    /// </summary>
    /// <param name="slug">Slug bài viết</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Chi tiết bài viết</returns>
    [HttpGet("public/{slug}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Lấy chi tiết tin tức theo slug (Public API)")]
    [ProducesResponseType(typeof(NewsForStoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlugPublicAsync(string slug, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNewsBySlugQuery { Slug = slug }, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật trạng thái hiển thị bài viết
    /// </summary>
    /// <param name="id">Mã bài viết</param>
    /// <param name="request">Trạng thái mới</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPatch("{id}/status")]
    [HasPermission(News.Edit)]
    [SwaggerOperation(Summary = "Cập nhật trạng thái hiển thị bài viết")]
    public async Task<IActionResult> UpdateStatusAsync(
        int id,
        [FromBody] UpdateNewsStatusCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateNewsStatusCommand>() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Upload ảnh bìa bài viết.
    /// </summary>
    [HttpPost("images/cover")]
    [SwaggerOperation(Summary = "Upload ảnh bìa bài viết")]
    public async Task<IActionResult> UploadCoverImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File không hợp lệ.");
        }
        var command = new UploadNewsCoverImageCommand
        {
            FileStream = file.OpenReadStream(),
            FileName = file.FileName,
            BaseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}"
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Upload ảnh trong nội dung bài viết (cho WangEditor).
    /// </summary>
    [HttpPost("images/content")]
    [SwaggerOperation(Summary = "Upload ảnh nội dung bài viết (WangEditor)")]
    public async Task<IActionResult> UploadContentImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var command = new UploadNewsContentImageCommand
        {
            File = file
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }
}

