using Application.ApiContracts.File;
using Application.Features.Files.Commands.DeleteFile;
using Application.Features.Files.Commands.DeleteMultipleFiles;
using Application.Features.Files.Commands.RestoreFile;
using Application.Features.Files.Commands.RestoreMultipleFiles;
using Application.Features.Files.Commands.UploadFile;
using Application.Features.Files.Commands.UploadMultipleFiles;
using Application.Interfaces.Services;
using Asp.Versioning;
using Domain.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý file ảnh trong hệ thống
/// </summary>
/// <param name="mediator"></param>
/// <param name="fileSelectService"></param>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class FileController(IMediator mediator, IFileSelectService fileSelectService) : ControllerBase
{
    /// <summary>
    /// Xem ảnh theo tên file, có thể truyền Width để resize
    /// </summary>
    /// <param name="fileName">Tên file đã lưu (ví dụ: guid.webp)</param>
    /// <param name="width">Chiều rộng mong muốn (optional, nếu không truyền sẽ trả về ảnh gốc)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("view/{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ViewImage(string fileName, [FromQuery] int? width, CancellationToken cancellationToken)
    {
        var (data, error) = await fileSelectService.GetImageAsync(fileName, width, cancellationToken).ConfigureAwait(true);
        
        if (error is not null)
        {
            return NotFound(error);
        }

        if (data is null)
        {
            return NotFound(new ErrorResponse { Errors = [new ErrorDetail { Message = "Image not found." }] });
        }

        return File(data.Value.fileStream, data.Value.contentType);
    }

    /// <summary>
    /// Up 1 ảnh lên hệ thống
    /// </summary>
    /// <param name="file"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("upload-single")]
    [ProducesResponseType(typeof(FileResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadSingleImage(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ErrorResponse { Errors = [new ErrorDetail { Message = "No file uploaded." }] });
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var command = new UploadFileCommand(file, baseUrl);
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);

        if (error is not null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Up nhiều ảnh lên hệ thống
    /// </summary>
    /// <param name="files"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("upload-multiple")]
    [ProducesResponseType(typeof(List<FileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadMultipleImages(List<IFormFile> files, CancellationToken cancellationToken)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(new ErrorResponse { Errors = [new ErrorDetail { Message = "No files uploaded." }] });
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var command = new UploadMultipleFilesCommand(files, baseUrl);
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);

        if (error is not null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Xoá 1 file theo tên file đã lưu (stored file name)
    /// </summary>
    /// <param name="fileName">Tên file đã lưu (ví dụ: guid.webp)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("delete/{fileName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteFile(string fileName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest(new ErrorResponse { Errors = [new ErrorDetail { Message = "File name is required." }] });
        }

        var command = new DeleteFileCommand(fileName);
        var error = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if (error is not null)
        {
            return BadRequest(error);
        }

        return NoContent();
    }

    /// <summary>
    /// Xoá nhiều file cùng lúc theo danh sách tên file đã lưu
    /// </summary>
    /// <param name="fileNames">Danh sách tên file đã lưu (stored file names)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("delete-multiple")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteMultipleFiles([FromBody] List<string?> fileNames, CancellationToken cancellationToken)
    {
        if (fileNames == null || fileNames.Count == 0)
        {
            return BadRequest(new ErrorResponse { Errors = [new ErrorDetail { Message = "File names are required." }] });
        }

        var command = new DeleteMultipleFilesCommand(fileNames);
        var error = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if (error is not null)
        {
            return BadRequest(error);
        }

        return NoContent();
    }

    /// <summary>
    /// Khôi phục 1 file theo tên file đã lưu (stored file name)
    /// </summary>
    /// <param name="fileName">Tên file đã lưu (ví dụ: guid.webp)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPatch("restore/{fileName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreFile(string fileName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest(new ErrorResponse { Errors = [new ErrorDetail { Message = "File name is required." }] });
        }

        var command = new RestoreFileCommand(fileName);
        var error = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if (error is not null)
        {
            return BadRequest(error);
        }

        return NoContent();
    }

    /// <summary>
    /// Khôi phục nhiều file cùng lúc theo danh sách tên file đã lưu
    /// </summary>
    /// <param name="fileNames">Danh sách tên file đã lưu (stored file names)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPatch("restore-multiple")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreMultipleFiles([FromBody] List<string> fileNames, CancellationToken cancellationToken)
    {
        if (fileNames == null || fileNames.Count == 0)
        {
            return BadRequest(new ErrorResponse { Errors = [new ErrorDetail { Message = "File names are required." }] });
        }

        var command = new RestoreMultipleFilesCommand(fileNames);
        var error = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if (error is not null)
        {
            return BadRequest(error);
        }

        return NoContent();
    }
}
