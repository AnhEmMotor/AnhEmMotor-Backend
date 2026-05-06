namespace Application.ApiContracts.Product.Common
{
    public sealed class VariantRow
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string? UrlSlug { get; set; }

        public decimal? Price { get; set; }

        public string? CoverImageUrl { get; set; }

        public List<string> Photos { get; set; } = [];

        public List<OptionPair> OptionPairs { get; set; } = [];

        public int Stock { get; set; }

        public int HasBeenBooked { get; set; }

        public string? VersionName { get; set; }

        public string? ColorName { get; set; }

        public string? ColorCode { get; set; }

        public string? SKU { get; set; }

        public decimal? Weight { get; set; }
        public string? Dimensions { get; set; }
        public decimal? Wheelbase { get; set; }
        public decimal? SeatHeight { get; set; }
        public decimal? GroundClearance { get; set; }
        public decimal? FuelCapacity { get; set; }
        public string? TireSize { get; set; }
        public string? FrontBrake { get; set; }
        public string? RearBrake { get; set; }
        public string? FrontSuspension { get; set; }
        public string? RearSuspension { get; set; }
        public string? EngineType { get; set; }
        public int? StockQuantity { get; set; }
    }
}
