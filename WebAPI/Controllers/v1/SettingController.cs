using Application.Common.Attributes;
using Application.Common.Models;
using Application.Features.Settings.Commands.SetSettings;
using Application.Features.Settings.Queries.GetAllSettings;
using Application.Features.Settings.Queries.GetStoreSettings;
using Asp.Versioning;
using Domain.Constants.Permission;
using Domain.Constants.RouteNames;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý cài đặt hệ thống: cập nhật số lượng cảnh báo tồn kho, số lượng mua tối đa, và các thông số tùy chỉnh khác.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý cài đặt hệ thống")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class SettingController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Cập nhật các cài đặt hệ thống (ví dụ: số lượng cảnh báo tồn kho, số lượng mua tối đa — valid keys được quy định
    /// sẵn).
    /// </summary>
    /// <param name="request">Từ điển các cài đặt cần cập nhật (key: tên setting, value: giá trị mới).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả cập nhật cài đặt hệ thống.</returns>
    /// <response code="200">Cập nhật cài đặt thành công.</response>
    /// <response code="400">Danh sách key không hợp lệ hoặc giá trị không đúng định dạng.</response>
    [HttpPut]
    [HasPermission(Permissions.Admin.SettingManagement.Edit)]
    [ProducesResponseType(typeof(Dictionary<string, string?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetSettingsAsync(
        [FromBody][ValidSettingKeys] Dictionary<string, string?> request,
        CancellationToken cancellationToken)
    {
        var command = new SetSettingsCommand() { Settings = request };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleCreated(result, Settings.GetAllSettings);
    }

    /// <summary>
    /// Lấy tất cả các cài đặt hệ thống hiện tại (với đầy đủ thông số dạng key-value).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Từ điển cài đặt hệ thống (key: tên setting, value: giá trị hiện tại).</returns>
    /// <response code="200">Trả về danh sách cài đặt thành công.</response>
    [HttpGet(Name = Settings.GetAllSettings)]
    [HasPermission(Permissions.Admin.SettingManagement.View)]
    [ProducesResponseType(typeof(Dictionary<string, long?>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSettingsAsync(CancellationToken cancellationToken)
    {
        var query = new GetAllSettingsQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy các cài đặt công khai dành cho Storefront (không cần quyền Admin — được dùng ở frontend).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Các cài đặt công khai (ví dụ: số lượng tối đa mua, giá tối thiểu).</returns>
    /// <response code="200">Trả về cài đặt công khai thành công.</response>
    [HttpGet("store")]
    [ProducesResponseType(typeof(Dictionary<string, string?>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStoreSettingsAsync(CancellationToken cancellationToken)
    {
        var query = new GetStoreSettingsQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
