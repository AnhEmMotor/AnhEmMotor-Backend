using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Product")]
    public class Product : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Name", TypeName = "nvarchar(100)")]
        public string? Name { get; set; }

        [Column("ShortDescription", TypeName = "nvarchar(255)")]
        public string? ShortDescription { get; set; }

        [Column("MetaTitle", TypeName = "nvarchar(100)")]
        public string? MetaTitle { get; set; }

        [Column("MetaDescription", TypeName = "nvarchar(255)")]
        public string? MetaDescription { get; set; }

        [Column("CategoryId")]
        [ForeignKey("ProductCategory")]
        public int? CategoryId { get; set; }

        [Column("StatusId")]
        [ForeignKey("ProductStatus")]
        public string? StatusId { get; set; }

        [Column("BrandId")]
        [ForeignKey("Brand")]
        public int? BrandId { get; set; }

        [Column("Weight", TypeName = "nvarchar(20)")]
        public decimal? Weight { get; set; }

        [Column("Dimensions", TypeName = "nvarchar(35)")]
        public string? Dimensions { get; set; }

        [Column("Wheelbase", TypeName = "nvarchar(20)")]
        public string? Wheelbase { get; set; }

        [Column("SeatHeight", TypeName = "nvarchar(20)")]
        public decimal? SeatHeight { get; set; }

        [Column("GroundClearance", TypeName = "nvarchar(20)")]
        public decimal? GroundClearance { get; set; }

        [Column("FuelCapacity", TypeName = "nvarchar(20)")]
        public decimal? FuelCapacity { get; set; }

        [Column("TireSize", TypeName = "nvarchar(100)")]
        public string? TireSize { get; set; }

        [Column("FrontSuspension", TypeName = "nvarchar(255)")]
        public string? FrontSuspension { get; set; }

        [Column("RearSuspension", TypeName = "nvarchar(255)")]
        public string? RearSuspension { get; set; }

        [Column("EngineType", TypeName = "nvarchar(100)")]
        public string? EngineType { get; set; }

        [Column("MaxPower", TypeName = "nvarchar(50)")]
        public string? MaxPower { get; set; }

        [Column("OilCapacity", TypeName = "nvarchar(250)")]
        public decimal? OilCapacity { get; set; }

        [Column("FuelConsumption", TypeName = "nvarchar(35)")]
        public string? FuelConsumption { get; set; }

        [Column("TransmissionType", TypeName = "nvarchar(100)")]
        public string? TransmissionType { get; set; }

        [Column("StarterSystem", TypeName = "nvarchar(30)")]
        public string? StarterSystem { get; set; }

        [Column("MaxTorque", TypeName = "nvarchar(50)")]
        public string? MaxTorque { get; set; }

        [Column("Displacement", TypeName = "nvarchar(50)")]
        public decimal? Displacement { get; set; }

        [Column("BoreStroke", TypeName = "nvarchar(30)")]
        public string? BoreStroke { get; set; }

        [Column("CompressionRatio", TypeName = "nvarchar(10)")]
        public string? CompressionRatio { get; set; }

        [Column("Description", TypeName = "nvarchar(MAX)")]
        public string? Description { get; set; }

        public Brand? Brand { get; set; }

        public ProductCategory? ProductCategory { get; set; }

        public ProductStatus? ProductStatus { get; set; }

        public ICollection<ProductVariant> ProductVariants { get; set; } = [];
    }
}