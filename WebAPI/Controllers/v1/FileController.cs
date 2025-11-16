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
        [ProducesResponseType(typeof(UploadResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadSingleImage(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new UploadResponse { IsSuccess = false, Error = "No file uploaded." });
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await fileService.UploadSingleFileAsync(file, baseUrl, cancellationToken).ConfigureAwait(true);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Up nhiều ảnh lên hệ thống
        /// </summary>
        /// <param name="files"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("upload-multiple")]
        [ProducesResponseType(typeof(List<UploadResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UploadResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadMultipleImages(List<IFormFile> files, CancellationToken cancellationToken)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(new UploadResponse { IsSuccess = false, Error = "No files uploaded." });
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var results = await fileService.UploadMultipleFilesAsync(files, baseUrl, cancellationToken).ConfigureAwait(true);
            return Ok(results);
        }

        /// <summary>
        /// Xuất ảnh dựa vào tên file, có thể thay đổi kích thước ảnh bằng tham số w (width)
        /// </summary>
        /// <param name="fileName">Tên file cần lấy</param>
        /// <param name="w">Chiều rộng của ảnh cần lấy</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetImage(string fileName, [FromQuery] int? w, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            var result = await fileService.GetImageAsync(fileName, w, cancellationToken).ConfigureAwait(true);

            if (result == null)
            {
                return NotFound();
            }

            return File(result.Value.fileStream, result.Value.contentType);
        }
    }
}
