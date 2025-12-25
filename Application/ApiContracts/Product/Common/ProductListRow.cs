namespace Application.ApiContracts.Product.Common
{
    public sealed class ProductListRow
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public int? CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public int? BrandId { get; set; }

        public string? BrandName { get; set; }

        public string? Description { get; set; }

        public decimal? Weight { get; set; }

        public string? Dimensions { get; set; }

        public string? Wheelbase { get; set; }

        public decimal? SeatHeight { get; set; }

        public decimal? GroundClearance { get; set; }

        public decimal? FuelCapacity { get; set; }

        public string? TireSize { get; set; }

        public string? FrontSuspension { get; set; }

        public string? RearSuspension { get; set; }

        public string? EngineType { get; set; }

        public string? MaxPower { get; set; }

        public decimal? OilCapacity { get; set; }

        public string? FuelConsumption { get; set; }

        public string? TransmissionType { get; set; }

        public string? StarterSystem { get; set; }

        public string? MaxTorque { get; set; }

        public decimal? Displacement { get; set; }

        public string? BoreStroke { get; set; }

        public string? CompressionRatio { get; set; }

        public string? StatusId { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public int TotalStock { get; set; }

        public int TotalBooked { get; set; }
    }
}
