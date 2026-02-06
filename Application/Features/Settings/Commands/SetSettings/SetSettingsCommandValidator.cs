using Domain.Constants;
using FluentValidation;

namespace Application.Features.Settings.Commands.SetSettings;

public sealed class SetSettingsCommandValidator : AbstractValidator<SetSettingsCommand>
{
    public SetSettingsCommandValidator()
    {
        RuleFor(x => x.Settings)
            .Custom(
                (settings, context) =>
                {
                    if(settings == null || settings.Count == 0)
                    {
                        context.AddFailure("Settings cannot be empty");
                        return;
                    }

                    if(!settings.Keys.All(SettingKeys.IsValidKey))
                    {
                        context.AddFailure(
                            $"Only the following keys are allowed: {string.Join(", ", SettingKeys.AllowedKeys)}");
                    }

                    foreach (var (key, value) in settings)
                {
                if(string.IsNullOrWhiteSpace(value))
                    continue;

                if(string.Compare(key, SettingKeys.DepositRatio) == 0)
                {
                    if(!decimal.TryParse(value, out var dValue))
                    {
                        context.AddFailure("All numeric fields must contain valid numbers");
                    } else
                    {
                        var valid = dValue >= 1 && dValue <= 99;
                        if(valid)
                        {
                            var parts = value.Trim().Split('.');
                            if(parts.Length > 1 && parts[1].Length > 1)
                                valid = false;
                        }

                        if(!valid)
                        {
                            context.AddFailure("Deposit ratio must be between 1.0 and 99.0 with max 1 decimal place");
                        }
                    }
                } else if(string.Compare(key, SettingKeys.InventoryAlertLevel) == 0 ||
                    string.Compare(key, SettingKeys.OrderValueExceeds) == 0 ||
                    string.Compare(key, SettingKeys.ZBikeThresholdForMeeting) == 0)
                {
                    if(decimal.TryParse(value, out var _))
                    {
                        if(long.TryParse(value, out _))
                        {
                        } else
                        {
                            context.AddFailure("Integer fields cannot have decimal values");
                        }
                    } else
                    {
                        context.AddFailure("All numeric fields must contain valid numbers");
                    }
                }
                }
                });
    }
}