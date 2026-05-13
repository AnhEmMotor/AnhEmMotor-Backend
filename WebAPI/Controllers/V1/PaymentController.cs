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

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý các hoạt động thanh toán.
/// </summary>
[ApiController]
[SwaggerTag("Quản lý các hoạt động thanh toán")]
[Route("api/[controller]")]
public class PaymentController(IMediator mediator) : ControllerBase
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
        await mediator.Send(new ProcessPayOSWebhookCommand(data), cancellationToken).ConfigureAwait(true);
        return Ok(new { success = true });
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
        var result = await mediator.Send(new GetPaymentLinkQuery(orderId, currentUserId), cancellationToken)
            .ConfigureAwait(true);
        if (result.IsSuccess)
            return Ok(result.Value);
        var firstError = result.Errors?.FirstOrDefault() ?? result.Error;
        return BadRequest(
            new { success = false, message = firstError?.Message ?? "Lỗi không xác định", code = firstError?.Code });
    }

    /// <summary>
    /// Callback từ VNPay sau khi hoàn tất thanh toán.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Chuyển hướng về trang xử lý của Frontend.</returns>
    [HttpGet("vnpay-callback")]
    public async Task<IActionResult> VNPayCallback(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ProcessVNPayIPNCommand(Request.Query), cancellationToken)
            .ConfigureAwait(true);
        var orderId = result.Value?.OrderId;
        var status = result.IsSuccess && string.Compare(result.Value?.VnPayResponseCode, "00") == 0
            ? "success"
            : "failed";
        return Redirect($"http://localhost:3000/payment-processing?id={orderId}&status={status}&method=VNPay");
    }

    /// <summary>
    /// Callback từ PayOS sau khi hoàn tất thanh toán.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Chuyển hướng về trang xử lý của Frontend.</returns>
    [HttpGet("payos-callback")]
    public async Task<IActionResult> PayOSCallback(CancellationToken cancellationToken)
    {
        if (!long.TryParse(Request.Query["orderCode"], out var orderCode))
        {
            return Redirect("http://localhost:3000/orders?payment=failed");
        }
        var result = await mediator.Send(new ProcessPayOSCallbackCommand(orderCode), cancellationToken)
            .ConfigureAwait(true);
        var status = result.IsSuccess ? "success" : "failed";
        var orderId = (int)(orderCode / 100000);
        if (orderId == 0)
            orderId = (int)orderCode;
        return Redirect($"http://localhost:3000/payment-processing?id={orderId}&status={status}&method=PayOS");
    }
}
