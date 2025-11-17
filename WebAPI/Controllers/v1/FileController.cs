using Application.ApiContracts.File;
using Application.Interfaces.Services.File;
using Asp.Versioning;
using Domain.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.v1
{
    /// <summary>
    /// Quản lý file
    /// </summary>
    /// <param name="fileService"></param>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public class FileController(IFileService fileService) : ControllerBase
    {
        /// <summary>
        /// Up 1 ảnh lên hệ thống
        /// </summary>
        /// <param name="file">File ảnh up lên</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("upload-single")]
        [ProducesResponseType(typeof(UploadResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadSingleImage(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail { Message = "No file uploaded." }] });
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var (data, error) = await fileService.UploadSingleFileAsync(file, baseUrl, cancellationToken).ConfigureAwait(true);

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
        [ProducesResponseType(typeof(List<UploadResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadMultipleImages(List<IFormFile> files, CancellationToken cancellationToken)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail { Message = "No files uploaded." }] });
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var (data, error) = await fileService.UploadMultipleFilesAsync(files, baseUrl, cancellationToken).ConfigureAwait(true);
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
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteFile(string fileName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest(new ErrorResponse { Errors = [new ErrorDetail { Message = "File name is required." }] });
            }

            var error = await fileService.DeleteFileAsync(fileName, cancellationToken).ConfigureAwait(true);
            if (error is not null)
            {
                return BadRequest(error);
            }

            return Ok();
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

            var error = await fileService.RestoreFileAsync(fileName, cancellationToken).ConfigureAwait(true);
            if (error is not null)
            {
                return BadRequest(error);
            }

            return Ok();
        }

        /// <summary>
        /// Xoá nhiều file cùng lúc theo danh sách tên file đã lưu
        /// </summary>
        /// <param name="fileNames">Danh sách tên file đã lưu (stored file names)</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("delete-multiple")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteMultipleFiles([FromBody] List<string> fileNames, CancellationToken cancellationToken)
        {
            if (fileNames == null || fileNames.Count == 0)
            {
                return BadRequest(new ErrorResponse { Errors = [new ErrorDetail { Message = "No file names provided." }] });
            }

            var error = await fileService.DeleteMultipleFilesAsync(fileNames, cancellationToken).ConfigureAwait(true);
            if (error is not null)
            {
                return BadRequest(error);
            }

            return Ok();
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

            var error = await fileService.RestoreMultipleFilesAsync(fileNames, cancellationToken).ConfigureAwait(true);
            if (error is not null)
            {
                return BadRequest(error);
            }

            return Ok();
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
            const int MAX_WIDTH = 2400;
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail { Message = "File name is required." }] });
            }

            if (w.HasValue && w.Value > MAX_WIDTH)
            {
                var errorMessage = $"Width 'w' cannot exceed {MAX_WIDTH} pixels.";
                return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail { Message = errorMessage }] });
            }

            var (imageResult, imageError) = await fileService.GetImageAsync(fileName, w, cancellationToken).ConfigureAwait(true);

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
}
