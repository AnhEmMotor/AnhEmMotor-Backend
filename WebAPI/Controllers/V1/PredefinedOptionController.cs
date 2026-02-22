using Application.Common.Models;
using Application.Features.PredefinedOptions.Queries.GetPredefinedOptionsList;
using Asp.Versioning;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;
using static Domain.Constants.Permission.PermissionsList;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý danh sách các thuộc tính sản phẩm được định nghĩa sẵn (ví dụ: Loại xe, Màu sắc).
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Danh sách thuộc tính sản phẩm được định nghĩa sẵn")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class PredefinedOptionController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách các thuộc tính được định nghĩa sẵn dưới dạng từ điển key-value. Chỉ người dùng có quyền tạo hoặc
    /// chỉnh sửa sản phẩm mới có thể truy cập.
    /// </summary>
    [HttpGet]
    [RequiresAnyPermissions(Products.View, Products.Create, Products.Edit, Products.Delete)]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPredefinedOptionsAsync(CancellationToken cancellationToken)
    {
        var query = new GetPredefinedOptionsListQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);

        return HandleResult(result);
    }
}
