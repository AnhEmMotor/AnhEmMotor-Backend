using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("ProductVariant")]
    public class ProductVariant : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("ProductId")]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Column("UrlSlug", TypeName = "nvarchar(255)")]
        public string? UrlSlug { get; set; }

        [Column("Price", TypeName = "decimal(18, 2)")]
        public decimal? Price { get; set; }

        [Column("CoverImageUrl", TypeName = "nvarchar(1000)")]
        public string? CoverImageUrl { get; set; }

        [Column("VersionName", TypeName = "nvarchar(100)")]
        public string? VersionName { get; set; }

        [Column("ColorName", TypeName = "nvarchar(500)")]
        public string? ColorName { get; set; }

        [Column("ColorCode", TypeName = "nvarchar(200)")]
        public string? ColorCode { get; set; }

        [Column("SKU", TypeName = "nvarchar(50)")]
        public string? SKU { get; set; }

        public Product? Product { get; set; }

        public ICollection<InputInfo> InputInfos { get; set; } = [];

        public ICollection<OutputInfo> OutputInfos { get; set; } = [];

        public ICollection<ProductCollectionPhoto> ProductCollectionPhotos { get; set; } = [];

        public ICollection<VariantOptionValue> VariantOptionValues { get; set; } = [];
    }
}