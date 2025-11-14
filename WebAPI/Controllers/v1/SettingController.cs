using Application.ApiContracts.Setting;
using Application.Interfaces.Services.Setting;
using Asp.Versioning;
using Domain.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.v1
{
    /// <summary>
    /// Quản lý cài đặt: cập nhật số lượng cảnh báo tồn kho, 
    /// </summary>
    /// <param name="settingService"></param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SettingController(ISettingService settingService) : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> SetSettings([FromBody] List<SetSettingItemRequest> request)
        {
            var errorResponse = await settingService.SetSettingsAsync(request);
            if (errorResponse != null)
            {
                return BadRequest(errorResponse);
            }
            return Ok();
        }
    }
}