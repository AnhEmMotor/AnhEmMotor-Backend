using Application.ApiContracts.RepairOrder.Responses;
using System;

namespace Application.ApiContracts.Vehicle.Responses
{
    public class VehiclePortfolioResponse
    {
        public VehicleResponse Vehicle { get; set; } = new();

        public List<RepairOrderResponse> History { get; set; } = [];

        public int TotalHistoryCount { get; set; }

        public List<MaintenanceAlertResponse> Alerts { get; set; } = [];
    }
}
