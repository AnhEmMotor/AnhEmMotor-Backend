using System.Reflection;
using Application.Features.Settings.Commands.SetSettings;
using Application.Features.Settings.Queries.GetAllSettings;
using Domain.Common.Models;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact(DisplayName = "SETTING_013 - SetSettings - Key không hợp lệ (không trong danh sách cho phép)")]
    public async Task SETTING_013_SetSettings_InvalidKey_BadRequest()
    {
        // Arrange
        var errorResponse = new Application.Common.Models.ErrorResponse
        {
            Errors =
            [
                new Application.Common.Models.ErrorDetail { Message = "Invalid setting key", Field = "Invalid_Key" }
            ]
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SetSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((null, errorResponse));

        var request = new Dictionary<string, string?> { { "Invalid_Key", "100" } };

        // Act
        var result = await _controller.SetSettings(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var returnedError = badRequestResult.Value.Should().BeOfType<Application.Common.Models.ErrorResponse>().Subject;
        returnedError.Errors.Should().Contain(e => e.Message.Contains("Invalid setting key"));
    }

    [Fact(DisplayName = "SETTING_030 - Controller GetAllSettings - Gọi đúng Query và trả về OkResult")]
    public async Task SETTING_030_Controller_GetAllSettings_CallsQueryAndReturnsOk()
    {
        var expectedSettings = new Dictionary<string, string?>
{
    { "Deposit_ratio", "50.5" },
    { "Inventory_alert_level", "10" },
    { "Order_value_exceeds", "50000000" }
};

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllSettingsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSettings);

        var result = await _controller.GetAllSettings(CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(It.IsAny<GetAllSettingsQuery>(), It.IsAny<CancellationToken>()), Times.Once);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedSettings = okResult.Value.Should().BeAssignableTo<Dictionary<string, string>>().Subject;
        returnedSettings.Should().HaveCount(3);
        returnedSettings["Deposit_ratio"].Should().Be("50.5");
    }

    [Fact(DisplayName = "SETTING_031 - Controller SetSettings - Gọi đúng Command và trả về OkResult")]
    public async Task SETTING_031_Controller_SetSettings_CallsCommandAndReturnsOk()
    {
        // Arrange
        var request = new Dictionary<string, string?>
        {
            { "Deposit_ratio", "50" },
            { "Inventory_alert_level", "10" }
        };

        var expectedResponse = new Dictionary<string, string?>
        {
            { "Deposit_ratio", "50" },
            { "Inventory_alert_level", "10" }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SetSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((expectedResponse, null));

        // Act
        var result = await _controller.SetSettings(request, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<SetSettingsCommand>(cmd => 
                cmd.Settings.ContainsKey("Deposit_ratio") && 
                cmd.Settings["Deposit_ratio"] == "50"),
            It.IsAny<CancellationToken>()), 
            Times.Once);
        
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedDict = okResult.Value.Should().BeAssignableTo<Dictionary<string, long?>>().Subject;
        returnedDict.Should().HaveCount(2);
        returnedDict["Deposit_ratio"].Should().Be(50);
    }

    [Fact(DisplayName = "SETTING_032 - Controller SetSettings - Trả về BadRequest khi validation fail")]
    public async Task SETTING_032_Controller_SetSettings_ValidationFail_ReturnsBadRequest()
    {
        // Arrange
        var errorResponse = new Application.Common.Models.ErrorResponse
        {
            Errors =
            [
                new Application.Common.Models.ErrorDetail { Message = "Validation failed", Field = "Settings" }
            ]
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SetSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((null, errorResponse));

        var request = new Dictionary<string, string?> { { "Deposit_ratio", "0" } };

        // Act
        var result = await _controller.SetSettings(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var returnedError = badRequestResult.Value.Should().BeOfType<Application.Common.Models.ErrorResponse>().Subject;
        returnedError.Errors.Should().Contain(e => e.Message == "Validation failed");
    }

    [Fact(DisplayName = "SETTING_033 - Controller GetAllSettings - Có attribute Authorize phù hợp")]
    public void SETTING_033_Controller_GetAllSettings_HasAuthorizeAttribute()
    {
        // Arrange & Act
        var method = typeof(SettingController).GetMethod(nameof(SettingController.GetAllSettings));
        var controllerType = typeof(SettingController);

        // Assert
        // Check if method or controller has Authorize attribute
        var methodHasAuth = method?.GetCustomAttribute<AuthorizeAttribute>() != null;
        var controllerHasAuth = controllerType.GetCustomAttribute<AuthorizeAttribute>() != null;
        
        (methodHasAuth || controllerHasAuth).Should().BeTrue("Controller or method should have Authorize attribute");
    }

    [Fact(DisplayName = "SETTING_034 - Controller SetSettings - Có attribute Authorize phù hợp")]
    public void SETTING_034_Controller_SetSettings_HasAuthorizeAttribute()
    {
        // Arrange & Act
        var method = typeof(SettingController).GetMethod(nameof(SettingController.SetSettings));
        var controllerType = typeof(SettingController);

        // Assert
        var methodHasAuth = method?.GetCustomAttribute<AuthorizeAttribute>() != null;
        var controllerHasAuth = controllerType.GetCustomAttribute<AuthorizeAttribute>() != null;
        
        (methodHasAuth || controllerHasAuth).Should().BeTrue("Controller or method should have Authorize attribute");
    }

    [Fact(DisplayName = "SETTING_036 - SetSettings - Gửi key với ký tự đặc biệt")]
    public async Task SETTING_036_SetSettings_KeyWithSpecialChars_BadRequest()
    {
        // Arrange
        var errorResponse = new Application.Common.Models.ErrorResponse
        {
            Errors =
            [
                new Application.Common.Models.ErrorDetail { Message = "Invalid setting key", Field = "Deposit<script>" }
            ]
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SetSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((null, errorResponse));

        var request = new Dictionary<string, string?> { { "Deposit<script>", "50" } };

        // Act
        var result = await _controller.SetSettings(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var returnedError = badRequestResult.Value.Should().BeOfType<Application.Common.Models.ErrorResponse>().Subject;
        returnedError.Errors.Should().Contain(e => e.Message.Contains("Invalid setting key"));
    }

    [Fact(DisplayName = "SETTING_037 - SetSettings - Value null cho một key")]
    public async Task SETTING_037_SetSettings_NullValue_KeepsOriginal()
    {
        // Arrange
        var expectedResponse = new Dictionary<string, string?>
        {
            { "Deposit_ratio", "50" } // Original value preserved
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SetSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((expectedResponse, null));

        var request = new Dictionary<string, string?> { { "Deposit_ratio", null } };

        // Act
        var result = await _controller.SetSettings(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedDict = okResult.Value.Should().BeAssignableTo<Dictionary<string, long?>>().Subject;
        returnedDict["Deposit_ratio"].Should().Be(50); // Original value, not null
    }

    [Fact(DisplayName = "SETTING_038 - SetSettings - Gửi nhiều keys cùng lúc, một key invalid")]
    public async Task SETTING_038_SetSettings_MultipleKeysOneInvalid_BadRequest()
    {
        // Arrange
        var errorResponse = new Application.Common.Models.ErrorResponse
        {
            Errors =
            [
                new Application.Common.Models.ErrorDetail { Message = "Invalid setting key", Field = "Invalid_Key" }
            ]
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SetSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((null, errorResponse));

        var request = new Dictionary<string, string?>
        {
            { "Deposit_ratio", "50" },
            { "Invalid_Key", "100" }
        };

        // Act
        var result = await _controller.SetSettings(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var returnedError = badRequestResult.Value.Should().BeOfType<Application.Common.Models.ErrorResponse>().Subject;
        returnedError.Errors.Should().Contain(e => e.Message.Contains("Invalid setting key"));
    }

    [Fact(DisplayName = "SETTING_039 - SetSettings - Integer field với giá trị rất lớn")]
    public async Task SETTING_039_SetSettings_LargeIntegerValue_Success()
    {
        // Arrange
        var expectedResponse = new Dictionary<string, string?>
        {
            { "Inventory_alert_level", "2147483647" } // Int32.MaxValue
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SetSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((expectedResponse, null));

        var request = new Dictionary<string, string?> { { "Inventory_alert_level", "2147483647" } };

        // Act
        var result = await _controller.SetSettings(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedDict = okResult.Value.Should().BeAssignableTo<Dictionary<string, long?>>().Subject;
        returnedDict["Inventory_alert_level"].Should().Be(2147483647);
    }

    [Fact(DisplayName = "SETTING_040 - SetSettings - Integer field với giá trị vượt quá Int32.MaxValue")]
    public async Task SETTING_040_SetSettings_IntegerOverflow_BadRequest()
    {
        // Arrange
        var errorResponse = new Application.Common.Models.ErrorResponse
        {
            Errors =
            [
                new Application.Common.Models.ErrorDetail { Message = "Value out of range for integer field", Field = "Inventory_alert_level" }
            ]
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SetSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((null, errorResponse));

        var request = new Dictionary<string, string?> { { "Inventory_alert_level", "9999999999999" } };

        // Act
        var result = await _controller.SetSettings(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var returnedError = badRequestResult.Value.Should().BeOfType<Application.Common.Models.ErrorResponse>().Subject;
        returnedError.Errors.Should().Contain(e => 
            e.Message.Contains("out of range") || e.Message.Contains("overflow"));
    }
}
