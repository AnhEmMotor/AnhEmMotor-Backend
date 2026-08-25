using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Ga4;
using Application.Interfaces.Services.Analytics;
using FluentAssertions;
using Infrastructure.Configurations.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class GoogleAnalytics
{
    private static readonly DateTimeOffset FixedNow = new(2026, 8, 25, 10, 0, 0, TimeSpan.FromHours(7));

    private readonly Mock<IGa4AnalyticsService> _ga4Mock = new();
    private readonly Mock<IGa4MeasurementProtocolService> _measurementMock = new();
    private readonly Mock<IServerDateProvider> _dateMock = new();
    private readonly GoogleAnalyticsController _controller;

    public GoogleAnalytics()
    {
        _dateMock.Setup(d => d.VietnamNow).Returns(FixedNow);
        var options = Options.Create(new GoogleAnalytics4Options { PropertyId = "123456789" });
        _controller = new GoogleAnalyticsController(
            options,
            _ga4Mock.Object,
            _measurementMock.Object,
            _dateMock.Object);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
    }

    [Fact(DisplayName = "GACTRL_01 - GetTopPageTitles chưa cấu hình GA4 thì trả Ok với report rỗng")]
    public async Task GetTopPageTitles_NotConfigured_ReturnsOkEmptyReport()
    {
        _ga4Mock.Setup(s => s.IsConfigured()).Returns(false);

        var result = await _controller.GetTopPageTitles(null, null, 10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var report = ok.Value.Should().BeOfType<Ga4ReportDto<Ga4DimensionRowDto>>().Subject;
        report.Rows.Should().BeEmpty();
        _ga4Mock.Verify(s => s.GetTopPageTitlesAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GACTRL_02 - GetTopPageTitles limit vượt 25 bị clamp về 25 và truyền đúng khoảng ngày")]
    public async Task GetTopPageTitles_LimitAbove25_ClampedAndPassesRange()
    {
        _ga4Mock.Setup(s => s.IsConfigured()).Returns(true);
        _ga4Mock
            .Setup(s => s.GetTopPageTitlesAsync(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 10), 25, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessReport(("Trang chủ", 100)));

        var result = await _controller.GetTopPageTitles(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 10), 100, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var report = ok.Value.Should().BeAssignableTo<Ga4ReportDto<Ga4DimensionRowDto>>().Subject;
        report.Rows.Should().ContainSingle().Which.Label.Should().Be("Trang chủ");
        _ga4Mock.Verify(
            s => s.GetTopPageTitlesAsync(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 10), 25, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "GACTRL_03 - GetTopPageTitles không truyền ngày thì mặc định 30 ngày về trước")]
    public async Task GetTopPageTitles_NoDates_DefaultsToLast30Days()
    {
        _ga4Mock.Setup(s => s.IsConfigured()).Returns(true);
        _ga4Mock
            .Setup(s => s.GetTopPageTitlesAsync(new DateOnly(2026, 7, 27), new DateOnly(2026, 8, 25), 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessReport());

        await _controller.GetTopPageTitles(null, null, 10, CancellationToken.None);

        _ga4Mock.Verify(
            s => s.GetTopPageTitlesAsync(new DateOnly(2026, 7, 27), new DateOnly(2026, 8, 25), 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "GACTRL_04 - GetOperatingSystemBreakdown chưa cấu hình thì trả Ok rỗng")]
    public async Task GetOperatingSystemBreakdown_NotConfigured_ReturnsOkEmpty()
    {
        _ga4Mock.Setup(s => s.IsConfigured()).Returns(false);

        var result = await _controller.GetOperatingSystemBreakdown(null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<Ga4ReportDto<Ga4DimensionRowDto>>();
        _ga4Mock.Verify(s => s.GetOperatingSystemBreakdownAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GACTRL_05 - GetOperatingSystemBreakdown cấu hình rồi thì trả rows theo hệ điều hành")]
    public async Task GetOperatingSystemBreakdown_Configured_ReturnsRows()
    {
        _ga4Mock.Setup(s => s.IsConfigured()).Returns(true);
        _ga4Mock
            .Setup(s => s.GetOperatingSystemBreakdownAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessReport(("Windows", 71), ("iOS", 50), ("Android", 20)));

        var result = await _controller.GetOperatingSystemBreakdown(null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var report = ok.Value.Should().BeAssignableTo<Ga4ReportDto<Ga4DimensionRowDto>>().Subject;
        report.Rows.Select(r => r.Label).Should().Equal("Windows", "iOS", "Android");
    }

    [Fact(DisplayName = "GACTRL_06 - GetBrowserBreakdown chưa cấu hình thì trả Ok rỗng")]
    public async Task GetBrowserBreakdown_NotConfigured_ReturnsOkEmpty()
    {
        _ga4Mock.Setup(s => s.IsConfigured()).Returns(false);

        var result = await _controller.GetBrowserBreakdown(null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<Ga4ReportDto<Ga4DimensionRowDto>>();
        _ga4Mock.Verify(s => s.GetBrowserBreakdownAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GACTRL_07 - GetBrowserBreakdown cấu hình rồi thì trả rows theo trình duyệt")]
    public async Task GetBrowserBreakdown_Configured_ReturnsRows()
    {
        _ga4Mock.Setup(s => s.IsConfigured()).Returns(true);
        _ga4Mock
            .Setup(s => s.GetBrowserBreakdownAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessReport(("Chrome", 90), ("Safari", 40)));

        var result = await _controller.GetBrowserBreakdown(null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var report = ok.Value.Should().BeAssignableTo<Ga4ReportDto<Ga4DimensionRowDto>>().Subject;
        report.Rows.Select(r => r.Label).Should().Equal("Chrome", "Safari");
    }

    [Fact(DisplayName = "GACTRL_08 - GetRealtime chưa cấu hình GA4 thì trả Ok với dto rỗng")]
    public async Task GetRealtime_NotConfigured_ReturnsOkEmptyDto()
    {
        _ga4Mock.Setup(s => s.IsConfigured()).Returns(false);

        var result = await _controller.GetRealtime(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<Ga4RealtimeDto>().Subject;
        dto.ActiveUsers.Should().Be(0);
        dto.ByMinute.Should().BeEmpty();
        dto.ByDevice.Should().BeEmpty();
        dto.BySource.Should().BeEmpty();
        _ga4Mock.Verify(s => s.GetRealtimeAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GACTRL_09 - GetRealtime cấu hình rồi thì trả dto đầy đủ")]
    public async Task GetRealtime_Configured_ReturnsDto()
    {
        _ga4Mock.Setup(s => s.IsConfigured()).Returns(true);
        _ga4Mock
            .Setup(s => s.GetRealtimeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Ga4RealtimeDto>.Success(new Ga4RealtimeDto
            {
                ActiveUsers = 7,
                ScreenPageViews = 12,
                ByMinute = [new Ga4RealtimeRowDto { Label = "202608251429", ActiveUsers = 7, ScreenPageViews = 12 }],
                ByDevice = [new Ga4RealtimeRowDto { Label = "DESKTOP", ActiveUsers = 7, ScreenPageViews = 12 }],
                BySource = [new Ga4RealtimeRowDto { Label = "google", ActiveUsers = 7, ScreenPageViews = 12 }]
            }));

        var result = await _controller.GetRealtime(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<Ga4RealtimeDto>().Subject;
        dto.ActiveUsers.Should().Be(7);
        dto.ScreenPageViews.Should().Be(12);
        dto.ByMinute.Should().ContainSingle().Which.Label.Should().Be("202608251429");
        dto.ByDevice.Single().Label.Should().Be("DESKTOP");
        dto.BySource.Single().Label.Should().Be("google");
    }

    [Fact(DisplayName = "GACTRL_10 - GetRealtime service lỗi thì trả BadRequest")]
    public async Task GetRealtime_ServiceFails_ReturnsBadRequest()
    {
        _ga4Mock.Setup(s => s.IsConfigured()).Returns(true);
        _ga4Mock
            .Setup(s => s.GetRealtimeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Ga4RealtimeDto>.Failure("Google Analytics trả lỗi 403."));

        var result = await _controller.GetRealtime(CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    private static Result<Ga4ReportDto<Ga4DimensionRowDto>> SuccessReport(params (string Label, long Users)[] rows)
    {
        return Result<Ga4ReportDto<Ga4DimensionRowDto>>.Success(new Ga4ReportDto<Ga4DimensionRowDto>
        {
            PropertyId = "123456789",
            StartDate = "2026-08-01",
            EndDate = "2026-08-10",
            Rows = rows
                .Select(r => new Ga4DimensionRowDto { Label = r.Label, TotalUsers = r.Users, ScreenPageViews = r.Users })
                .ToArray(),
            RowCount = rows.Length
        });
    }
}
