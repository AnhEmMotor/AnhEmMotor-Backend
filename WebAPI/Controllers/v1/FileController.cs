using Application.ApiContracts.File;
using Application.Interfaces.Services.File;
using Asp.Versioning;
using Domain.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý file ảnh trong hệ thống
/// </summary>
/// <param name="insertService"></param>
/// <param name="selectService"></param>
/// <param name="deleteService"></param>
/// <param name="updateService"></param>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class FileController(
    IFileInsertService insertService,
    IFileSelectService selectService,
    IFileDeleteService deleteService,
    IFileUpdateService updateService) : ControllerBase
{
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
        var (data, error) = await insertService.UploadSingleFileAsync(file, baseUrl, cancellationToken).ConfigureAwait(true);

        if (error is not null)
        {
            return BadRequest(error);
        }

        return CreatedAtAction(nameof(GetImage), new { fileName = data?.FileName }, data);
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
        var (data, error) = await insertService.UploadMultipleFilesAsync(files, baseUrl, cancellationToken).ConfigureAwait(true);

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

        var error = await deleteService.DeleteFileAsync(fileName, cancellationToken).ConfigureAwait(true);
        if (error is not null)
        {
            return BadRequest(error);
        }

        return NoContent();
    }

    /// <summary>
    /// Khôi phục 1 file theo tên file đã lưu
    /// </summary>
    /// <param name="fileName">Tên file đã lưu</param>
    /// <param name="cancellationToken"></param>
    [HttpPost("restore/{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreFile(string fileName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest(new ErrorResponse { Errors = [new ErrorDetail { Message = "File name is required." }] });
        }

        var (error, response) = await updateService.RestoreFileAsync(fileName, cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return BadRequest(error);
        }

        return Ok(response);
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
            return BadRequest(new ErrorResponse { Errors = [new ErrorDetail { Message = "No file names provided." }] });
        }

        var error = await deleteService.DeleteMultipleFilesAsync(fileNames, cancellationToken).ConfigureAwait(true);
        if (error is not null)
        {
            return BadRequest(error);
        }

        return NoContent();
    }

    /// <summary>
    /// Khôi phục nhiều file cùng lúc theo danh sách tên file đã lưu
    /// </summary>
    /// <param name="fileNames">Tên file cần khôi phục</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("restore-multiple")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreMultipleFiles([FromBody] List<string> fileNames, CancellationToken cancellationToken)
    {
        if (fileNames == null || fileNames.Count == 0)
        {
            return BadRequest(new ErrorResponse { Errors = [new ErrorDetail { Message = "No file names provided." }] });
        }

        var (error, response) = await updateService.RestoreMultipleFilesAsync(fileNames, cancellationToken).ConfigureAwait(true);
        if (error is not null)
        {
            return BadRequest(error);
        }

        return Ok(response);
    }

    /// <summary>
    /// Xuất ảnh dựa vào tên file, có thể thay đổi kích thước ảnh bằng tham số w (width)
    /// </summary>
    /// <param name="fileName">Tên file cần lấy</param>
    /// <param name="w">Chiều rộng của ảnh cần lấy</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{fileName}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetImage(string fileName, [FromQuery] int? w, CancellationToken cancellationToken)
    {
        const int maxWidth = 2400;
        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest(new ErrorResponse { Errors = [new ErrorDetail { Message = "File name is required." }] });
        }

        if (w.HasValue && w.Value > maxWidth)
        {
            return BadRequest(new ErrorResponse { Errors = [new ErrorDetail { Message = $"Width 'w' cannot exceed {maxWidth} pixels." }] });
        }

        var (imageResult, imageError) = await selectService.GetImageAsync(fileName, w, cancellationToken).ConfigureAwait(true);

        if (imageError is not null)
        {
            return NotFound(imageError);
        }

        if (imageResult == null)
        {
            return NotFound();
        }

        return File(imageResult.Value.fileStream, imageResult.Value.contentType);
    }
}