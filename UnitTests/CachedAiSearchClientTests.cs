using Application.ApiContracts.Ai;
using Application.Interfaces.Repositories.Ai;
using FluentAssertions;
using Infrastructure.Services.Ai.Clients;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace UnitTests;

public class CachedAiSearchClientTests
{
    private static AiAgentResponse<AiSearchResult> SuccessResponse(string keyword) => new()
    {
        Status = "success",
        Result = new AiSearchResult { Keyword = keyword }
    };

    [Fact(DisplayName = "AICACHE_01 - Gọi 2 lần cùng keyword chỉ gọi AI Sidecar 1 lần")]
    public async Task ChatSearchAsync_SameKeywordTwice_CallsInnerOnce()
    {
        var innerMock = new Mock<IAiSearchClient>();
        innerMock.Setup(x => x.ChatSearchAsync("xe ga êm ái dễ lái", null)).ReturnsAsync(SuccessResponse("xe ga"));
        var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new CachedAiSearchClient(innerMock.Object, cache);

        var first = await sut.ChatSearchAsync("xe ga êm ái dễ lái", null);
        var second = await sut.ChatSearchAsync("xe ga êm ái dễ lái", null);

        first.Result!.Keyword.Should().Be("xe ga");
        second.Result!.Keyword.Should().Be("xe ga");
        innerMock.Verify(x => x.ChatSearchAsync(It.IsAny<string>(), It.IsAny<string?>()), Times.Once);
    }

    [Fact(DisplayName = "AICACHE_02 - Chuẩn hóa hoa/thường và khoảng trắng vẫn trúng cache")]
    public async Task ChatSearchAsync_SameKeywordDifferentCasing_StillHitsCache()
    {
        var innerMock = new Mock<IAiSearchClient>();
        innerMock.Setup(x => x.ChatSearchAsync(It.IsAny<string>(), null)).ReturnsAsync(SuccessResponse("xe ga"));
        var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new CachedAiSearchClient(innerMock.Object, cache);

        await sut.ChatSearchAsync("Xe Ga êm ái", null);
        await sut.ChatSearchAsync("  xe ga êm ái  ", null);

        innerMock.Verify(x => x.ChatSearchAsync(It.IsAny<string>(), It.IsAny<string?>()), Times.Once);
    }

    [Fact(DisplayName = "AICACHE_03 - Keyword khác nhau gọi AI Sidecar riêng biệt")]
    public async Task ChatSearchAsync_DifferentKeywords_CallsInnerForEach()
    {
        var innerMock = new Mock<IAiSearchClient>();
        innerMock.Setup(x => x.ChatSearchAsync(It.IsAny<string>(), null)).ReturnsAsync((string k, string? _) => SuccessResponse(k));
        var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new CachedAiSearchClient(innerMock.Object, cache);

        await sut.ChatSearchAsync("xe ga êm ái", null);
        await sut.ChatSearchAsync("xe số bền", null);

        innerMock.Verify(x => x.ChatSearchAsync(It.IsAny<string>(), It.IsAny<string?>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "AICACHE_04 - Kết quả lỗi từ AI Sidecar không được cache, lần sau vẫn thử gọi lại")]
    public async Task ChatSearchAsync_ErrorStatus_NotCached()
    {
        var innerMock = new Mock<IAiSearchClient>();
        innerMock.Setup(x => x.ChatSearchAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new AiAgentResponse<AiSearchResult> { Status = "error", Result = null });
        var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new CachedAiSearchClient(innerMock.Object, cache);

        await sut.ChatSearchAsync("xe khó tả quá trời luôn á", null);
        await sut.ChatSearchAsync("xe khó tả quá trời luôn á", null);

        innerMock.Verify(x => x.ChatSearchAsync(It.IsAny<string>(), It.IsAny<string?>()), Times.Exactly(2));
    }
}
