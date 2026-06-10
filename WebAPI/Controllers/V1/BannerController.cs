using Application.Features.Banners.Commands.CreateBanner;
using Application.Features.Banners.Commands.DeleteBanner;
using Application.Features.Banners.Commands.UpdateBanner;
using Application.Features.Banners.Queries.GetStoreBanners;
using Application.Features.Banners.Queries.GetBannerAuditLogs;
using Application.Features.Banners.Queries.GetBannersList;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Infrastructure.Authorization.Attribute;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý Banner &amp; Khuyến mãi (Marketing)
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý Banner &amp; Khuyến mãi (Marketing)")]
[Route("api/v{version:apiVersion}/banners")]
public class BannerController(ISender sender) : ApiController
{
    /// <summary>
    /// Thêm banner mới.
    /// </summary>
    /// <param name="command">Dữ liệu banner</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpPost]
    [HasPermission("Domain.Constants.Permission.Permissions.Banners.Create")]
    [SwaggerOperation(Summary = "Thêm banner mới")]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateBannerCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật banner
    /// </summary>
    /// <param name="id">Mã banner</param>
    /// <param name="request">Dữ liệu cập nhật</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPut("{id}")]
    [HasPermission(Banners.Edit)]
    [SwaggerOperation(Summary = "Cập nhật banner")]
    public async Task<IActionResult> UpdateAsync(
        int id,
        [FromBody] UpdateBannerCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateBannerCommand>() with { Id = id };
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa banner
    /// </summary>
    /// <param name="id">Mã banner</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("{id}")]
    [HasPermission(Banners.Delete)]
    [SwaggerOperation(Summary = "Xóa banner")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteBannerCommand(id), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }



    /// <summary>
    /// Lấy danh sách banner đang hoạt động
    /// </summary>
    /// <returns>Danh sách banner</returns>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpGet("store")]
    [SwaggerOperation(Summary = "Lấy danh sách banner cho cửa hàng (lọc theo vị trí)")]
    public async Task<IActionResult> GetStoreAsync([FromQuery] string? placement, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoreBannersQuery { Placement = placement }, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    [HttpGet("placements")]
    [SwaggerOperation(Summary = "Lấy danh sách vị trí banner hợp lệ")]
    public IActionResult GetPlacements()
    {
        var placements = Domain.Constants.BannerPlacements.PlacementLabels.Select(kvp => new { Value = kvp.Key, Label = kvp.Value });
        return Ok(new { Data = placements, Success = true });
    }

    /// <summary>
    /// Lấy toàn bộ danh sách banner (Dành cho quản lý)
    /// </summary>
    /// <returns>Danh sách banner</returns>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpGet]
    [HasPermission(Banners.View)]
    [SwaggerOperation(Summary = "Lấy toàn bộ danh sách banner")]
    public async Task<IActionResult> GetListAsync(
        [FromQuery] GetBannersListQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy lịch sử thay đổi của banner
    /// </summary>
    /// <param name="id">Mã banner</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Danh sách lịch sử</returns>
    [HttpGet("{id}/audit")]
    [HasPermission(Banners.View)]
    [SwaggerOperation(Summary = "Lấy lịch sử thay đổi của banner")]
    public async Task<IActionResult> GetAuditLogsAsync(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBannerAuditLogsQuery(id), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}

