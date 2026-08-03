using System.Net;
using Application.Interfaces.Services;
using Infrastructure.Services;
using Infrastructure.Services.Ai;
using Microsoft.Extensions.Configuration;
using Moq;

namespace UnitTests;

public class StoreChatAiClientTests
{
    private sealed class FakeHttpMessageHandler(string responseBody) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(responseBody) });
        }
    }

    private static StoreChatAiClient BuildClient(string sseBody)
    {
        var httpClient = new HttpClient(new FakeHttpMessageHandler(sseBody));
        var mockFactory = new Mock<IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
        var mockUrlProvider = new Mock<IAiSidecarUrlProvider>();
        mockUrlProvider.Setup(p => p.GetSidecarUrl()).Returns("http://fake-sidecar");
        var mockConfig = new Mock<IConfiguration>();
        return new StoreChatAiClient(mockFactory.Object, mockUrlProvider.Object, mockConfig.Object, new SystemServerDateProvider());
    }

    [Fact(DisplayName = "message_correction thay thế toàn bộ text_delta đã stream trước đó, không nối thêm")]
    public async Task GetReplyAsync_MessageCorrection_ReplacesAccumulatedText()
    {
        var sse = string.Join('\n', new[]
        {
            """{"type":"text_delta","payload":"Để tôi chuyển bạn sang nhân viên nhé."}""",
            """{"type":"message_correction","payload":""}""",
        });
        var client = BuildClient(sse);

        var result = await client.GetReplyAsync(Guid.NewGuid(), "Cho tôi gặp nhân viên", [], CancellationToken.None);

        Assert.Equal("", result.Text);
    }

    [Fact(DisplayName = "message_correction có nội dung sẽ thay bằng đúng nội dung đó (dùng cho check_output rewrite)")]
    public async Task GetReplyAsync_MessageCorrectionWithText_ReplacesWithCorrectionText()
    {
        var sse = string.Join('\n', new[]
        {
            """{"type":"text_delta","payload":"để tôi kiểm tra giúp bạn"}""",
            """{"type":"message_correction","payload":"Xin lỗi, để tôi tra cứu ngay."}""",
        });
        var client = BuildClient(sse);

        var result = await client.GetReplyAsync(Guid.NewGuid(), "Hi", [], CancellationToken.None);

        Assert.Equal("Xin lỗi, để tôi tra cứu ngay.", result.Text);
    }

    [Fact(DisplayName = "Không có message_correction thì text_delta tích luỹ bình thường")]
    public async Task GetReplyAsync_NoCorrection_AccumulatesTextDeltaNormally()
    {
        var sse = string.Join('\n', new[]
        {
            """{"type":"text_delta","payload":"Dạ shop "}""",
            """{"type":"text_delta","payload":"còn SH ạ"}""",
        });
        var client = BuildClient(sse);

        var result = await client.GetReplyAsync(Guid.NewGuid(), "Còn SH không?", [], CancellationToken.None);

        Assert.Equal("Dạ shop còn SH ạ", result.Text);
    }
}
