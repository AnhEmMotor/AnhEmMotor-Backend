using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;
using Infrastructure.Authorization.Attribute;
using Asp.Versioning;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller quản lý hình ảnh cho bài viết/tin tức.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/news/images")]
[ApiVersion("1.0")]
[SwaggerTag("Quản lý hình ảnh bài viết")]
public class NewsImageController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public NewsImageController(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    /// <summary>
    /// Upload ảnh bìa bài viết (có nén).
    /// </summary>
    [HttpPost("cover")]
    // [HasPermission("Domain.Constants.Permission.Permissions.News.Create")] // Optional, depending on if you want it protected. I'll leave it open for now or just let the API authorize it.
    [SwaggerOperation(Summary = "Upload ảnh bìa bài viết")]
    public async Task<IActionResult> UploadCoverImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File không hợp lệ.");
        }

        var relativePath = "articles/covers";
        
        // _env.WebRootPath automatically points to LocalFileStorage:UploadPath if configured, else wwwroot.
        var uploadFolder = Path.Combine(_env.WebRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        var fileName = $"{Guid.NewGuid()}.jpg";
        var filePath = Path.Combine(uploadFolder, fileName);

        using var image = await Image.LoadAsync(file.OpenReadStream());
        
        // Resize if it's too large, e.g., max width 1200
        if (image.Width > 1200)
        {
            image.Mutate(x => x.Resize(1200, 0));
        }

        // Save as JPEG with compression
        await image.SaveAsync(filePath, new JpegEncoder { Quality = 80 });

        // Generate URL
        var request = HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
        
        var publicPath = "/" + relativePath.Replace('\\', '/');
        var url = $"{baseUrl}{publicPath}/{Uri.EscapeDataString(fileName)}";

        return Ok(new { url });
    }

    /// <summary>
    /// Upload ảnh trong nội dung bài viết (cho WangEditor).
    /// </summary>
    [HttpPost("content")]
    [SwaggerOperation(Summary = "Upload ảnh nội dung bài viết (WangEditor)")]
    public async Task<IActionResult> UploadContentImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { errno = 1, message = "File không hợp lệ." });
        }

        var relativePath = "articles/content";

        var uploadFolder = Path.Combine(_env.WebRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Generate URL
        var request = HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
        
        var publicPath = "/" + relativePath.Replace('\\', '/');
        var url = $"{baseUrl}{publicPath}/{Uri.EscapeDataString(fileName)}";

        // Format required by WangEditor custom upload OR standard upload
        // We will return standard format
        return Ok(new
        {
            errno = 0,
            data = new
            {
                url = url,
                alt = file.FileName,
                href = url
            }
        });
    }
}
