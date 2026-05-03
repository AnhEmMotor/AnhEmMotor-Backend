using Application.ApiContracts.Payment.Requests;
using Application.Features.Outputs.Queries.GetPaymentLink;
using Application.Features.Payments.Commands.ProcessPayOSCallback;
using Application.Features.Payments.Commands.ProcessPayOSWebhook;
using Application.Features.Payments.Commands.ProcessVNPayIPN;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers;

/// <summary>
/// Controller for handling payment-related operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Webhook endpoint for PayOS to notify payment status.
    /// </summary>
    /// <param name="data">Webhook data from PayOS.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Success response.</returns>
    [HttpPost("payos-webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookData data, CancellationToken cancellationToken)
    {
        await mediator.Send(new ProcessPayOSWebhookCommand(data), cancellationToken).ConfigureAwait(true);
        return Ok(new { success = true });
    }

    /// <summary>
    /// Generates a payment link for a specific order.
    /// </summary>
    /// <param name="orderId">The ID of the order.</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>The payment link data.</returns>
    [HttpGet("link/{orderId}")]
    [Authorize]
    public async Task<IActionResult> GetPaymentLink(int orderId, CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await mediator.Send(new GetPaymentLinkQuery(orderId, currentUserId), cancellationToken).ConfigureAwait(true);
        if (result.IsSuccess)
            return Ok(result.Value);
        var firstError = result.Errors?.FirstOrDefault() ?? result.Error;
        return BadRequest(
            new { success = false, message = firstError?.Message ?? "Lỗi không xác định", code = firstError?.Code });
    }

    /// <summary>
    /// Callback endpoint for VNPay after payment completion.
    /// </summary>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Redirects to the storefront processing page.</returns>
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
    /// Callback endpoint for PayOS after payment completion.
    /// </summary>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Redirects to the storefront processing page.</returns>
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
