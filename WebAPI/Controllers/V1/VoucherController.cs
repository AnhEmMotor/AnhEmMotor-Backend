using Application.ApiContracts.Voucher.Requests;
using Application.Common.Models;
using Application.Features.Vouchers.Commands.ApplyVoucher;
using Application.Features.Vouchers.Commands.CreateVoucher;
using Application.Features.Vouchers.Commands.DeleteVoucher;
using Application.Features.Vouchers.Commands.RemoveVoucher;
using Application.Features.Vouchers.Commands.UpdateVoucher;
using Application.Features.Vouchers.Queries.GetVoucherByCode;
using Application.Features.Vouchers.Queries.GetVoucherById;
using Application.Features.Vouchers.Queries.GetVoucherList;
using Application.Features.Vouchers.Queries.ValidateVoucher;
using Asp.Versioning;
using Domain.Primitives;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý voucher (mã giảm giá) — danh sách, tạo, cập nhật, xóa, kiểm tra hợp lệ, áp dụng vào đơn hàng và xóa voucher
/// đã áp dụng.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý voucher")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class VoucherController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách voucher với phân trang, lọc, sắp xếp theo quy tắc Sieve.
    /// </summary>
    /// <param name="request">Tham số truy vấn (theo quy tắc Sieve: trang, lọc, sắp xếp).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách voucher đã phân trang.</returns>
    /// <response code="200">Trả về danh sách voucher thành công.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetVouchers(
        [FromQuery] GetVouchersRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetVouchersQuery { Request = request }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy chi tiết một voucher theo ID.
    /// </summary>
    /// <param name="id">ID của voucher cần xem chi tiết.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin chi tiết voucher (mã, loại, giá trị, điều kiện áp dụng, thời hạn, số lượng đã dùng).</returns>
    /// <response code="200">Trả về chi tiết voucher thành công.</response>
    /// <response code="404">Không tìm thấy voucher.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetVoucherById(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetVoucherByIdQuery(id), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy chi tiết một voucher theo mã (code).
    /// </summary>
    /// <param name="code">Mã của voucher cần lấy.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin chi tiết voucher.</returns>
    /// <response code="200">Trả về chi tiết voucher thành công.</response>
    /// <response code="404">Không tìm thấy voucher.</response>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVoucherByCode(string code, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetVoucherByCodeQuery(code), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo mới một voucher (mã giảm giá) với các điều kiện áp dụng.
    /// </summary>
    /// <param name="request">Thông tin voucher cần tạo (mã, loại, giá trị, điều kiện, ngày hết hạn, số lượng).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Voucher vừa được tạo thành công.</returns>
    /// <response code="200">Tạo voucher thành công.</response>
    /// <response code="400">Dữ liệu không hợp lệ hoặc mã voucher đã tồn tại.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateVoucher(
        [FromBody] CreateVoucherRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateVoucherCommand { Request = request }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật thông tin một voucher đã có.
    /// </summary>
    /// <param name="id">ID của voucher cần cập nhật.</param>
    /// <param name="request">Thông tin cập nhật voucher.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Voucher sau cập nhật.</returns>
    /// <response code="200">Cập nhật voucher thành công.</response>
    /// <response code="400">ID không khớp hoặc dữ liệu không hợp lệ.</response>
    /// <response code="404">Không tìm thấy voucher.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateVoucher(
        int id,
        [FromBody] UpdateVoucherRequest request,
        CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest("Id không hợp lệ");
        var result = await mediator.Send(new UpdateVoucherCommand { Request = request }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa một voucher khỏi hệ thống (soft-delete).
    /// </summary>
    /// <param name="id">ID của voucher cần xóa.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả xóa voucher.</returns>
    /// <response code="200">Xóa voucher thành công.</response>
    /// <response code="404">Không tìm thấy voucher.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteVoucher(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteVoucherCommand(id), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Kiểm tra voucher có hợp lệ cho đơn hàng cụ thể hay không (trước khi áp dụng).
    /// </summary>
    /// <param name="request">Thông tin kiểm tra (ID voucher, ID đơn hàng).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả kiểm tra tính hợp lệ của voucher.</returns>
    /// <response code="200">Trả về kết quả kiểm tra thành công.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpPost("validate")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ValidateVoucher(
        [FromBody] VoucherValidateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new VoucherValidateQuery(request.VoucherId, request.OutputId, request.OrderTotal),
            cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Áp dụng voucher vào một đơn hàng (tính toán giảm giá, ghi nhận voucher đã sử dụng).
    /// </summary>
    /// <param name="request">Thông tin áp dụng (ID voucher, ID đơn hàng).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả áp dụng voucher (số tiền giảm, tổng tiền sau giảm).</returns>
    /// <response code="200">Áp dụng voucher thành công.</response>
    /// <response code="400">Voucher không hợp lệ, đã hết hạn, hoặc không đủ điều kiện áp dụng.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpPost("apply")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ApplyVoucher(
        [FromBody] ApplyVoucherRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var result = await mediator.Send(
            new ApplyVoucherCommand
            {
                VoucherId = request.VoucherId,
                OutputId = request.OutputId,
                CurrentUserId = currentUserId
            },
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa voucher đã áp dụng khỏi đơn hàng (hoàn tác việc áp dụng voucher).
    /// </summary>
    /// <param name="orderVoucherId">ID của bản ghi voucher đã áp dụng trên đơn hàng (OrderVoucher ID).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả xóa voucher đã áp dụng.</returns>
    /// <response code="200">Xóa voucher đã áp dụng thành công.</response>
    /// <response code="404">Không tìm thấy bản ghi voucher đã áp dụng.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpDelete("apply/{orderVoucherId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveVoucher(int orderVoucherId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var result = await mediator.Send(new RemoveVoucherCommand(orderVoucherId, currentUserId), cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(userIdStr) ? Guid.Empty : Guid.Parse(userIdStr);
    }
}
