using Application.Features.Banners.Commands.CreateBanner;
using Asp.Versioning;
using Domain.Constants;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[SwaggerTag("Quản lý Banner & Khuyến mãi (Marketing)")]
[Route("api/v{version:apiVersion}/banners")]
public class BannerController(ISender sender) : ApiController
{
    [HttpPost]
    [HasPermission("Permissions.Banners.Create")]
    [SwaggerOperation(Summary = "Thêm banner mới")]
    public async Task<IActionResult> Create([FromBody] CreateBannerCommand command)
    {
        var result = await sender.Send(command);
        return HandleResult(result);
    }

    [HttpGet("active")]
    [SwaggerOperation(Summary = "Lấy danh sách banner đang hoạt động")]
    public async Task<IActionResult> GetActive()
    {
        var result = await sender.Send(new Application.Features.Banners.Queries.GetActiveBanners.GetActiveBannersQuery());
        return HandleResult(result);
    }
}
