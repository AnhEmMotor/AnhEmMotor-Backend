using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Domains.Entities
{
    [Table("Supplier")]
    public class Supplier
    {
        [Key]
        [Column("Id")]
        public int? Id { get; set; }

        [Column("Name", TypeName = "nvarchar(100)")]
        public string? Name { get; set; }

        [Column("Phone", TypeName = "nvarchar(15)")]
        public string? Phone { get; set; }

        [Column("Email", TypeName = "nvarchar(50)")]
        public string? Email { get; set; }

        [Column("StatusId")]
        [ForeignKey("SupplierStatus")]
        public string? StatusId { get; set; }

        [Column("Notes", TypeName = "nvarchar(MAX)")]
        public string? Notes { get; set; }

        [Column("Address", TypeName = "nvarchar(255)")]
        public string? Address { get; set; }

        public SupplierStatus? SupplierStatus { get; set; }

        public ICollection<Input> InputReceipts { get; set; } = [];
    }
}