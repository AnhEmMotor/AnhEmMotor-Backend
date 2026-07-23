using Application.Interfaces.Services.Shipping;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/shipping-location")]
[ApiController]
public class ShippingLocationController(IShippingService shippingService) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách Tỉnh/Thành phố từ Giao Hàng Nhanh.
    /// </summary>
    /// <remarks>
    /// ⚠️ CẢNH BÁO: API này chỉ dùng tạm thời để lấy danh sách Tỉnh/Phường trực tiếp từ đối tác vận chuyển thứ ba.  Cần
    /// được thiết kế lại và xoá khỏi controller này trong khoảng thời gian sớm nhất.
    /// </remarks>
    [HttpGet("provinces")]
    public async Task<IActionResult> GetProvinces(CancellationToken cancellationToken)
    {
        var result = await shippingService.GetProvincesAsync(cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    /// <summary>
    /// Lấy danh sách Quận/Huyện/Phường/Xã từ Giao Hàng Nhanh dựa trên ID Tỉnh/Thành phố.
    /// </summary>
    /// <remarks>
    /// ⚠️ CẢNH BÁO: API này chỉ dùng tạm thời để lấy danh sách Tỉnh/Phường trực tiếp từ đối tác vận chuyển thứ ba.  Cần
    /// được thiết kế lại và xoá khỏi controller này trong khoảng thời gian sớm nhất.
    /// </remarks>
    [HttpGet("wards/{provinceId}")]
    public async Task<IActionResult> GetWards(int provinceId, CancellationToken cancellationToken)
    {
        var result = await shippingService.GetWardsAsync(provinceId, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }
}
