using Application.ApiContracts.File.Requests;
using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using Application.Features.Files.Commands.DeleteFile;
using Application.Features.Files.Commands.DeleteManyFiles;
using Application.Features.Files.Commands.RestoreFile;
using Application.Features.Files.Commands.RestoreManyFiles;
using Application.Features.Files.Commands.UploadBannerImage;
using Application.Features.Files.Commands.UploadManyProductImages;
using Application.Features.Files.Commands.UploadNewsImage;
using Application.Features.Files.Commands.UploadProductImage;
using Application.Features.Files.Queries.GetDeletedFilesList;
using Application.Features.Files.Queries.GetFileById;
using Application.Features.Files.Queries.GetFilesList;
using Application.Features.Files.Queries.ViewImage;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Domain.Constants.RouteNames;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý tệp media (ảnh, video, tài liệu).
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý tệp media")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class MediaFileController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách tệp media (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet]
    [HasPermission(Products.View)]
    [ProducesResponseType(typeof(PagedResult<MediaFileResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilesAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetFilesListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách tệp media đã bị xoá (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet("deleted")]
    [HasPermission(Products.View)]
    [ProducesResponseType(typeof(PagedResult<MediaFileResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedFilesAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedFilesListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin của tệp media được chọn.
    /// </summary>
    [HttpGet("{id:int}", Name = MediaFile.GetById)]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(typeof(MediaFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFileByIdAsync(int id, CancellationToken cancellationToken)
    {
        var query = new GetFileByIdQuery() { Id = id };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Tải lên một tệp ảnh cho sản phẩm.
    /// </summary>
    [HttpPost("product/upload")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(typeof(MediaFileResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadProductImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);
        var command = new UploadProductImageCommand { FileContent = file.OpenReadStream(), FileName = file.FileName };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleCreated(result, MediaFile.GetById, new { id = result.IsSuccess ? result.Value.Id : null });
    }

    /// <summary>
    /// Tải lên một tệp ảnh cho bài viết/tin tức.
    /// </summary>
    [HttpPost("news/upload")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(typeof(MediaFileResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadNewsImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);
        var command = new UploadNewsImageCommand { FileContent = file.OpenReadStream(), FileName = file.FileName };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleCreated(result, MediaFile.GetById, new { id = result.IsSuccess ? result.Value.Id : null });
    }

    /// <summary>
    /// Tải lên một tệp ảnh cho banner.
    /// </summary>
    [HttpPost("banner/upload")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(typeof(MediaFileResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadBannerImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);
        var command = new UploadBannerImageCommand { FileContent = file.OpenReadStream(), FileName = file.FileName };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleCreated(result, MediaFile.GetById, new { id = result.IsSuccess ? result.Value.Id : null });
    }

    /// <summary>
    /// Tải lên nhiều ảnh sản phẩm cùng lúc.
    /// </summary>
    [HttpPost("product/upload-many")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(typeof(List<MediaFileResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadManyProductImagesAsync(
        List<IFormFile> files,
        CancellationToken cancellationToken)
    {
        var command = new UploadManyProductImagesCommand
        {
            Files =
                files.Select(f => new FileParameter { Content = f.OpenReadStream(), FileName = f.FileName }).ToList()
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleCreated(result);
    }

    /// <summary>
    /// Xoá tệp media sản phẩm theo tên file.
    /// </summary>
    [HttpDelete("product/{**storagePath}")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductFileAsync(string storagePath, CancellationToken cancellationToken)
    {
        var command = new DeleteProductImageCommand() { StoragePath = storagePath };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Xoá nhiều tệp media cùng lúc.
    /// </summary>
    [HttpDelete("delete-many")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteFilesAsync(
        [FromBody] DeleteManyFilesCommand request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục lại tệp media đã xoá theo tên file.
    /// </summary>
    [HttpPost("restore/{**storagePath}")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(typeof(MediaFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreFileAsync(string storagePath, CancellationToken cancellationToken)
    {
        var command = new RestoreFileCommand() with { StoragePath = storagePath };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục nhiều tệp media đã xoá cùng lúc.
    /// </summary>
    [HttpPost("restore-many")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(typeof(List<MediaFileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreFilesAsync(
        [FromBody] RestoreManyFilesCommand request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Xem ảnh với khả năng resize theo kích thước mong muốn.
    /// </summary>
    [HttpGet("view-image/{**storagePath}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ViewImageWithResizeAsync(
        string storagePath,
        [FromQuery] int? width,
        CancellationToken cancellationToken)
    {
        var query = new ViewImageQuery { StoragePath = storagePath, Width = width };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}

