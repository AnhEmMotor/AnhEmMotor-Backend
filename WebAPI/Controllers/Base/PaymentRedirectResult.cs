using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.Base;

/// <summary>
/// Custom IActionResult to handle redirecting to Frontend payment processing page based on Result.
/// </summary>
/// <typeparam name="T">Type of result value.</typeparam>
public class PaymentRedirectResult<T>(
    Result<T> result,
    string method,
    Func<T?, string?> getOrderId,
    string? fallbackOrderId,
    Func<T?, bool>? checkCustomSuccess = null) : IActionResult
{
    /// <summary>
    /// Executes the redirect action result.
    /// </summary>
    /// <param name="context">The action context.</param>
    public Task ExecuteResultAsync(ActionContext context)
    {
        if (result.IsFailure)
        {
            var error = result.Errors?.FirstOrDefault() ?? result.Error;
            if (string.Compare(error?.Code, "InvalidOrderCode") == 0)
            {
                var failRedirect = new RedirectResult("http://localhost:3000/orders?payment=failed");
                return failRedirect.ExecuteResultAsync(context);
            }
        }
        var orderId = result.IsSuccess && result.Value is not null ? getOrderId(result.Value) : fallbackOrderId;
        var isSuccess = result.IsSuccess && (checkCustomSuccess == null || checkCustomSuccess(result.Value));
        var status = isSuccess ? "success" : "failed";
        var redirectUrl = $"http://localhost:3000/payment-processing?id={orderId}&status={status}&method={method}";
        var redirectResult = new RedirectResult(redirectUrl);
        return redirectResult.ExecuteResultAsync(context);
    }
}
