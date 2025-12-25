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
    }
}
