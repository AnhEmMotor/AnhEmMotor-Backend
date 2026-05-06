using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("OptionValue")]
    public class OptionValue : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("OptionId")]
        [ForeignKey("Option")]
        public int? OptionId { get; set; }

        [Column("Name", TypeName = "nvarchar(100)")]
        public string? Name { get; set; }

        [Column("Description", TypeName = "nvarchar(MAX)")]
        public string? Description { get; set; }

        [Column("ImageUrl")]
        public string? ImageUrl { get; set; }

        [Column("SeoTitle", TypeName = "nvarchar(200)")]
        public string? SeoTitle { get; set; }

        [Column("SeoDescription", TypeName = "nvarchar(500)")]
        public string? SeoDescription { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("ColorCode", TypeName = "nvarchar(20)")]
        public string? ColorCode { get; set; }

        public Option? Option { get; set; }

        public ICollection<VariantOptionValue> VariantOptionValues { get; set; } = [];
    }
}