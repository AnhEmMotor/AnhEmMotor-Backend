using System.Collections.Generic;

namespace Application.ApiContracts.Admin.Inventory
{
    public record InventoryVehicleResponse(
        int Id, 
        string ModelName, 
        string Vin, 
        string StatusTag, // 🔴 Đã cọc, 🟢 Sẵn sàng, 🟡 Đang về
        string Color);

    public record AgingStockResponse(
        int Id, 
        string ModelName, 
        int DaysInStock, 
        DateTime ArrivalDate);

    public record PartsWarningResponse(
        string PartName, 
        int CurrentStock, 
        int MinimumThreshold, 
        string WarningLevel);
}
