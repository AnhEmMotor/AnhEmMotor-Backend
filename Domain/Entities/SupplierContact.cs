using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("SupplierContact")]
    public class SupplierContact : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("SupplierId")]
        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; }

        [Column("Name", TypeName = "nvarchar(100)")]
        public string? Name { get; set; }

        [Column("Phone", TypeName = "nvarchar(15)")]
        public string? Phone { get; set; }

        [Column("Email", TypeName = "nvarchar(50)")]
        public string? Email { get; set; }

        [Column("CitizenID", TypeName = "varchar(20)")]
        public string? CitizenID { get; set; }

        public Supplier? Supplier { get; set; }
    }
}