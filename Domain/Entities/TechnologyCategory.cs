using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class TechnologyCategory : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("Name", TypeName = "nvarchar(255)")]
        public string Name { get; set; } = string.Empty;

        public ICollection<Technology> Technologies { get; set; } = [];
    }
}
