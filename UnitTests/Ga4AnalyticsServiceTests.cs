using System.Net;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Models;
using Application.Common.Models.Ga4;
using FluentAssertions;
using Infrastructure.Configurations.Options;
using Infrastructure.Services.Analytics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace UnitTests;

public class Ga4AnalyticsServiceTests
{
    private const string MinuteResponse = """
        {
          "totals": [{ "metricValues": [{ "value": "7" }, { "value": "12" }] }],
          "rows": [
            { "dimensionValues": [{ "value": "2" }], "metricValues": [{ "value": "2" }, { "value": "4" }] },
            { "dimensionValues": [{ "value": "0" }], "metricValues": [{ "value": "3" }, { "value": "5" }] },
            { "dimensionValues": [{ "value": "1" }], "metricValues": [{ "value": "2" }, { "value": "3" }] }
          ]
        }
        """;

    private const string DeviceResponse = """
        {
          "rows": [
            { "dimensionValues": [{ "value": "MOBILE" }], "metricValues": [{ "value": "2" }, { "value": "3" }] },
            { "dimensionValues": [{ "value": "DESKTOP" }], "metricValues": [{ "value": "5" }, { "value": "9" }] }
          ]
        }
        """;

    private const string SourceResponse = """
        {
          "rows": [
            { "dimensionValues": [{ "value": "google" }], "metricValues": [{ "value": "4" }, { "value": "6" }] },
            { "dimensionValues": [{ "value": "facebook" }], "metricValues": [{ "value": "1" }, { "value": "2" }] }
          ]
        }
        """;

    private const string MinuteResponseNoTotals = """
        {
          "rows": [
            { "dimensionValues": [{ "value": "5" }], "metricValues": [{ "value": "1" }, { "value": "1" }] }
          ]
        }
        """;

    [Fact(DisplayName = "GA4SVC_01 - GetRealtimeAsync chưa cấu hình GA4 thì trả Failure")]
    public async Task GetRealtimeAsync_NotConfigured_ReturnsFailure()
    {
        var sut = CreateService(new FakeGa4Handler(), configured: false);

        var result = await sut.GetRealtimeAsync(CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("chưa được cấu hình");
    }

    [Fact(DisplayName = "GA4SVC_02 - GetRealtimeAsync happy path: đọc totals, sắp xếp byMinute tăng, byDevice/bySource giảm")]
    public async Task GetRealtimeAsync_HappyPath_ParsesTotalsAndSortsRows()
    {
        var handler = new FakeGa4Handler
        {
            RealtimeResponder = body => body switch
            {
                var b when b.Contains("\"minutesAgo\"") => MinuteResponse,
                var b when b.Contains("\"deviceCategory\"") => DeviceResponse,
                var b when b.Contains("\"sessionSource\"") => SourceResponse,
                _ => null
            }
        };
        var sut = CreateService(handler);

        var result = await sut.GetRealtimeAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ActiveUsers.Should().Be(7);
        result.Value.ScreenPageViews.Should().Be(12);
        result.Value.ByMinute.Select(r => r.Label).Should().Equal("0", "1", "2");
        result.Value.ByMinute.Sum(r => r.ActiveUsers).Should().Be(7);
        result.Value.ByDevice.Select(r => r.Label).Should().Equal("DESKTOP", "MOBILE");
        result.Value.BySource.Select(r => r.Label).Should().Equal("google", "facebook");
        handler.RealtimeCalls.Should().Be(3);
        handler.TokenCalls.Should().Be(1);
    }

    [Fact(DisplayName = "GA4SVC_03 - GetRealtimeAsync gọi lần 2 trong 60s dùng cache, không gọi lại GA4")]
    public async Task GetRealtimeAsync_SecondCallWithinCacheWindow_HitsGa4OnlyOnce()
    {
        var handler = new FakeGa4Handler
        {
            RealtimeResponder = body => body switch
            {
                var b when b.Contains("\"minutesAgo\"") => MinuteResponse,
                var b when b.Contains("\"deviceCategory\"") => DeviceResponse,
                var b when b.Contains("\"sessionSource\"") => SourceResponse,
                _ => null
            }
        };
        var sut = CreateService(handler);

        var first = await sut.GetRealtimeAsync(CancellationToken.None);
        var second = await sut.GetRealtimeAsync(CancellationToken.None);

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        second.Value.ActiveUsers.Should().Be(first.Value.ActiveUsers);
        handler.RealtimeCalls.Should().Be(3);
    }

    [Fact(DisplayName = "GA4SVC_04 - GetRealtimeAsync thiếu khối totals thì cộng từ phân rã thiết bị")]
    public async Task GetRealtimeAsync_MissingTotals_FallsBackToSumOfDeviceRows()
    {
        var handler = new FakeGa4Handler
        {
            RealtimeResponder = body => body switch
            {
                var b when b.Contains("\"minutesAgo\"") => MinuteResponseNoTotals,
                var b when b.Contains("\"deviceCategory\"") => DeviceResponse,
                var b when b.Contains("\"sessionSource\"") => SourceResponse,
                _ => null
            }
        };
        var sut = CreateService(handler);

        var result = await sut.GetRealtimeAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ActiveUsers.Should().Be(7);
        result.Value.ScreenPageViews.Should().Be(12);
    }

    [Fact(DisplayName = "GA4SVC_05 - GetRealtimeAsync chuỗi theo phút lỗi thì trả Failure")]
    public async Task GetRealtimeAsync_MinuteReportFails_ReturnsFailure()
    {
        var handler = new FakeGa4Handler { RealtimeResponder = _ => null };
        var sut = CreateService(handler);

        var result = await sut.GetRealtimeAsync(CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("403");
    }

    [Fact(DisplayName = "GA4SVC_05B - byMinute (minutesAgo) sort theo số, '10' đứng sau '2'")]
    public async Task GetRealtimeAsync_MinuteLabelsSortNumerically()
    {
        const string response = """
            {
              "totals": [{ "metricValues": [{ "value": "3" }, { "value": "3" }] }],
              "rows": [
                { "dimensionValues": [{ "value": "0" }], "metricValues": [{ "value": "1" }, { "value": "1" }] },
                { "dimensionValues": [{ "value": "10" }], "metricValues": [{ "value": "1" }, { "value": "1" }] },
                { "dimensionValues": [{ "value": "2" }], "metricValues": [{ "value": "1" }, { "value": "1" }] }
              ]
            }
            """;
        var handler = new FakeGa4Handler
        {
            RealtimeResponder = body => body switch
            {
                var b when b.Contains("\"minutesAgo\"") => response,
                var b when b.Contains("\"deviceCategory\"") => DeviceResponse,
                var b when b.Contains("\"sessionSource\"") => SourceResponse,
                _ => null
            }
        };
        var sut = CreateService(handler);

        var result = await sut.GetRealtimeAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ByMinute.Select(r => r.Label).Should().Equal("0", "2", "10");
    }

    [Fact(DisplayName = "GA4SVC_06 - BuildPayload với dimension pageTitle có orderBy sessions giảm dần")]
    public void BuildPayload_PageTitleDimension_HasSessionsOrderByDesc()
    {
        var payload = Ga4AnalyticsService.BuildPayload(
            new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 10), "pageTitle", 10);

        var dimensions = ((object[])payload["dimensions"]).Cast<Dictionary<string, string>>().ToArray();
        dimensions.Should().ContainSingle().Which["name"].Should().Be("pageTitle");

        var orderBys = ((object[])payload["orderBys"]).Cast<Dictionary<string, object>>().ToArray();
        var orderBy = orderBys.Should().ContainSingle().Subject;
        ((Dictionary<string, string>)orderBy["metric"])["metricName"].Should().Be("sessions");
        ((bool)orderBy["desc"]).Should().BeTrue();

        ((object[])payload["metrics"]).Should().HaveCount(8);
        ((int)payload["limit"]).Should().Be(10);
    }

    [Fact(DisplayName = "GA4SVC_07 - BuildPayload với dimension date giữ thứ tự thời gian, không orderBy")]
    public void BuildPayload_DateDimension_HasNoOrderBy()
    {
        var payload = Ga4AnalyticsService.BuildPayload(
            new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 10), "date", 400);

        payload.Should().NotContainKey("orderBys");
        ((object[])payload["dimensions"]).Cast<Dictionary<string, string>>()
            .Should().ContainSingle().Which["name"].Should().Be("date");
    }

    [Fact(DisplayName = "GA4SVC_08 - BuildPayload không dimension thì không có dimensions/orderBys")]
    public void BuildPayload_NoDimension_TotalOnly()
    {
        var payload = Ga4AnalyticsService.BuildPayload(
            new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 10), null, 1);

        payload.Should().NotContainKey("dimensions");
        payload.Should().NotContainKey("orderBys");
    }

    [Fact(DisplayName = "GA4SVC_09 - BuildRealtimePayload đủ metrics, minuteRanges 30 phút và dimension")]
    public void BuildRealtimePayload_IncludesMetricsMinuteRangesAndDimension()
    {
        var minuteRanges = new object[]
        {
            new Dictionary<string, int> { ["startMinutesAgo"] = 29, ["endMinutesAgo"] = 0 }
        };

        var payload = Ga4AnalyticsService.BuildRealtimePayload("deviceCategory", minuteRanges);

        var metrics = ((object[])payload["metrics"]).Cast<Dictionary<string, string>>().ToArray();
        metrics.Select(m => m["name"]).Should().Equal("activeUsers", "screenPageViews");
        payload["minuteRanges"].Should().BeSameAs(minuteRanges);

        var dimensions = ((object[])payload["dimensions"]).Cast<Dictionary<string, string>>().ToArray();
        dimensions.Should().ContainSingle().Which["name"].Should().Be("deviceCategory");
    }

    [Fact(DisplayName = "GA4SVC_10 - BuildRealtimePayload không dimension thì bỏ trống dimensions")]
    public void BuildRealtimePayload_NoDimension_OmitsDimensions()
    {
        var payload = Ga4AnalyticsService.BuildRealtimePayload(null, []);

        payload.Should().NotContainKey("dimensions");
        ((object[])payload["metrics"]).Should().HaveCount(2);
    }

    [Fact(DisplayName = "GA4SVC_11 - RunReportAsync chưa cấu hình thì trả Failure")]
    public async Task RunReportAsync_NotConfigured_ReturnsFailure()
    {
        var sut = CreateService(new FakeGa4Handler(), configured: false);

        var result = await sut.RunReportAsync(
            new Ga4ReportRequest { StartDate = new DateOnly(2026, 8, 1), EndDate = new DateOnly(2026, 8, 10) },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("chưa được cấu hình");
    }

    private static Ga4AnalyticsService CreateService(FakeGa4Handler handler, bool configured = true)
    {
        var options = new GoogleAnalytics4Options();
        if (configured)
        {
            options.PropertyId = "123456789";
            options.ServiceAccount["client_email"] = "ga4@test-project.iam.gserviceaccount.com";
            options.ServiceAccount["private_key"] = GeneratePrivateKeyPem();
        }

        return new Ga4AnalyticsService(
            Microsoft.Extensions.Options.Options.Create(options),
            new FakeHttpClientFactory(handler),
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<Ga4AnalyticsService>.Instance);
    }

    private static string GeneratePrivateKeyPem()
    {
        using var rsa = RSA.Create(2048);
        var base64 = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
        var lines = Enumerable.Range(0, (int)Math.Ceiling(base64.Length / 64.0))
            .Select(i => base64.Substring(i * 64, Math.Min(64, base64.Length - i * 64)));
        return "-----BEGIN PRIVATE KEY-----\n" + string.Join("\n", lines) + "\n-----END PRIVATE KEY-----";
    }

    private sealed class FakeHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class FakeGa4Handler : HttpMessageHandler
    {
        public int TokenCalls { get; private set; }

        public int RealtimeCalls { get; private set; }

        public int ReportCalls { get; private set; }

        public Func<string, string?>? RealtimeResponder { get; init; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var url = request.RequestUri!.ToString();
            if (url.Contains("oauth2.googleapis.com/token"))
            {
                TokenCalls++;
                return Json(HttpStatusCode.OK, """{"access_token":"test-token","expires_in":3600}""");
            }

            var body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            if (url.Contains(":runRealtimeReport"))
            {
                RealtimeCalls++;
                var payload = RealtimeResponder?.Invoke(body);
                return payload is null
                    ? Json(HttpStatusCode.Forbidden, """{"error":{"message":"forbidden"}}""")
                    : Json(HttpStatusCode.OK, payload);
            }

            ReportCalls++;
            return Json(HttpStatusCode.OK, """{"rowCount":0}""");
        }

        private static HttpResponseMessage Json(HttpStatusCode statusCode, string content) => new(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };
    }
}
