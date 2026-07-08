namespace Domain.Entities
{
    public class ProductCategoryTranslation
    {
        public int Id { get; set; }

        public int ProductCategoryId { get; set; }

        public string LanguageCode { get; set; } = "vi";

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ProductCategory ProductCategory { get; set; } = null!;

        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }
    }
}
