using Application.ApiContracts.Payment.Requests;
using Application.Features.Outputs.Queries.GetPaymentLink;
using Application.Features.Payments.Commands.ProcessPayOSCallback;
using Application.Features.Payments.Commands.ProcessPayOSWebhook;
using Application.Features.Payments.Commands.ProcessVNPayIPN;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý các hoạt động thanh toán.
/// </summary>
[ApiController]
[SwaggerTag("Quản lý các hoạt động thanh toán")]
[Route("api/[controller]")]
public class PaymentController(ISender sender) : ApiController
{
    /// <summary>
    /// Webhook từ PayOS để thông báo trạng thái thanh toán.
    /// </summary>
    /// <param name="data">Dữ liệu Webhook từ PayOS.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả phản hồi.</returns>
    [HttpPost("payos-webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookData data, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ProcessPayOSWebhookCommand(data), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo link thanh toán cho một đơn hàng cụ thể.
    /// </summary>
    /// <param name="orderId">ID của đơn hàng.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Dữ liệu link thanh toán.</returns>
    [HttpGet("link/{orderId}")]
    [Authorize]
    public async Task<IActionResult> GetPaymentLink(int orderId, CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await sender.Send(new GetPaymentLinkQuery(orderId, currentUserId), cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Callback từ VNPay sau khi hoàn tất thanh toán.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Chuyển hướng về trang xử lý của Frontend.</returns>
    [HttpGet("vnpay-callback")]
    public async Task<IActionResult> VNPayCallback(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ProcessVNPayIPNCommand(Request.Query), cancellationToken)
            .ConfigureAwait(true);
        return HandlePaymentRedirect(
            result,
            method: "VNPay",
            getOrderId: val => val?.OrderId,
            fallbackOrderId: null,
            checkCustomSuccess: val => string.Compare(val?.VnPayResponseCode, "00") == 0);
    }

    /// <summary>
    /// Callback từ PayOS sau khi hoàn tất thanh toán.
    /// </summary>
    /// <param name="orderCode">Mã đơn hàng.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Chuyển hướng về trang xử lý của Frontend.</returns>
    [HttpGet("payos-callback")]
    public async Task<IActionResult> PayOSCallback([FromQuery] long? orderCode, CancellationToken cancellationToken)
    {
        var command = new ProcessPayOSCallbackCommand(orderCode);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandlePaymentRedirect(
            result,
            method: "PayOS",
            getOrderId: val => val.ToString(),
            fallbackOrderId: command.OrderId.ToString());
    }
}
