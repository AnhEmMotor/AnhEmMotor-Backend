using Application.ApiContracts.Option.Responses;
using Application.Features.Options.Queries.GetOptionsList;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý các tùy chọn (Options) của sản phẩm.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý tùy chọn sản phẩm")]
[Route("api/v{version:apiVersion}/[controller]")]
public class OptionController(ISender sender) : ApiController
{
    /// <summary>
    /// Lấy danh sách tất cả các tùy chọn và giá trị của chúng.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<OptionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOptionsAsync(CancellationToken cancellationToken)
    {
        var query = new GetOptionsListQuery();
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}
