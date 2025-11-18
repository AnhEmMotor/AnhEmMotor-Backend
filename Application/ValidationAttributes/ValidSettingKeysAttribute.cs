using System.ComponentModel.DataAnnotations;

namespace Application.ValidationAttributes
{
    public class ValidSettingKeysAttribute : ValidationAttribute
    {
        private readonly HashSet<string> _ValidKeys =
        [
            "deposit_ratio",
            "inventory_alert_level",
            "order_value_exceeds",
            "bike_threshold_for_meeting"
        ];

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not Dictionary<string, long?> dictionary)
            {
                return ValidationResult.Success;
            }

            var invalidKeys = dictionary.Keys.Where(k => !string.IsNullOrEmpty(k) && !_ValidKeys.Contains(k)).ToList();

            if (invalidKeys.Count != 0)
            {
                var errorMessage = $"Key invaild: {string.Join(", ", invalidKeys)}. Key must be 'deposit_ratio', 'inventory_alert_level', 'order_value_exceeds' or 'bike_threshold_for_meeting'.";
                return new ValidationResult(errorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
