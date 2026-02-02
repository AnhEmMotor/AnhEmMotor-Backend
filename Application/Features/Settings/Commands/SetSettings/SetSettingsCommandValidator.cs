using Domain.Constants;
using FluentValidation;

namespace Application.Features.Settings.Commands.SetSettings;

public sealed class SetSettingsCommandValidator : AbstractValidator<SetSettingsCommand>
{
    public SetSettingsCommandValidator()
    {
        RuleFor(x => x.Settings)
            .Custom((settings, context) =>
            {
                if (settings == null || settings.Count == 0)
                {
                    context.AddFailure("Settings cannot be empty");
                    return;
                }

                if (!settings.Keys.All(SettingKeys.IsValidKey))
                {
                    context.AddFailure($"Only the following keys are allowed: {string.Join(", ", SettingKeys.AllowedKeys)}");
                }

                foreach (var (key, value) in settings)
                {
                    if (string.IsNullOrWhiteSpace(value)) continue;

                    if (key == SettingKeys.DepositRatio)
                    {
                        if (!decimal.TryParse(value, out var dValue))
                        {
                            context.AddFailure("All numeric fields must contain valid numbers");
                        }
                        else
                        {
                            // Check range and decimal places (max 1)
                            var valid = dValue >= 1 && dValue <= 99;
                            if (valid)
                            {
                                var parts = value.Trim().Split('.');
                                if (parts.Length > 1 && parts[1].Length > 1) valid = false;
                            }

                            if (!valid)
                            {
                                context.AddFailure("Deposit ratio must be between 1.0 and 99.0 with max 1 decimal place");
                            }
                        }
                    }
                    else if (key == SettingKeys.InventoryAlertLevel ||
                             key == SettingKeys.OrderValueExceeds ||
                             key == SettingKeys.ZBikeThresholdForMeeting)
                    {
                        // Check if it is a number at NOT an integer (i.e. has decimals)
                        if (decimal.TryParse(value, out var dVal))
                        {
                            // It is a valid number
                            // Check if it has decimal part. 
                            // Note: 50.0 might be considered integer, but "50.5" is not.
                            // Test SETTING_021: "50.5" -> "Integer fields cannot have decimal values"
                            // Implementation: check if dVal % 1 != 0 or string contains '.' ?
                            // String check is safer for "1.0" if strict integer required, but usually 1.0 == 1.
                            // Tests use "50.5".
                            // SettingKeys.InventoryAlertLevel test says "Integer fields cannot have decimal values".
                            
                            // Simple way: TryParse int/long. If fail but decimal ok -> has decimals (or out of range).
                            
                            if (long.TryParse(value, out _))
                            {
                                // it is valid integer
                            }
                            else
                            {
                                // Is it a valid decimal?
                                context.AddFailure("Integer fields cannot have decimal values");
                            }
                        }
                        else
                        {
                             // Not a number at all
                             context.AddFailure("All numeric fields must contain valid numbers");
                        }
                    }
                }
            });
    }
}