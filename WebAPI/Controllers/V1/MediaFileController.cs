using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using Application.Features.Files.Commands.DeleteFile;
using Application.Features.Files.Commands.DeleteManyFiles;
using Application.Features.Files.Commands.RestoreFile;
using Application.Features.Files.Commands.RestoreManyFiles;
using Application.Features.Files.Commands.UploadImage;
using Application.Features.Files.Commands.UploadManyImage;
using Application.Features.Files.Queries.GetDeletedFilesList;
using Application.Features.Files.Queries.GetFileById;
using Application.Features.Files.Queries.GetFilesList;
using Application.Features.Files.Queries.ViewImage;
using Asp.Versioning;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;
using static Domain.Constants.Permission.PermissionsList;

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
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
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
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin của tệp media được chọn.
    /// </summary>
    [HttpGet("{id:int}", Name = RouteNames.MediaFile.GetById)]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(typeof(MediaFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFileByIdAsync(int id, CancellationToken cancellationToken)
    {
        var query = new GetFileByIdQuery() { Id = id };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tải lên một tệp ảnh.
    /// </summary>
    [HttpPost("upload-image")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(typeof(MediaFileResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if(file == null)
            return BadRequest();
        var command = new UploadImageCommand { FileContent = file.OpenReadStream(), FileName = file.FileName };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(result, RouteNames.MediaFile.GetById, new { id = result.IsSuccess ? result.Value.Id : null } );
    }

    /// <summary>
    /// Tải lên nhiều ảnh cùng lúc.
    /// </summary>
    [HttpPost("upload-images")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(typeof(List<MediaFileResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadManyImagesAsync(List<IFormFile> files, CancellationToken cancellationToken)
    {
        var fileDtos = new List<(Stream FileContent, string FileName)>();

        foreach(var file in files)
        {
            if(file.Length > 0)
            {
                fileDtos.Add((file.OpenReadStream(), file.FileName));
            }
        }

        var command = new UploadManyImageCommand { Files = fileDtos };

        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(result);
    }

    /// <summary>
    /// Xoá tệp media theo tên file.
    /// </summary>
    [HttpDelete("{storagePath}")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFileAsync(string storagePath, CancellationToken cancellationToken)
    {
        var command = new DeleteFileCommand() with { StoragePath = storagePath };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
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
        var result = await mediator.Send(request, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục lại tệp media đã xoá theo tên file.
    /// </summary>
    [HttpPost("restore/{storagePath}")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(typeof(MediaFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreFileAsync(string storagePath, CancellationToken cancellationToken)
    {
        var command = new RestoreFileCommand() with { StoragePath = storagePath };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
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
        var result = await mediator.Send(request, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xem ảnh với khả năng resize theo kích thước mong muốn.
    /// </summary>
    [HttpGet("view-image/{storagePath}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ViewImageWithResizeAsync(
        string storagePath,
        [FromQuery] int? width,
        CancellationToken cancellationToken)
    {
        var query = new ViewImageQuery { StoragePath = storagePath, Width = width };

        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);

        if(result.IsFailure)
        {
            return HandleResult(result);
        }

        if(result.Value is { } imageData)
        {
            var (fileStream, contentType) = imageData;
            return File(fileStream, contentType);
        }

        return StatusCode(StatusCodes.Status500InternalServerError);
    }
}
