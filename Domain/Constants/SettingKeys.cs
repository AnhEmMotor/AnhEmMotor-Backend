namespace Domain.Constants;

public static class SettingKeys
{
    public const string DepositRatio = "Deposit_ratio";
    public const string InventoryAlertLevel = "Inventory_alert_level";
    public const string OrderValueExceeds = "Order_value_exceeds";
    public const string DepositType = "Deposit_type";
    public const string FixedDepositAmount = "Fixed_deposit_amount";

    public const string VehicleDepositEnabled = "VehicleDeposit_enabled";
    public const string VehiclePartsDepositEnabled = "VehiclePartsDeposit_enabled";
    public const string PartsDepositEnabled = "PartsDeposit_enabled";
    public const string AccessoriesDepositEnabled = "AccessoriesDeposit_enabled";
    public const string VehicleOrderValueExceeds = "VehicleOrder_value_exceeds";
    public const string VehiclePartsOrderValueExceeds = "VehiclePartsOrder_value_exceeds";
    public const string PartsOrderValueExceeds = "PartsOrder_value_exceeds";
    public const string AccessoriesOrderValueExceeds = "AccessoriesOrder_value_exceeds";
    public const string VehicleDepositRatio = "VehicleDeposit_ratio";
    public const string VehiclePartsDepositRatio = "VehiclePartsDeposit_ratio";
    public const string PartsDepositRatio = "PartsDeposit_ratio";
    public const string AccessoriesDepositRatio = "AccessoriesDeposit_ratio";
    public const string VehicleDepositType = "VehicleDeposit_type";
    public const string VehiclePartsDepositType = "VehiclePartsDeposit_type";
    public const string PartsDepositType = "PartsDeposit_type";
    public const string AccessoriesDepositType = "AccessoriesDeposit_type";
    public const string VehicleFixedDepositAmount = "VehicleFixed_deposit_amount";
    public const string VehiclePartsFixedDepositAmount = "VehiclePartsFixed_deposit_amount";
    public const string PartsFixedDepositAmount = "PartsFixed_deposit_amount";
    public const string AccessoriesFixedDepositAmount = "AccessoriesFixed_deposit_amount";

    public static readonly HashSet<string> AllowedKeys = [DepositRatio, InventoryAlertLevel, OrderValueExceeds, DepositType, FixedDepositAmount, VehicleDepositEnabled, VehiclePartsDepositEnabled, PartsDepositEnabled, AccessoriesDepositEnabled, VehicleOrderValueExceeds, VehiclePartsOrderValueExceeds, PartsOrderValueExceeds, AccessoriesOrderValueExceeds, VehicleDepositRatio, VehiclePartsDepositRatio, PartsDepositRatio, AccessoriesDepositRatio, VehicleDepositType, VehiclePartsDepositType, PartsDepositType, AccessoriesDepositType, VehicleFixedDepositAmount, VehiclePartsFixedDepositAmount, PartsFixedDepositAmount, AccessoriesFixedDepositAmount];

    public static bool IsValidKey(string key) => AllowedKeys.Contains(key);
}
