using Application.Common.Models;
using Application.Features.PredefinedOptions.Queries.GetPredefinedOptionsList;
using Asp.Versioning;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

    /// <summary>
    /// Lấy danh sách ánh xạ trạng thái tồn kho (key tiếng Anh - nhãn tiếng Việt) để Frontend binding.
    /// </summary>
    /// <example>
    /// <code> GET /api/v1/PredefinedOption/inventory-statuses Response: [{ "key": "OutOfStock", "label": "Hết hàng" },
    /// ...]</code>
    /// </example>
    [HttpGet("inventory-statuses")]
    [RequiresAnyPermissions(Products.View, Products.Create, Products.Edit, Products.Delete)]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public IActionResult GetInventoryStatuses()
    {
        var statuses = new[]
        {
            new { key = nameof(Domain.Constants.InventoryStatus.OutOfStock), label = "Hết hàng" },
            new { key = nameof(Domain.Constants.InventoryStatus.LowStock), label = "Sắp hết hàng" },
            new { key = nameof(Domain.Constants.InventoryStatus.InStock), label = "Còn hàng" }
        };

        return Ok(statuses);
    }

    /// <summary>
    /// Lấy danh sách ánh xạ giới tính (key tiếng Anh - nhãn tiếng Việt) để Frontend binding.
    /// </summary>
    [HttpGet("gender-options")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public IActionResult GetGenderOptions()
    {
        var options = new[]
        {
            new { key = nameof(Domain.Constants.GenderStatus.Male), label = "Nam" },
            new { key = nameof(Domain.Constants.GenderStatus.Female), label = "Nữ" },
            new { key = nameof(Domain.Constants.GenderStatus.Other), label = "Khác" }
        };

        return Ok(options);
    }
}
