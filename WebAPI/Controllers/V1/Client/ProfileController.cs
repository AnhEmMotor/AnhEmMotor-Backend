using Application.ApiContracts.Client.Profile;
using Application.Features.Client.Profile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.V1.Client;

/// <summary>
/// Quản lý hồ sơ cá nhân của khách hàng (Client Portal).
/// </summary>
[ApiController]
[Route("api/v1/client/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Lấy thông tin hồ sơ cá nhân của khách hàng đang đăng nhập.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _mediator.Send(new GetProfileQuery());
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật thông tin hồ sơ cá nhân (họ tên, SĐT, địa chỉ, ...).
    /// </summary>
    /// <param name="request">Thông tin cần cập nhật.</param>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var result = await _mediator.Send(new UpdateProfileCommand(request));
        return result ? Ok() : BadRequest();
    }
}
