namespace Application.ApiContracts.RepairOrder.Responses
{
    public class RepairOrderDetailResponse
    {
        public int Id { get; set; }

        public int RepairOrderId { get; set; }

        public int? ServiceId { get; set; }

        public string? ServiceName { get; set; }

        public int? ProductVariantId { get; set; }

        public string? VariantName { get; set; }

        public string? ProductCode { get; set; }

        public int Count { get; set; }

        public decimal Price { get; set; }

        public decimal LaborCost { get; set; }

        public string Type { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}
