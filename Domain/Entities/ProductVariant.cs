using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("ProductVariant")]
    public class ProductVariant
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("ProductId")]
        [ForeignKey("Product")]
        public int? ProductId { get; set; }

        [Column("UrlSlug", TypeName = "nvarchar(50)")]
        public string? UrlSlug { get; set; }

        [Column("Price")]
        public long? Price { get; set; }

        [Column("CoverImageUrl", TypeName = "nvarchar(100)")]
        public string? CoverImageUrl { get; set; }

        public Product? Product { get; set; }

        public ICollection<InputInfo> InputInfos { get; set; } = [];
        public ICollection<OutputInfo> OutputInfos { get; set; } = [];
        public ICollection<ProductCollectionPhoto> ProductCollectionPhotos { get; set; } = [];
        public ICollection<VariantOptionValue> VariantOptionValues { get; set; } = [];
    }
}