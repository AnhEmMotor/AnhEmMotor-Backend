using Application.ApiContracts.Ai;
using Application.Interfaces.Repositories.Ai;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services.Ai.Clients;

/// <summary>
/// Cache kết quả AI Sidecar theo keyword đã chuẩn hóa, tránh trả phí LLM lần nữa cho các câu tìm kiếm trùng lặp mà
/// AiSearchRuleParser không xử lý được. Sidecar không cá nhân hóa theo userId cho search (xem
/// app/api/v1/search_products.py) nên cache có thể dùng chung giữa mọi user.
/// </summary>
public class CachedAiSearchClient(IAiSearchClient inner, IMemoryCache cache) : IAiSearchClient
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public async Task<AiAgentResponse<AiSearchResult>> ChatSearchAsync(string keyword, string? userId)
    {
        var cacheKey = $"AiSearch_Llm_{keyword.Trim().ToLowerInvariant()}";
        if (cache.TryGetValue(cacheKey, out AiAgentResponse<AiSearchResult>? cached))
            return cached!;
        var response = await inner.ChatSearchAsync(keyword, userId);
        if (response.Status == "success")
            cache.Set(cacheKey, response, CacheDuration);
        return response;
    }
}
