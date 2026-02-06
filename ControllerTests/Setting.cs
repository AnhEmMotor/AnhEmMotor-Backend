using Application.Common.Models;
using Application.Features.Settings.Commands.SetSettings;
using Application.Features.Settings.Queries.GetAllSettings;
using FluentAssertions;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class Setting
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly SettingController _controller;

    public Setting()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new SettingController(_mediatorMock.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
    }

#pragma warning disable IDE0079 
#pragma warning disable CRR0035

    [Fact(DisplayName = "SETTING_030 - Controller GetAllSettings - Gọi đúng Query và trả về OkResult")]
    public async Task SETTING_030_Controller_GetAllSettings_CallsQueryAndReturnsOk()
    {
        var expectedSettings = new Dictionary<string, string?>
        { { "Deposit_ratio", "50.5" }, { "Inventory_alert_level", "10" }, { "Order_value_exceeds", "50000000" } };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllSettingsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Dictionary<string, string?>>.Success(expectedSettings));

        var result = await _controller.GetAllSettingsAsync(CancellationToken.None).ConfigureAwait(true);

        _mediatorMock.Verify(m => m.Send(It.IsAny<GetAllSettingsQuery>(), It.IsAny<CancellationToken>()), Times.Once);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedSettings = okResult.Value.Should().BeAssignableTo<Dictionary<string, string>>().Subject;
        returnedSettings.Should().HaveCount(3);
        returnedSettings["Deposit_ratio"].Should().Be("50.5");
    }

    [Fact(DisplayName = "SETTING_031 - Controller SetSettings - Gọi đúng Command và trả về OkResult")]
    public async Task SETTING_031_Controller_SetSettings_CallsCommandAndReturnsOk()
    {
        var request = new Dictionary<string, string?> { { "Deposit_ratio", "50" }, { "Inventory_alert_level", "10" } };

        var expectedResponse = new Dictionary<string, string?>
        { { "Deposit_ratio", "50" }, { "Inventory_alert_level", "10" } };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SetSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Dictionary<string, string?>?>.Success(expectedResponse));

        var result = await _controller.SetSettingsAsync(request, CancellationToken.None).ConfigureAwait(true);

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<SetSettingsCommand>(
                    cmd => cmd.Settings!.ContainsKey("Deposit_ratio") &&
                        string.Compare(cmd.Settings["Deposit_ratio"], "50") == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);

        var okResult = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        var returnedDict = okResult.Value.Should().BeAssignableTo<Dictionary<string, string?>>().Subject;
        returnedDict.Should().HaveCount(2);
        returnedDict["Deposit_ratio"].Should().Be("50");
    }

    [Fact(DisplayName = "SETTING_032 - Controller SetSettings - Trả về BadRequest khi validation fail")]
    public async Task SETTING_032_Controller_SetSettings_ValidationFail_ReturnsBadRequest()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<SetSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<Dictionary<string, string?>?>.Failure(Error.Validation("Validation failed", "Settings")));

        var request = new Dictionary<string, string?> { { "Deposit_ratio", "0" } };

        var result = await _controller.SetSettingsAsync(request, CancellationToken.None).ConfigureAwait(true);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var returnedError = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;

        returnedError.Errors.Should().ContainSingle().Which.Message.Should().Be("Validation failed");
    }

    [Fact(DisplayName = "SETTING_033 - Controller GetAllSettings - Có attribute Authorize phù hợp")]
    public void SETTING_033_Controller_GetAllSettings_HasAuthorizeAttribute()
    {
        var method = typeof(SettingController).GetMethod(nameof(SettingController.GetAllSettingsAsync));
        var controllerType = typeof(SettingController);

        var methodHasAuth = method?.GetCustomAttribute<AuthorizeAttribute>() != null;
        var controllerHasAuth = controllerType.GetCustomAttribute<AuthorizeAttribute>() != null;

        (methodHasAuth || controllerHasAuth).Should().BeTrue("Controller or method should have Authorize attribute");
    }

    [Fact(DisplayName = "SETTING_034 - Controller SetSettings - Có attribute Authorize phù hợp")]
    public void SETTING_034_Controller_SetSettings_HasAuthorizeAttribute()
    {
        var method = typeof(SettingController).GetMethod(nameof(SettingController.SetSettingsAsync));
        var controllerType = typeof(SettingController);

        var methodHasAuth = method?.GetCustomAttribute<AuthorizeAttribute>() != null;
        var controllerHasAuth = controllerType.GetCustomAttribute<AuthorizeAttribute>() != null;

        (methodHasAuth || controllerHasAuth).Should().BeTrue("Controller or method should have Authorize attribute");
    }

    [Fact(DisplayName = "SETTING_037 - SetSettings - Value null cho một key")]
    public async Task SETTING_037_SetSettings_NullValue_KeepsOriginal()
    {
        var expectedResponse = new Dictionary<string, string?> { { "Deposit_ratio", "50" } };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SetSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Dictionary<string, string?>?>.Success(expectedResponse));

        var request = new Dictionary<string, string?> { { "Deposit_ratio", null } };

        var result = await _controller.SetSettingsAsync(request, CancellationToken.None).ConfigureAwait(true);

        var okResult = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        var returnedDict = okResult.Value.Should().BeAssignableTo<Dictionary<string, string?>>().Subject;
        returnedDict["Deposit_ratio"].Should().Be("50");
    }

    [Fact(DisplayName = "SETTING_039 - SetSettings - Integer field với giá trị rất lớn")]
    public async Task SETTING_039_SetSettings_LargeIntegerValue_Success()
    {
        var expectedResponse = new Dictionary<string, string?> { { "Inventory_alert_level", "2147483647" } };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SetSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Dictionary<string, string?>?>.Success(expectedResponse));

        var request = new Dictionary<string, string?> { { "Inventory_alert_level", "2147483647" } };

        var result = await _controller.SetSettingsAsync(request, CancellationToken.None).ConfigureAwait(true);

        var okResult = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        var returnedDict = okResult.Value.Should().BeAssignableTo<Dictionary<string, string?>>().Subject;
        returnedDict["Inventory_alert_level"].Should().Be("2147483647");
    }
#pragma warning restore CRR0035
#pragma warning restore IDE0079
}
