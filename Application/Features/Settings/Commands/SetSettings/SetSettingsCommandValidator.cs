using Domain.Constants;
using FluentValidation;
using System.Globalization;

namespace Application.Features.Settings.Commands.SetSettings;

public class SetSettingsCommandValidator : AbstractValidator<SetSettingsCommand>
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
				context.AddFailure(
					$"Only the following keys are allowed: {string.Join(", ", SettingKeys.AllowedKeys)}");
			}

			foreach (var (key, value) in settings)
			{
				if (string.IsNullOrWhiteSpace(value))
					continue;

				var lowerKey = key.ToLowerInvariant();

				if (lowerKey.EndsWith("deposit_ratio"))
				{
					if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var dValue))
					{
						context.AddFailure("All numeric fields must contain valid numbers");
						continue;
					}

					var valid = dValue >= 1m && dValue <= 99m;
					if (valid)
					{
						var parts = value.Trim().Split('.');
						if (parts.Length > 1 && parts[1].Length > 1)
							valid = false;
					}

					if (!valid)
						context.AddFailure("Deposit ratio must be between 1.0 and 99.0 with max 1 decimal place");
				}
				else if (lowerKey.EndsWith("order_value_exceeds") || lowerKey.EndsWith("inventory_alert_level"))
				{
					if (value.Contains('.') || value.Contains(','))
					{
						context.AddFailure("Integer fields cannot have decimal values");
					}
					else if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
					{
						context.AddFailure("All numeric fields must contain valid numbers");
					}
				}
				else if (lowerKey.EndsWith("deposit_type"))
				{
					var isValidType = string.Equals(value, "percentage", StringComparison.OrdinalIgnoreCase) ||
						string.Equals(value, "fixed", StringComparison.OrdinalIgnoreCase);
					if (!isValidType)
					{
						context.AddFailure("Deposit type must be either 'percentage' or 'fixed'");
					}
				}
				else if (lowerKey.EndsWith("fixed_deposit_amount"))
				{
					if (value.Contains('.') || value.Contains(','))
					{
						context.AddFailure("Fixed deposit amount cannot have decimal values");
					}
					else if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedAmount) || parsedAmount < 0)
					{
						context.AddFailure("Fixed deposit amount must be a non-negative number");
					}
				}
				else if (lowerKey.EndsWith("_enabled"))
				{
					var isBool = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
					              || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
					if (!isBool)
					{
						context.AddFailure($"{key} must be 'true' or 'false'");
					}
				}
			}
		});
	}
}
