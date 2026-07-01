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
/// Quản lý các tùy chọn (Options) của sản phẩm.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý tùy chọn sản phẩm")]
[Route("api/v{version:apiVersion}/[controller]")]
public class OptionController(ISender sender) : ApiController
{
    /// <summary>
    /// Lấy danh sách tất cả các tùy chọn và giá trị của chúng (Public).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<OptionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOptionsAsync(CancellationToken cancellationToken)
    {
        var query = new GetOptionsListQuery();
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách toàn bộ các thuộc tính (Options) và các giá trị của chúng (Dành cho Quản trị viên).
    /// </summary>
    [HttpGet("all")]
    [RequiresAnyPermissions(Permissions.Warehouse.ProductManagement.View, Permissions.Order.ProductManagement.View, Permissions.Warehouse.ProductManagement.Create, Permissions.Order.ProductManagement.Create, Permissions.Warehouse.ProductManagement.Edit, Permissions.Order.ProductManagement.Edit, Permissions.Warehouse.ProductManagement.Delete, Permissions.Order.ProductManagement.Delete)]
    [ProducesResponseType(typeof(List<OptionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOptionsAsync(CancellationToken cancellationToken)
    {
        var query = new GetOptionsListQuery();
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách các thuộc tính được định nghĩa sẵn dưới dạng từ điển key-value.
    /// </summary>
    [HttpGet("predefined")]
    [RequiresAnyPermissions(Permissions.Warehouse.ProductManagement.View, Permissions.Order.ProductManagement.View, Permissions.Warehouse.ProductManagement.Create, Permissions.Order.ProductManagement.Create, Permissions.Warehouse.ProductManagement.Edit, Permissions.Order.ProductManagement.Edit, Permissions.Warehouse.ProductManagement.Delete, Permissions.Order.ProductManagement.Delete)]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPredefinedOptionsAsync(CancellationToken cancellationToken)
    {
        var query = new GetPredefinedOptionsListQuery();
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo mới giá trị thuộc tính.
    /// </summary>
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
    /// Cập nhật giá trị thuộc tính.
    /// </summary>
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
    /// Xoá giá trị thuộc tính.
    /// </summary>
    [HttpDelete("values/{id:int}")]
    [RequiresAnyPermissions(Permissions.Warehouse.ProductManagement.Delete, Permissions.Order.ProductManagement.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteOptionValueAsync(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteOptionValueCommand(id), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}

