using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class TechnologyImage : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int TechnologyId { get; set; }

        [Required]
        [Column("ImageUrl", TypeName = "nvarchar(1000)")]
        public string ImageUrl { get; set; } = string.Empty;

        [Column("Type", TypeName = "nvarchar(50)")]
        public string Type { get; set; } = "detail";

        public Technology? Technology { get; set; }
    }
}
