using Application.Interfaces.Services.Setting;
using Application.ValidationAttributes;
using Asp.Versioning;
using Domain.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.v1
{
    /// <summary>
    /// Quản lý cài đặt hệ thống: cập nhật số lượng cảnh báo tồn kho, số lượng mua tối đa
    /// </summary>
    /// <param name="settingService"></param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public class SettingController(ISettingService settingService) : ControllerBase
    {
        /// <summary>
        /// Sửa các cài đặt hệ thống (cập nhật số lượng cảnh báo tồn kho, số lượng mua tối đa, ...)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SetSettings([FromBody][ValidSettingKeys] Dictionary<string, long?> request)
        {
            var errorResponse = await settingService.SetSettingsAsync(request);
            if (errorResponse != null)
            {
                return BadRequest(errorResponse);
            }
            return Ok();
        }

        /// <summary>
        /// Lấy các thông số cài đặt hệ thống (số lượng cảnh báo tồn kho, số lượng mua tối đa, ...)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(Dictionary<string, long?>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllSettings()
        {
            var settings = await settingService.GetAllSettingsAsync();
            return Ok(settings);
        }
    }
}
