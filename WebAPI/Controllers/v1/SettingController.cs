using Application.Common.Attributes;
using Application.Features.Settings.Commands.SetSettings;
using Application.Features.Settings.Queries.GetAllSettings;
using Asp.Versioning;
using Domain.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý cài đặt hệ thống: cập nhật số lượng cảnh báo tồn kho, số lượng mua tối đa, ...
/// </summary>
/// <param name="mediator"></param>
[ApiController]
[SwaggerTag("Quản lý cài đặt hệ thống: cập nhật số lượng cảnh báo tồn kho, số lượng mua tối đa, ...")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class SettingController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Sửa các cài đặt hệ thống (cập nhật số lượng cảnh báo tồn kho, số lượng mua tối đa, ...)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [ProducesResponseType(typeof(Dictionary<string, long?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetSettings(
        [FromBody][ValidSettingKeys] Dictionary<string, long?> request,
        CancellationToken cancellationToken)
    {
        var command = new SetSettingsCommand(request);
        var (data, errorResponse) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(errorResponse != null)
        {
            return BadRequest(errorResponse);
        }
        return Ok(data);
    }

    /// <summary>
    /// Lấy các thông số cài đặt hệ thống (số lượng cảnh báo tồn kho, số lượng mua tối đa, ...)
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Dictionary<string, long?>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSettings(CancellationToken cancellationToken)
    {
        var query = new GetAllSettingsQuery();
        var settings = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(settings);
    }
}
