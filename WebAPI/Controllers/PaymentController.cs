using Application.Features.Outputs.Queries.GetPaymentLink;
using Application.Features.Payments.Commands.ProcessPayOSCallback;
using Application.Features.Payments.Commands.ProcessPayOSWebhook;
using Application.Features.Payments.Commands.ProcessVNPayIPN;
using Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController(IMediator mediator) : ControllerBase
{
    [HttpPost("payos-webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookData data)
    {
        await mediator.Send(new ProcessPayOSWebhookCommand(data));
        return Ok(new { success = true });
    }
    [HttpGet("link/{orderId}")]
    public async Task<IActionResult> GetPaymentLink(int orderId)
    {
        var result = await mediator.Send(new GetPaymentLinkQuery(orderId));
        if (result.IsSuccess) return Ok(result.Value);
        
        var firstError = result.Errors?.FirstOrDefault() ?? result.Error;
        return BadRequest(new { 
            success = false, 
            message = firstError?.Message ?? "Lỗi không xác định",
            code = firstError?.Code
        });
    }

    [HttpGet("vnpay-callback")]
    public async Task<IActionResult> VNPayCallback()
    {
        var result = await mediator.Send(new ProcessVNPayIPNCommand(Request.Query));
        
        // Redirect to storefront processing page
        var orderId = result.Value?.OrderId;
        var status = result.IsSuccess && result.Value?.VnPayResponseCode == "00" ? "success" : "failed";
        
        return Redirect($"http://localhost:3000/payment-processing?id={orderId}&status={status}&method=VNPay");
    }

    [HttpGet("payos-callback")]
    public async Task<IActionResult> PayOSCallback()
    {
        if (!long.TryParse(Request.Query["orderCode"], out var orderCode))
        {
            return Redirect("http://localhost:3000/orders?payment=failed");
        }

        var result = await mediator.Send(new ProcessPayOSCallbackCommand(orderCode));
        var status = result.IsSuccess ? "success" : "failed";
        
        // Extract orderId safely from orderCode for redirection
        var orderId = (int)(orderCode / 100000);
        if (orderId == 0) orderId = (int)orderCode;

        return Redirect($"http://localhost:3000/payment-processing?id={orderId}&status={status}&method=PayOS");
    }
}
