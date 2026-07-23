using Application.ApiContracts.Option.Responses;
using Application.Common.Models;
using Application.Features.Options.Queries.GetOptionsList;
using Application.Features.OptionValues.Commands.CreateOptionValue;
using Application.Features.OptionValues.Commands.DeleteOptionValue;
using Application.Features.OptionValues.Commands.UpdateOptionValue;
using Application.Features.PredefinedOptions.Queries.GetPredefinedOptionsList;
using Asp.Versioning;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý các tùy chọn (Options) của sản phẩm — thuộc tính như màu sắc, kích thước, dung lượng, v.v.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý tùy chọn sản phẩm")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class OptionController(ISender sender) : ApiController
{
    /// <summary>
    /// Lấy danh sách tất cả các tùy chọn và các giá trị của chúng (không yêu cầu quyền — dùng chung).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách tất cả các tùy chọn (Option) và giá trị tương ứng.</returns>
    /// <response code="200">Trả về danh sách tùy chọn thành công.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<OptionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOptionsAsync(CancellationToken cancellationToken)
    {
        var query = new GetOptionsListQuery();
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách toàn bộ các thuộc tính (Options) và giá trị của chúng (dành cho Quản trị viên, có phân quyền chi
    /// tiết).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách đầy đủ các tùy chọn và giá trị.</returns>
    /// <response code="200">Trả về danh sách tùy chọn thành công.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpGet("all")]
    [RequiresAnyPermissions(
        Permissions.Warehouse.ProductManagement.View,
        Permissions.Order.ProductManagement.View,
        Permissions.Warehouse.ProductManagement.Create,
        Permissions.Order.ProductManagement.Create,
        Permissions.Warehouse.ProductManagement.Edit,
        Permissions.Order.ProductManagement.Edit,
        Permissions.Warehouse.ProductManagement.Delete,
        Permissions.Order.ProductManagement.Delete)]
    [ProducesResponseType(typeof(List<OptionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllOptionsAsync(CancellationToken cancellationToken)
    {
        var query = new GetOptionsListQuery();
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách các thuộc tính được định nghĩa sẵn dưới dạng từ điển key-value (ví dụ: màu sắc, chất liệu).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách các option được định nghĩa sẵn (từ điển key → label).</returns>
    /// <response code="200">Trả về danh sách option định nghĩa sẵn thành công.</response>
    /// <response code="403">Không có quyền truy cập.</response>
    [HttpGet("predefined")]
    [RequiresAnyPermissions(
        Permissions.Warehouse.ProductManagement.View,
        Permissions.Order.ProductManagement.View,
        Permissions.Warehouse.ProductManagement.Create,
        Permissions.Order.ProductManagement.Create,
        Permissions.Warehouse.ProductManagement.Edit,
        Permissions.Order.ProductManagement.Edit,
        Permissions.Warehouse.ProductManagement.Delete,
        Permissions.Order.ProductManagement.Delete)]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPredefinedOptionsAsync(CancellationToken cancellationToken)
    {
        var query = new GetPredefinedOptionsListQuery();
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo mới một giá trị thuộc tính (Option Value) cho một Option đã có.
    /// </summary>
    /// <param name="request">Thông tin giá trị thuộc tính mới (tên, giá trị, thứ tự hiển thị).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>ID của giá trị thuộc tính vừa tạo.</returns>
    /// <response code="200">Tạo giá trị thuộc tính thành công.</response>
    [HttpPost("values")]
    [RequiresAnyPermissions(Permissions.Warehouse.ProductManagement.Create, Permissions.Order.ProductManagement.Create)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateOptionValueAsync(
        [FromBody] CreateOptionValueCommand request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật thông tin một giá trị thuộc tính (tên, giá trị, thứ tự hiển thị).
    /// </summary>
    /// <param name="id">ID của giá trị thuộc tính cần cập nhật.</param>
    /// <param name="request">Thông tin cập nhật (tên, giá trị, thứ tự hiển thị).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Không có nội dung trả về (204 No Content).</returns>
    /// <response code="204">Cập nhật giá trị thuộc tính thành công.</response>
    [HttpPut("values/{id:int}")]
    [RequiresAnyPermissions(Permissions.Warehouse.ProductManagement.Edit, Permissions.Order.ProductManagement.Edit)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateOptionValueAsync(
        int id,
        [FromBody] UpdateOptionValueCommand request,
        CancellationToken cancellationToken)
    {
        var command = request with { Id = id };
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Xoá một giá trị thuộc tính khỏi hệ thống.
    /// </summary>
    /// <param name="id">ID của giá trị thuộc tính cần xoá.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Không có nội dung trả về (204 No Content).</returns>
    /// <response code="204">Xoá giá trị thuộc tính thành công.</response>
    /// <response code="400">Không thể xoá (giá trị đang được sản phẩm sử dụng).</response>
    /// <response code="404">Không tìm thấy giá trị thuộc tính.</response>
    [HttpDelete("values/{id:int}")]
    [RequiresAnyPermissions(Permissions.Warehouse.ProductManagement.Delete, Permissions.Order.ProductManagement.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOptionValueAsync(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteOptionValueCommand(id), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
