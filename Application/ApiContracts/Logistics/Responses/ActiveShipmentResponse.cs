using System;

namespace Application.ApiContracts.Logistics.Responses
{
    public class ActiveShipmentResponse
    {
        public int Id { get; set; }

        public string? TrackingNumber { get; set; }

        public string? CustomerName { get; set; }

        public string? CustomerPhone { get; set; }

        public string? CustomerAddress { get; set; }

        public string? Carrier { get; set; }

        public int Status { get; set; }

        public decimal CodAmount { get; set; }

        public decimal ShippingCost { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ExpectedAt { get; set; }

        public int DaysInTransit { get; set; }

        public bool IsStuck { get; set; }
    }
}
