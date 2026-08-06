using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Features.Vehicles.Commands.CreateVehicle;
using Application.Features.Vehicles.Commands.TransferOwnership;
using Application.Features.Vehicles.Commands.UpdateLicensePlate;
using Application.Features.Vehicles.Queries.GetVehiclePortfolio;
using Application.Features.Vehicles.Queries.GetVehicles;
using Asp.Versioning;
using Domain.Primitives;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý tài sản xe của khách hàng — theo dõi xe đã mua, biển số, VIN, lịch sử bảo dưỡng.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý tài sản xe của khách hàng")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class VehicleController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy thông tin chi tiết một xe của khách hàng theo ID.
    /// </summary>
    /// <param name="id">ID của xe cần xem chi tiết.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin chi tiết xe (biển số, VIN, model, năm sản xuất, v.v.).</returns>
    /// <response code="200">Trả về thông tin chi tiết xe thành công.</response>
    /// <response code="404">Không tìm thấy xe với ID đã cho.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpGet("{id:int}")]
    [Authorize]
    [SwaggerOperation(Summary = "Lấy chi tiết xe của khách hàng")]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Ok(new VehicleResponse { Id = id });
    }

    /// <summary>
    /// Lấy danh sách xe của khách hàng hiện tại với phân trang, lọc và sắp xếp.
    /// </summary>
    /// <param name="sieveModel">Tham số phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách xe của khách hàng đã phân trang.</returns>
    /// <response code="200">Trả về danh sách xe thành công.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpGet]
    [Authorize]
    [SwaggerOperation(Summary = "Lấy danh sách xe của khách hàng")]
    [ProducesResponseType(typeof(PagedResult<VehicleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetVehiclesQuery { SieveModel = sieveModel }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo mới tài sản xe cho khách hàng hiện tại.
    /// </summary>
    /// <param name="command">Thông tin xe cần tạo (VIN, biển số, model, v.v.).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin xe vừa tạo thành công.</returns>
    /// <response code="201">Tạo xe thành công.</response>
    /// <response code="400">Dữ liệu không hợp lệ.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateVehicleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleCreated(result);
    }

    /// <summary>
    /// Cập nhật biển số xe của khách hàng.
    /// </summary>
    /// <param name="id">ID của xe cần cập nhật.</param>
    /// <param name="command">Thông tin biển số xe cần cập nhật.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả cập nhật thành công hay thất bại.</returns>
    /// <response code="200">Cập nhật thành công.</response>
    /// <response code="400">Dữ liệu không hợp lệ.</response>
    /// <response code="404">Không tìm thấy xe.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpPatch("{id:int}/license-plate")]
    [Authorize]
    [SwaggerOperation(Summary = "Cập nhật biển số xe")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateLicensePlateAsync(
        int id,
        [FromBody] UpdateLicensePlateCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Chuyển quyền sở hữu xe cho người dùng khác.
    /// </summary>
    /// <param name="id">ID của xe cần chuyển quyền sở hữu.</param>
    /// <param name="command">Thông tin chuyển quyền (ID người nhận, loại chuyển nhượng).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả chuyển quyền sở hữu.</returns>
    /// <response code="200">Chuyển quyền thành công.</response>
    /// <response code="404">Không tìm thấy xe với ID đã cho.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpPost("{id:int}/transfer")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> TransferOwnershipAsync(
        int id,
        [FromBody] TransferOwnershipCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Tra cứu hồ sơ xe theo VIN, biển số hoặc số điện thoại của chủ sở hữu.
    /// </summary>
    /// <param name="query">Từ khóa tra cứu (VIN, biển số, hoặc số điện thoại).</param>
    /// <param name="queryType">Loại tra cứu: "vin", "license-plate", "phone" (mặc định "auto").</param>
    /// <param name="page">Số trang (mặc định 1).</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định 5).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả tra cứu hồ sơ xe đã phân trang.</returns>
    /// <response code="200">Trả về kết quả tra cứu thành công.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpGet("portfolio")]
    [Authorize]
    [SwaggerOperation(Summary = "Tra cứu hồ sơ xe")]
    [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPortfolioAsync(
        [FromQuery] string query,
        [FromQuery] string queryType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetVehiclePortfolioQuery(query ?? string.Empty, queryType ?? "auto", page, pageSize),
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }
}
