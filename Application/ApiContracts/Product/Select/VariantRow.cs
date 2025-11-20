namespace Application.ApiContracts.Product.Select
{
    public sealed class VariantRow
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? UrlSlug { get; set; }
        public long? Price { get; set; }
        public string? CoverImageUrl { get; set; }
        public List<string> Photos { get; set; } = [];
        public List<OptionPair> OptionPairs { get; set; } = [];
        public long Stock { get; set; }
        public long HasBeenBooked { get; set; }
    }
}
