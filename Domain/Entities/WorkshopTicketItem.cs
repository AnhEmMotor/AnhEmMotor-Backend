using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("WorkshopTicketItems")]
public class WorkshopTicketItem : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("WorkshopTicketId")]
    [ForeignKey("WorkshopTicket")]
    public int WorkshopTicketId { get; set; }
    public WorkshopTicket WorkshopTicket { get; set; } = null!;

    [Column("Description", TypeName = "nvarchar(MAX)")]
    public string Description { get; set; } = string.Empty;

    [Column("IsPart")]
    public bool IsPart { get; set; } // true if it's a part, false if it's labor

    [Column("Quantity")]
    public int Quantity { get; set; } = 1;

    [Column("UnitPrice", TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; set; }

    [Column("ProductVariantId")]
    [ForeignKey("ProductVariant")]
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
}
