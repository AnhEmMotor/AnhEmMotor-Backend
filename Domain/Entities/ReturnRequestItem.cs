using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("ReturnRequestItem")]
    public class ReturnRequestItem : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        public int ReturnRequestId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public int Quantity { get; set; }
        public int ReturnQuantity { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        [ForeignKey("ReturnRequestId")]
        public virtual ReturnRequest? ReturnRequest { get; set; }
        
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
