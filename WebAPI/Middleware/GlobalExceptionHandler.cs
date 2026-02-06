using Application.Common.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace WebAPI.Middleware;

/// <summary>
/// Provides a centralized exception handler for HTTP requests, generating standardized API error responses and logging
/// exceptions.
/// </summary>
/// <remarks>
/// This handler formats error responses as JSON and distinguishes between validation errors and unhandled exceptions.
/// In development environments, detailed exception information is included in the response; in production, a generic
/// error message is returned. All exceptions are logged appropriately based on their type.
/// </remarks>
/// <param name="environment">The hosting environment used to determine whether detailed error information should be included in responses.</param>
/// <param name="logger">The logger used to record exception details and validation failures.</param>
public partial class GlobalExceptionHandler(IWebHostEnvironment environment, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    [LoggerMessage(Level = LogLevel.Error, Message = "CRITICAL: An unhandled exception occurred. TraceId: {TraceId}")]
    private partial void LogUnhandledException(Exception exception, string traceId);

    /// <summary>
    /// Attempts to handle the specified exception by writing an appropriate error response to the HTTP context
    /// asynchronously.
    /// </summary>
    /// <remarks>
    /// If the exception is a validation error, a detailed validation error response is returned with status code 400
    /// (Bad Request). For other exceptions, a generic error response is returned with status code 500 (Internal Server
    /// Error). The response format and content type are set to JSON. The method does not propagate the exception
    /// further.
    /// </remarks>
    /// <param name="httpContext">The HTTP context to which the error response will be written. Must not be null.</param>
    /// <param name="exception">The exception to handle and convert into an error response. Must not be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A value indicating whether the exception was handled and an error response was written. Always returns <see
    /// langword="true"/>.
    /// </returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        LogUnhandledException(exception, httpContext.TraceIdentifier);

        var errorResponse = environment.IsDevelopment() || environment.IsEnvironment("Test")
            ? ErrorResponse.CreateDevelopmentError(exception)
            : ErrorResponse.CreateProductionError("An unexpected system error occurred. Please contact support.");

        httpContext.Response.ContentType = "application/json";

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken).ConfigureAwait(false);

        return true;
    }
}