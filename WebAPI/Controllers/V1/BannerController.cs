using Application.Features.Banners.Commands.CreateBanner;
using Application.Features.Banners.Commands.DeleteBanner;
using Application.Features.Banners.Commands.TrackBannerClick;
using Application.Features.Banners.Commands.UpdateBanner;
using Application.Features.Banners.Queries.GetActiveBanners;
using Application.Features.Banners.Queries.GetBannerAuditLogs;
using Application.Features.Banners.Queries.GetBannersList;
using Asp.Versioning;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý Banner &amp; Khuyến mãi (Marketing)
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý Banner & Khuyến mãi (Marketing)")]
[Route("api/v{version:apiVersion}/banners")]
public class BannerController(ISender sender) : ApiController
{
    /// <summary>
    /// Thêm banner mới
    /// </summary>
    /// <param name="command">Dữ liệu banner</param>
    /// <returns>Kết quả tạo mới</returns>
    [HttpPost]
    [HasPermission("Permissions.Banners.Create")]
    [SwaggerOperation(Summary = "Thêm banner mới")]
    public async Task<IActionResult> Create([FromBody] CreateBannerCommand command)
    {
        var result = await sender.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật banner
    /// </summary>
    /// <param name="id">Mã banner</param>
    /// <param name="command">Dữ liệu cập nhật</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPut("{id}")]
    [HasPermission("Permissions.Banners.Update")]
    [SwaggerOperation(Summary = "Cập nhật banner")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBannerCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");
        var result = await sender.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa banner
    /// </summary>
    /// <param name="id">Mã banner</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("{id}")]
    [HasPermission("Permissions.Banners.Delete")]
    [SwaggerOperation(Summary = "Xóa banner")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await sender.Send(new DeleteBannerCommand(id));
        return HandleResult(result);
    }

    /// <summary>
    /// Ghi nhận lượt click vào banner
    /// </summary>
    /// <param name="id">Mã banner</param>
    /// <returns>Kết quả</returns>
    [HttpPost("{id}/click")]
    [SwaggerOperation(Summary = "Ghi nhận lượt click banner")]
    public async Task<IActionResult> TrackClick(int id)
    {
        var result = await sender.Send(new TrackBannerClickCommand(id));
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách banner đang hoạt động
    /// </summary>
    /// <returns>Danh sách banner</returns>
    [HttpGet("active")]
    [SwaggerOperation(Summary = "Lấy danh sách banner đang hoạt động")]
    public async Task<IActionResult> GetActive()
    {
        var result = await sender.Send(new GetActiveBannersQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy toàn bộ danh sách banner (Dành cho quản lý)
    /// </summary>
    /// <returns>Danh sách banner</returns>
    [HttpGet]
    [HasPermission("Permissions.Banners.View")]
    [SwaggerOperation(Summary = "Lấy toàn bộ danh sách banner")]
    public async Task<IActionResult> GetList()
    {
        var result = await sender.Send(new GetBannersListQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy lịch sử thay đổi của banner
    /// </summary>
    /// <param name="id">Mã banner</param>
    /// <returns>Danh sách lịch sử</returns>
    [HttpGet("{id}/audit")]
    [HasPermission("Permissions.Banners.View")]
    [SwaggerOperation(Summary = "Lấy lịch sử thay đổi của banner")]
    public async Task<IActionResult> GetAuditLogs(int id)
    {
        var result = await sender.Send(new GetBannerAuditLogsQuery(id));
        return HandleResult(result);
    }
}
