using Application.Common.Attributes;
using Application.Common.Models;
using Application.Features.Settings.Commands.SetSettings;
using Application.Features.Settings.Queries.GetAllSettings;
using Asp.Versioning;
using Domain.Constants;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;
using static Domain.Constants.Permission.PermissionsList;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý cài đặt hệ thống: cập nhật số lượng cảnh báo tồn kho, số lượng mua tối đa, ...
/// </summary>
/// <param name="mediator"></param>
[SwaggerTag("Quản lý cài đặt hệ thống: cập nhật số lượng cảnh báo tồn kho, số lượng mua tối đa, ...")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class SettingController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Sửa các cài đặt hệ thống (cập nhật số lượng cảnh báo tồn kho, số lượng mua tối đa, ...)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [HasPermission(Settings.Edit)]
    [ProducesResponseType(typeof(Dictionary<string, string?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetSettingsAsync(
        [FromBody][ValidSettingKeys] Dictionary<string, string?> request,
        CancellationToken cancellationToken)
    {
        var command = new SetSettingsCommand() { Settings = request };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(result, RouteNames.Settings.GetAllSettings);
    }

    /// <summary>
    /// Lấy các thông số cài đặt hệ thống (số lượng cảnh báo tồn kho, số lượng mua tối đa, ...)
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet(Name = RouteNames.Settings.GetAllSettings)]
    [HasPermission(Settings.View)]
    [ProducesResponseType(typeof(Dictionary<string, long?>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSettingsAsync(CancellationToken cancellationToken)
    {
        var query = new GetAllSettingsQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}
