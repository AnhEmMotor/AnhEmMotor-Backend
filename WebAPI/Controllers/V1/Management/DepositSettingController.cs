using Application.ApiContracts.DepositSetting.Requests;
using Application.Features.DepositSettings.Commands;
using Application.Features.DepositSettings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.V1.Management;

[ApiController]
[Route("api/v1/[controller]")]
// [Authorize]
public class DepositSettingController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var result = await _mediator.Send(new GetDepositSettingsQuery());
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var result = await _mediator.Send(new GetDepositSettingsHistoryQuery());
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateDepositSettingRequest request)
    {
        await _mediator.Send(new UpdateDepositSettingCommand(request));
        return Ok();
    }
}
