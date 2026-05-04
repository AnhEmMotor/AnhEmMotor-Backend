using Application.Features.Banners.Commands.CreateBanner;
using Application.Features.Banners.Queries.GetActiveBanners;
using Asp.Versioning;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller quản lý Banner.
/// </summary>
/// <param name="sender"></param>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý Banner & Khuyến mãi (Marketing)")]
[Route("api/v{version:apiVersion}/banners")]
public class BannerController(ISender sender) : ApiController
{
    /// <summary>
    /// Thêm banner mới.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost]
    [HasPermission("Permissions.Banners.Create")]
    [SwaggerOperation(Summary = "Thêm banner mới")]
    public async Task<IActionResult> Create([FromBody] CreateBannerCommand command)
    {
        var result = await sender.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách banner đang hoạt động.
    /// </summary>
    /// <returns></returns>
    [HttpGet("active")]
    [SwaggerOperation(Summary = "Lấy danh sách banner đang hoạt động")]
    public async Task<IActionResult> GetActive()
    {
        var result = await sender.Send(new GetActiveBannersQuery());
        return HandleResult(result);
    }
}
