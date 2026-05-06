using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("ProductCompatibility")]
    public class ProductCompatibility : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("BaseProductId")]
        public int BaseProductId { get; set; }

        [Column("CompatibleVehicleModelId")]
        public int CompatibleVehicleModelId { get; set; }

        [ForeignKey("BaseProductId")]
        public Product? BaseProduct { get; set; }

        [ForeignKey("CompatibleVehicleModelId")]
        public Product? CompatibleVehicleModel { get; set; }

        [Column("Notes", TypeName = "nvarchar(500)")]
        public string? Notes { get; set; }
    }
}
