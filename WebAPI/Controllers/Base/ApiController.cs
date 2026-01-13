using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.Base;

/// <summary>
///
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    protected IActionResult HandleResult(Result result)
    {
        if(result.IsSuccess)
            return NoContent();
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
        if(result.IsSuccess)
            return Ok(result.Value);
        return MapErrorsToResponse(result);
    }

    private IActionResult MapErrorsToResponse(Result result)
    {
        var error = result.Errors?.FirstOrDefault() ?? result.Error;

        if(error is null)
        {
            return StatusCode(500, ErrorResponse.CreateProductionError("Unknown error occurred."));
        }

        var errorResponse = result.Errors is not null && result.Errors.Count > 0
            ? ErrorResponse.FromErrors(result.Errors)
            : ErrorResponse.FromError(error);

        if(result.Errors is not null && result.Errors.Count > 1)
        {
            return BadRequest(errorResponse);
        }

        return error.Code switch
        {
            "NotFound" => NotFound(errorResponse),
            "Unauthorized" => Unauthorized(errorResponse),
            "Forbidden" => StatusCode(403, errorResponse),
            "Conflict" => Conflict(errorResponse),
            _ => BadRequest(errorResponse)
        };
    }
}