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
using Domain.Constants.Permission;
using Domain.Constants.RouteNames;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using SixLabors.ImageSharp;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý tệp media (ảnh, video, tài liệu) — tải lên, xóa, khôi phục, xem ảnh với resize.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý tệp media")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class MediaFileController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách tệp media (có phân trang, lọc, sắp xếp theo quy tắc Sieve).
    /// </summary>
    /// <param name="sieveModel">Tham số phân trang, lọc, sắp xếp.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách tệp media đã phân trang.</returns>
    /// <response code="200">Trả về danh sách tệp media thành công.</response>
    [HttpGet]
    [RequiresAnyPermissions(Permissions.Warehouse.ProductManagement.View, Permissions.Order.ProductManagement.View)]
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
    /// <param name="sieveModel">Tham số phân trang, lọc, sắp xếp.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách tệp media đã bị xoá đã phân trang.</returns>
    /// <response code="200">Trả về danh sách tệp đã xoá thành công.</response>
    [HttpGet("deleted")]
    [RequiresAnyPermissions(Permissions.Warehouse.ProductManagement.View, Permissions.Order.ProductManagement.View)]
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
    /// Lấy thông tin chi tiết của một tệp media theo ID.
    /// </summary>
    /// <param name="id">ID của tệp media cần xem.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin chi tiết của tệp media.</returns>
    /// <response code="200">Trả về thông tin tệp media thành công.</response>
    /// <response code="404">Không tìm thấy tệp media với ID đã cho.</response>
    [HttpGet("{id:int}", Name = MediaFile.GetById)]
    [RequiresAnyPermissions(
        Permissions.Warehouse.ProductManagement.Edit,
        Permissions.Order.ProductManagement.Edit,
        Permissions.Warehouse.ProductManagement.Create,
        Permissions.Order.ProductManagement.Create)]
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
    /// <param name="file">Tệp ảnh (JPEG, PNG) cần tải lên.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin tệp media đã tải lên thành công.</returns>
    /// <response code="201">Tải lên ảnh sản phẩm thành công.</response>
    /// <response code="400">File rỗng hoặc định dạng không hợp lệ.</response>
    [HttpPost("product/upload")]
    [RequiresAnyPermissions(
        Permissions.Warehouse.ProductManagement.Edit,
        Permissions.Order.ProductManagement.Edit,
        Permissions.Warehouse.ProductManagement.Create,
        Permissions.Order.ProductManagement.Create)]
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
    /// <param name="file">Tệp ảnh (JPEG, PNG) cần tải lên.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin tệp media đã tải lên thành công.</returns>
    /// <response code="201">Tải lên ảnh bài viết thành công.</response>
    /// <response code="400">File rỗng hoặc định dạng không hợp lệ.</response>
    [HttpPost("news/upload")]
    [RequiresAnyPermissions(
        Permissions.Warehouse.ProductManagement.Edit,
        Permissions.Order.ProductManagement.Edit,
        Permissions.Warehouse.ProductManagement.Create,
        Permissions.Order.ProductManagement.Create)]
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
    /// Tải lên một tệp ảnh cho banner (slider trang chủ).
    /// </summary>
    /// <param name="file">Tệp ảnh banner (JPEG, PNG) cần tải lên.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin tệp media đã tải lên thành công.</returns>
    /// <response code="201">Tải lên ảnh banner thành công.</response>
    /// <response code="400">File rỗng hoặc định dạng không hợp lệ.</response>
    [HttpPost("banner/upload")]
    [RequiresAnyPermissions(
        Permissions.Warehouse.ProductManagement.Edit,
        Permissions.Order.ProductManagement.Edit,
        Permissions.Warehouse.ProductManagement.Create,
        Permissions.Order.ProductManagement.Create)]
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
    /// Tải lên nhiều ảnh sản phẩm cùng lúc (hỗ trợ upload nhóm).
    /// </summary>
    /// <param name="files">Danh sách tệp ảnh cần tải lên.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách thông tin tệp media đã tải lên thành công.</returns>
    /// <response code="201">Tải lên nhiều ảnh thành công.</response>
    /// <response code="400">File rỗng hoặc định dạng không hợp lệ.</response>
    [HttpPost("product/upload-many")]
    [RequiresAnyPermissions(
        Permissions.Warehouse.ProductManagement.Edit,
        Permissions.Order.ProductManagement.Edit,
        Permissions.Warehouse.ProductManagement.Create,
        Permissions.Order.ProductManagement.Create)]
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
    /// Xoá tệp media sản phẩm theo đường dẫn lưu trữ (storage path).
    /// </summary>
    /// <param name="storagePath">Đường dẫn lưu trữ của tệp (catch-all route).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả xoá tệp (204 No Content nếu thành công).</returns>
    /// <response code="204">Xoá tệp thành công.</response>
    /// <response code="404">Không tìm thấy tệp media.</response>
    [HttpDelete("product/{**storagePath}")]
    [RequiresAnyPermissions(
        Permissions.Warehouse.ProductManagement.Edit,
        Permissions.Order.ProductManagement.Edit,
        Permissions.Warehouse.ProductManagement.Create,
        Permissions.Order.ProductManagement.Create)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductFileAsync(string storagePath, CancellationToken cancellationToken)
    {
        var command = new DeleteProductImageCommand() { StoragePath = storagePath };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Xoá nhiều tệp media cùng lúc theo danh sách đường dẫn.
    /// </summary>
    /// <param name="request">Yêu cầu chứa danh sách đường dẫn tệp cần xoá.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả xoá nhiều tệp.</returns>
    /// <response code="204">Xoá nhiều tệp thành công.</response>
    /// <response code="400">Danh sách tệp không hợp lệ.</response>
    [HttpDelete("delete-many")]
    [RequiresAnyPermissions(
        Permissions.Warehouse.ProductManagement.Edit,
        Permissions.Order.ProductManagement.Edit,
        Permissions.Warehouse.ProductManagement.Create,
        Permissions.Order.ProductManagement.Create)]
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
    /// Khôi phục lại tệp media đã bị xoá mềm (soft-delete) theo đường dẫn lưu trữ.
    /// </summary>
    /// <param name="storagePath">Đường dẫn lưu trữ của tệp cần khôi phục (catch-all route).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin tệp media đã khôi phục.</returns>
    /// <response code="200">Khôi phục tệp thành công.</response>
    /// <response code="404">Không tìm thấy tệp media đã xoá.</response>
    [HttpPost("restore/{**storagePath}")]
    [RequiresAnyPermissions(
        Permissions.Warehouse.ProductManagement.Edit,
        Permissions.Order.ProductManagement.Edit,
        Permissions.Warehouse.ProductManagement.Create,
        Permissions.Order.ProductManagement.Create)]
    [ProducesResponseType(typeof(MediaFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreFileAsync(string storagePath, CancellationToken cancellationToken)
    {
        var command = new RestoreFileCommand() with { StoragePath = storagePath };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục nhiều tệp media đã bị xoá cùng lúc theo danh sách đường dẫn.
    /// </summary>
    /// <param name="request">Yêu cầu chứa danh sách đường dẫn tệp cần khôi phục.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách thông tin tệp media đã khôi phục.</returns>
    /// <response code="200">Khôi phục nhiều tệp thành công.</response>
    /// <response code="400">Danh sách tệp không hợp lệ.</response>
    [HttpPost("restore-many")]
    [RequiresAnyPermissions(
        Permissions.Warehouse.ProductManagement.Edit,
        Permissions.Order.ProductManagement.Edit,
        Permissions.Warehouse.ProductManagement.Create,
        Permissions.Order.ProductManagement.Create)]
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
    /// Tải lên ảnh hướng dẫn sử dụng (User Manual) với tên file cố định để ghi đè.
    /// </summary>
    /// <param name="file">Tệp ảnh cần tải lên.</param>
    /// <param name="targetFileName">Tên file mong muốn (ví dụ: customer-data).</param>
    /// <param name="environment">Môi trường ứng dụng.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpPost("manual/upload")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadManualImageAsync(
        IFormFile file,
        [FromQuery] string targetFileName,
        [FromServices] IWebHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (string.IsNullOrWhiteSpace(targetFileName))
        {
            return BadRequest(new ErrorResponse("Target file name is required."));
        }
        var webRoot = string.IsNullOrWhiteSpace(environment.WebRootPath)
            ? Path.Combine(environment.ContentRootPath, "wwwroot")
            : environment.WebRootPath;
        var manualsDir = Path.Combine(webRoot, "uploads", "manuals");
        if (!Directory.Exists(manualsDir))
        {
            Directory.CreateDirectory(manualsDir);
        }
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".webp")
        {
            return BadRequest(new ErrorResponse("Only image files (.png, .jpg, .jpeg, .webp) are allowed."));
        }
        var targetFile = Path.Combine(manualsDir, $"{targetFileName}.webp");
        try
        {
            using (var fileStream = new FileStream(targetFile, FileMode.Create))
            {
                await file.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            }
            var publicUrl = $"/api/v1/MediaFile/view-image/manuals/{targetFileName}.webp";
            return Ok(new { PublicUrl = publicUrl });
        } catch (Exception ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Xem ảnh với khả năng resize theo kích thước mong muốn (dùng cho thumbnail/preview).
    /// </summary>
    /// <param name="storagePath">Đường dẫn lưu trữ của ảnh (catch-all route).</param>
    /// <param name="width">Chiều rộng mong muốn sau resize (tuỳ chọn — null trả về kích thước gốc).</param>
    /// <param name="download">Nếu true, trả về file để tải về thay vì hiển thị trực tiếp.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Ảnh đã được resize (nếu có width) hoặc ảnh gốc (content-type: image/*).</returns>
    /// <response code="200">Trả về ảnh thành công.</response>
    /// <response code="404">Không tìm thấy ảnh.</response>
    /// <response code="400">Tham số width không hợp lệ.</response>
    [HttpGet("view-image/{**storagePath}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ViewImageWithResizeAsync(
        string storagePath,
        [FromQuery] int? width,
        [FromQuery] bool download,
        CancellationToken cancellationToken)
    {
        var query = new ViewImageQuery { StoragePath = storagePath, Width = width };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
