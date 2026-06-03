using System;
using System.Collections.Generic;
using Application.ApiContracts.RepairOrder.Responses;

namespace Application.ApiContracts.Vehicle.Responses
{
    public class VehiclePortfolioResponse
    {
        public VehicleResponse Vehicle { get; set; } = new();
        public List<RepairOrderResponse> History { get; set; } = new();
        public int TotalHistoryCount { get; set; }
        public List<MaintenanceAlertResponse> Alerts { get; set; } = new();
    }

    public class MaintenanceAlertResponse
    {
        public string Title { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Type { get; set; } = "info"; // warning, danger, info, success
        public string Description { get; set; } = string.Empty;
    }
}
