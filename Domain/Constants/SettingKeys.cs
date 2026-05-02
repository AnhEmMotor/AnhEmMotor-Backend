
namespace Domain.Constants;

public static class SettingKeys
{
    public const string DepositRatio = "Deposit_ratio";
    public const string InventoryAlertLevel = "Inventory_alert_level";
    public const string OrderValueExceeds = "Order_value_exceeds";

    public static readonly HashSet<string> AllowedKeys = [DepositRatio, InventoryAlertLevel, OrderValueExceeds];

    public static bool IsValidKey(string key) => AllowedKeys.Contains(key);
}