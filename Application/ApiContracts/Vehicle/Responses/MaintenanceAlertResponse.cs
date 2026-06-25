using Domain.Constants;
using System;

namespace Application.ApiContracts.Vehicle.Responses
{
    public class MaintenanceAlertResponse
    {
        public string Title { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;

        public string Type { get; set; } = VehiclePortfolio.AlertTypeInfo;

        public string Description { get; set; } = string.Empty;
    }
}
