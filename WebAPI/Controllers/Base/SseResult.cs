using Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WebAPI.Controllers.Base;

/// <summary>
/// Custom IActionResult to push Server-Sent Events (SSE) stream to the client.
/// </summary>
/// <typeparam name="T">The type of the streamed content.</typeparam>
/// <param name="stream">The source async enumerable stream.</param>
public class SseResult<T>(IAsyncEnumerable<Result<T>> stream) : IActionResult
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Executes the result by writing the stream chunks to the response body in SSE format.
    /// </summary>
    /// <param name="context">The action context.</param>
    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        var cancellationToken = context.HttpContext.RequestAborted;

        response.Headers.Append("Content-Type", "text/event-stream");
        response.Headers.Append("Cache-Control", "no-cache");
        response.Headers.Append("X-Accel-Buffering", "no");

        try
        {
            await foreach (var result in stream.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (result.IsSuccess)
                {
                    var json = JsonSerializer.Serialize(result.Value, JsonSerializerOptions);
                    await response.WriteAsync($"data: {json}\n\n", cancellationToken).ConfigureAwait(true);
                }
                else
                {
                    var errorJson = JsonSerializer.Serialize(result.Error, JsonSerializerOptions);
                    await response.WriteAsync($"event: error\ndata: {errorJson}\n\n", cancellationToken).ConfigureAwait(true);
                }
                await response.Body.FlushAsync(cancellationToken).ConfigureAwait(true);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
