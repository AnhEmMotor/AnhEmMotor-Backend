using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.Base;

/// <summary>
/// 
/// </summary>
[ApiController] // Tự động validate model state và apply attribute cho class con
[Route("api/[controller]")] // Route mặc định, có thể override
public abstract class ApiController : ControllerBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess) return NoContent();
        return MapErrorsToResponse(result);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <returns></returns>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess) return Ok(result.Value);
        return MapErrorsToResponse(result);
    }

    private IActionResult MapErrorsToResponse(Result result)
    {
        // 1. Xác định lỗi chủ đạo (Primary Error)
        // Nếu có danh sách lỗi, lấy cái đầu tiên làm đại diện để check type,
        // HOẶC mặc định là BadRequest nếu có nhiều lỗi.
        var error = result.Errors?.FirstOrDefault() ?? result.Error;

        if (error is null)
        {
            // Fallback an toàn: Logic code sai ở đâu đó nên mới ra Result Failure mà không có Error nào
            return StatusCode(500, ErrorResponse.CreateProductionError("Unknown error occurred."));
        }

        // 2. Chuẩn bị payload trả về
        var errorResponse = result.Errors is not null && result.Errors.Count > 0
            ? ErrorResponse.FromErrors(result.Errors)
            : ErrorResponse.FromError(error);

        // 3. Quyết định Status Code dựa trên Error Code chủ đạo
        // Nếu có nhiều lỗi (Errors.Count > 0), logic thông thường là trả về 400 (Bad Request)
        // trừ khi bạn muốn xử lý đặc biệt.
        if (result.Errors is not null && result.Errors.Count > 1)
        {
            return BadRequest(errorResponse);
        }

        return error.Code switch
        {
            "NotFound" => NotFound(errorResponse),
            "Unauthorized" => Unauthorized(errorResponse),
            "Forbidden" => StatusCode(403, errorResponse), // Dùng StatusCode rõ ràng hơn Forbid()
            "Conflict" => Conflict(errorResponse),
            // Mọi lỗi khác (Validation, BadRequest, Failure...) đều về 400
            _ => BadRequest(errorResponse)
        };
    }
}