using Application.Features.Settings.Commands.SetSettings;
using Application.Features.Settings.Queries.GetAllSettings;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Setting;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using Xunit;
using SettingEntity = Domain.Entities.Setting;

namespace UnitTests;

public class Setting
{
    private readonly Mock<ISettingRepository> _settingRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public Setting()
    {
        _settingRepoMock = new Mock<ISettingRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact(DisplayName = "SETTING_014 - Validator - Deposit_ratio = 1 (giá trị biên tối thiểu hợp lệ)")]
    public void SETTING_014_Validator_DepositRatio_MinimumBoundary_ShouldPass()
    {
        // Arrange
        var validator = new SetSettingsCommandValidator();
        var request = new SetSettingsCommand(new Dictionary<string, string?> { { "Deposit_ratio", "1" } });

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "SETTING_015 - Validator - Deposit_ratio = 99 (giá trị biên tối đa hợp lệ)")]
    public void SETTING_015_Validator_DepositRatio_MaximumBoundary_ShouldPass()
    {
        // Arrange
        var validator = new SetSettingsCommandValidator();
        var request = new SetSettingsCommand(new Dictionary<string, string?> { { "Deposit_ratio", "99" } });

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "SETTING_016 - Validator - Deposit_ratio = 50.5 (hợp lệ với 1 chữ số thập phân)")]
    public void SETTING_016_Validator_DepositRatio_OneDecimalPlace_ShouldPass()
    {
        // Arrange
        var validator = new SetSettingsCommandValidator();
        var request = new SetSettingsCommand(new Dictionary<string, string?> { { "Deposit_ratio", "50.5" } });

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "SETTING_017 - Validator - Deposit_ratio = 0.9 (dưới ngưỡng)")]
    public void SETTING_017_Validator_DepositRatio_BelowMinimum_ShouldFail()
    {
        // Arrange
        var validator = new SetSettingsCommandValidator();
        var request = new SetSettingsCommand(new Dictionary<string, string?> { { "Deposit_ratio", "0.9" } });

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Settings)
            .WithErrorMessage("Deposit ratio must be between 1.0 and 99.0 with max 1 decimal place");
    }

    [Fact(DisplayName = "SETTING_018 - Validator - Deposit_ratio = 99.1 (trên ngưỡng)")]
    public void SETTING_018_Validator_DepositRatio_AboveMaximum_ShouldFail()
    {
        // Arrange
        var validator = new SetSettingsCommandValidator();
        var request = new SetSettingsCommand(new Dictionary<string, string?> { { "Deposit_ratio", "99.1" } });

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Settings)
            .WithErrorMessage("Deposit ratio must be between 1.0 and 99.0 with max 1 decimal place");
    }

    [Fact(DisplayName = "SETTING_019 - Validator - Deposit_ratio = \"abc\" (không phải số)")]
    public void SETTING_019_Validator_DepositRatio_NonNumeric_ShouldFail()
    {
        // Arrange
        var validator = new SetSettingsCommandValidator();
        var request = new SetSettingsCommand(new Dictionary<string, string?> { { "Deposit_ratio", "abc" } });

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Settings)
            .WithErrorMessage("All numeric fields must contain valid numbers");
    }

    [Fact(DisplayName = "SETTING_020 - Validator - Inventory_alert_level = 100 (integer hợp lệ)")]
    public void SETTING_020_Validator_InventoryAlertLevel_ValidInteger_ShouldPass()
    {
        // Arrange
        var validator = new SetSettingsCommandValidator();
        var request = new SetSettingsCommand(new Dictionary<string, string?> { { "Inventory_alert_level", "100" } });

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "SETTING_021 - Validator - Inventory_alert_level = 50.5 (decimal không hợp lệ cho integer field)")]
    public void SETTING_021_Validator_InventoryAlertLevel_DecimalValue_ShouldFail()
    {
        // Arrange
        var validator = new SetSettingsCommandValidator();
        var request = new SetSettingsCommand(new Dictionary<string, string?> { { "Inventory_alert_level", "50.5" } });

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Settings)
            .WithErrorMessage("Integer fields cannot have decimal values");
    }

    [Fact(DisplayName = "SETTING_022 - Validator - Order_value_exceeds = \"text\" (không phải số)")]
    public void SETTING_022_Validator_OrderValueExceeds_NonNumeric_ShouldFail()
    {
        // Arrange
        var validator = new SetSettingsCommandValidator();
        var request = new SetSettingsCommand(new Dictionary<string, string?> { { "Order_value_exceeds", "text" } });

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Settings)
            .WithErrorMessage("All numeric fields must contain valid numbers");
    }

    [Fact(DisplayName = "SETTING_023 - Validator - Z-bike_threshold_for_meeting = -5 (số âm)")]
    public void SETTING_023_Validator_BikeThreshold_NegativeNumber_ShouldPass()
    {
        // Arrange
        var validator = new SetSettingsCommandValidator();
        var request = new SetSettingsCommand(new Dictionary<string, string?> { { "Z-bike_threshold_for_meeting", "-5" } });

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "SETTING_024 - Validator - Settings dictionary rỗng")]
    public void SETTING_024_Validator_EmptySettings_ShouldFail()
    {
        // Arrange
        var validator = new SetSettingsCommandValidator();
        var request = new SetSettingsCommand([]);

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Settings)
            .WithErrorMessage("Settings cannot be empty");
    }

    [Fact(DisplayName = "SETTING_025 - Validator - Khoảng trắng đầu/cuối trong value")]
    public void SETTING_025_Validator_WhitespaceInValue_ShouldHandleCorrectly()
    {
        // Arrange
        var validator = new SetSettingsCommandValidator();
        var request = new SetSettingsCommand(new Dictionary<string, string?> { { "Deposit_ratio", " 50 " } });

        // Act
        var result = validator.TestValidate(request);

        // Assert
        // Validator should either trim and pass, or fail with appropriate message
        // Based on validator implementation, it should parse after trim
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "SETTING_026 - Handler GetAllSettings - Trả về tất cả settings từ repository")]
    public async Task SETTING_026_Handler_GetAllSettings_ReturnsAllSettings()
    {
        // Arrange
        var settings = new List<SettingEntity>
        {
            new () { Key = "Deposit_ratio", Value = "50.5" },
            new () { Key = "Inventory_alert_level", Value = "10" },
            new () { Key = "Order_value_exceeds", Value = "50000000" },
            new () { Key = "Z-bike_threshold_for_meeting", Value = "5" }
        };

        _settingRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);

        var handler = new GetAllSettingsQueryHandler(_settingRepoMock.Object);
        var query = new GetAllSettingsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().HaveCount(4);
        result.Value["Deposit_ratio"].Should().Be("50.5");
        result.Value["Inventory_alert_level"].Should().Be("10");
        result.Value["Order_value_exceeds"].Should().Be("50000000");
        result.Value["Z-bike_threshold_for_meeting"].Should().Be("5");
    }

    [Fact(DisplayName = "SETTING_027 - Handler SetSettings - Gọi Update repository với đúng data")]
    public async Task SETTING_027_Handler_SetSettings_CallsUpdateWithCorrectData()
    {
        // Arrange
        var existingSettings = new List<SettingEntity>
        {
            new () { Key = "Deposit_ratio", Value = "30" },
            new () { Key = "Inventory_alert_level", Value = "5" }
        };

        _settingRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSettings);

        var handler = new SetSettingsCommandHandler(_settingRepoMock.Object, _unitOfWorkMock.Object);
        var command = new SetSettingsCommand(new Dictionary<string, string?>
        {
            { "Deposit_ratio", "50" },
            { "Inventory_alert_level", "10" }
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        _settingRepoMock.Verify(x => x.Update(It.Is<IEnumerable<SettingEntity>>(s => 
            s.Count() == 2 &&
            s.Any(setting => setting.Key == "Deposit_ratio" && setting.Value == "50") &&
            s.Any(setting => setting.Key == "Inventory_alert_level" && setting.Value == "10")
        )), Times.Once);
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
