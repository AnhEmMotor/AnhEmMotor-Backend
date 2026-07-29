
namespace Domain.Constants;

public static class SettingKeys
{
    public const string DepositRatio = "Deposit_ratio";
    public const string InventoryAlertLevel = "Inventory_alert_level";
    public const string OrderValueExceeds = "Order_value_exceeds";
    public const string DepositType = "Deposit_type";
    public const string FixedDepositAmount = "Fixed_deposit_amount";

    public static readonly HashSet<string> AllowedKeys = [DepositRatio, InventoryAlertLevel, OrderValueExceeds, DepositType, FixedDepositAmount];

    public static bool IsValidKey(string key) => AllowedKeys.Contains(key);
}
