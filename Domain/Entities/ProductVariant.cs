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

        [Column("Weight", TypeName = "decimal(18, 2)")]
        public decimal? Weight { get; set; }

        [Column("Dimensions", TypeName = "nvarchar(35)")]
        public string? Dimensions { get; set; }

        [Column("Wheelbase", TypeName = "decimal(18, 2)")]
        public decimal? Wheelbase { get; set; }

        [Column("SeatHeight", TypeName = "decimal(18, 2)")]
        public decimal? SeatHeight { get; set; }

        [Column("GroundClearance", TypeName = "decimal(18, 2)")]
        public decimal? GroundClearance { get; set; }

        [Column("FuelCapacity", TypeName = "decimal(18, 2)")]
        public decimal? FuelCapacity { get; set; }

        [Column("TireSize", TypeName = "nvarchar(100)")]
        public string? TireSize { get; set; }

        [Column("FrontBrake", TypeName = "nvarchar(100)")]
        public string? FrontBrake { get; set; }

        [Column("RearBrake", TypeName = "nvarchar(100)")]
        public string? RearBrake { get; set; }

        [Column("FrontSuspension", TypeName = "nvarchar(255)")]
        public string? FrontSuspension { get; set; }

        [Column("RearSuspension", TypeName = "nvarchar(255)")]
        public string? RearSuspension { get; set; }

        [Column("EngineType", TypeName = "nvarchar(100)")]
        public string? EngineType { get; set; }

        [Column("StockQuantity")]
        public int? StockQuantity { get; set; }

        public Product? Product { get; set; }

        public ICollection<InputInfo> InputInfos { get; set; } = [];

        public ICollection<OutputInfo> OutputInfos { get; set; } = [];

        public ICollection<ProductCollectionPhoto> ProductCollectionPhotos { get; set; } = [];

        public ICollection<VariantOptionValue> VariantOptionValues { get; set; } = [];
    }
}