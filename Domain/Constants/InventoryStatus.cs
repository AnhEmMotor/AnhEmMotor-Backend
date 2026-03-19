
namespace Domain.Constants;

public static class InventoryStatus
{
    public const string OutOfStock = "OutOfStock";
    public const string LowStock = "LowStock";
    public const string InStock = "InStock";

    public static int GetSeverity(string? status) => status switch
    {
        OutOfStock => 1,
        LowStock => 2,
        InStock => 3,
        _ => 4
    };
}
