using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("WorkshopPayment")]
public class WorkshopPayment : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    public string PaymentNumber { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public int SourceId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? VehicleInfo { get; set; }
    public string? ServiceDescription { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public Guid? ReceivedById { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset? InvoicePrintedAt { get; set; }
}
