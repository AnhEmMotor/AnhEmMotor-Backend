using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Invoice")]
    public class Invoice : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Type { get; set; } = string.Empty; // Mua xe / Dịch vụ & Phụ tùng
        
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
