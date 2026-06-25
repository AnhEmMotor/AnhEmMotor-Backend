namespace Application.ApiContracts.Admin.Inventory
{
    public record InventoryVehicleResponse(int Id, string ModelName, string Vin, string StatusTag, string Color);

    public record AgingStockResponse(int Id, string ModelName, int DaysInStock, DateTime ArrivalDate);

    public record PartsWarningResponse(string PartName, int CurrentStock, int MinimumThreshold, string WarningLevel);
}
