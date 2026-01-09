using Application.ApiContracts.File.Responses;
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

using Infrastructure.Authorization.Attribute;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using static Domain.Constants.Permission.PermissionsList;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý tệp media (ảnh, video, tài liệu).
/// </summary>
/// <param name="mediator"></param>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý tệp media")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status500InternalServerError)]
public class MediaFileController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách tệp media (có phân trang, lọc, sắp xếp).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [NonAction]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<MediaFileResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilesAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetFilesListQuery(sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Lấy danh sách tệp media đã bị xoá (có phân trang, lọc, sắp xếp).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [NonAction]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<MediaFileResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedFilesAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedFilesListQuery(sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Lấy thông tin của tệp media được chọn.
    /// </summary>
    /// <param name="id">Mã tệp media cần lấy thông tin.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [NonAction]
    [ProducesResponseType(typeof(MediaFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFileByIdAsync(int id, CancellationToken cancellationToken)
    {
        var query = new GetFileByIdQuery(id);
        var (data, error) = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Tải lên một tệp ảnh.
    /// </summary>
    /// <param name="file">Tệp ảnh cần upload.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("upload-image")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(typeof(MediaFileResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken cancellationToken)
    {
        if(file == null)
            return BadRequest();
        var command = new UploadImageCommand { FileContent = file.OpenReadStream(), FileName = file.FileName };
        var response = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Tải lên nhiều ảnh cùng lúc.
    /// </summary>
    /// <param name="files">Danh sách tệp ảnh cần upload.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("upload-images")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(typeof(List<MediaFileResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadManyImages(List<IFormFile> files, CancellationToken cancellationToken)
    {
        var fileDtos = new List<Application.ApiContracts.File.Requests.FileUploadRequest>();

        foreach(var file in files)
        {
            if(file.Length > 0)
            {
                fileDtos.Add(
                    new Application.ApiContracts.File.Requests.FileUploadRequest(file.OpenReadStream(), file.FileName));
            }
        }

        var command = new UploadManyImageCommand { Files = fileDtos };

        var responses = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return Ok(responses);
    }

    /// <summary>
    /// Xoá tệp media theo tên file.
    /// </summary>
    /// <param name="storagePath">Tên file đã lưu (GUID.webp).</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{storagePath}")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFile(string storagePath, CancellationToken cancellationToken)
    {
        var command = new DeleteFileCommand() with { StoragePath = storagePath };
        var error = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return NoContent();
    }

    /// <summary>
    /// Xoá nhiều tệp media cùng lúc.
    /// </summary>
    /// <param name="request">Danh sách Id tệp media cần xoá.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("delete-many")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteFiles(
        [FromBody] Application.ApiContracts.File.Requests.DeleteManyMediaFilesRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<DeleteManyFilesCommand>();
        var error = await mediator.Send(command, cancellationToken).ConfigureAwait(true);

        if(error != null)
        {
            return BadRequest(error);
        }

        return NoContent();
    }

    /// <summary>
    /// Khôi phục lại tệp media đã xoá theo tên file.
    /// </summary>
    /// <param name="storagePath">Tên file đã lưu (GUID.webp).</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("restore/{storagePath}")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(typeof(MediaFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreFile(string storagePath, CancellationToken cancellationToken)
    {
        var command = new RestoreFileCommand() with { StoragePath = storagePath };
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);

        if(error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Khôi phục nhiều tệp media đã xoá cùng lúc.
    /// </summary>
    /// <param name="request">Danh sách Id tệp media cần khôi phục.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("restore-many")]
    [RequiresAnyPermissions(Products.Edit, Products.Create)]
    [ProducesResponseType(typeof(List<MediaFileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreFiles(
        [FromBody] Application.ApiContracts.File.Requests.RestoreManyMediaFilesRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManyFilesCommand>();
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);

        if(error != null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Xem ảnh với khả năng resize theo kích thước mong muốn.
    /// </summary>
    /// <param name="storagePath">Đường dẫn lưu trữ của ảnh.</param>
    /// <param name="width">Chiều rộng mong muốn (pixel). Nếu không truyền sẽ trả về ảnh gốc.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("view-image/{storagePath}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ViewImageWithResize(
        string storagePath,
        [FromQuery] int? width,
        CancellationToken cancellationToken)
    {
        var query = new ViewImageQuery { StoragePath = storagePath, Width = width };

        var (data, error) = await mediator.Send(query, cancellationToken).ConfigureAwait(true);

        if(error != null)
        {
            return NotFound(error);
        }

        if(data == null)
        {
            return NotFound(
                new Application.Common.Models.ErrorResponse
                {
                    Errors = [ new Application.Common.Models.ErrorDetail { Message = "Image not found." } ]
                });
        }

        var (fileStream, contentType) = data.Value;
        return File(fileStream, contentType);
    }
}
