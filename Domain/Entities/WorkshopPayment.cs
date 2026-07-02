using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("WorkshopPayment")]
    public class WorkshopPayment : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("PaymentNumber", TypeName = "nvarchar(50)")]
        public string PaymentNumber { get; set; } = string.Empty;

        [Column("SourceType", TypeName = "nvarchar(30)")]
        public string SourceType { get; set; } = string.Empty;

        [Column("SourceId")]
        public int SourceId { get; set; }

        [Column("CustomerName", TypeName = "nvarchar(100)")]
        public string CustomerName { get; set; } = string.Empty;

        [Column("CustomerPhone", TypeName = "nvarchar(20)")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Column("VehicleInfo", TypeName = "nvarchar(200)")]
        public string? VehicleInfo { get; set; }

        [Column("ServiceDescription", TypeName = "nvarchar(MAX)")]
        public string? ServiceDescription { get; set; }

        [Column("SubTotal", TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column("DiscountAmount", TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column("TotalAmount", TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column("PaymentMethod", TypeName = "nvarchar(30)")]
        public string PaymentMethod { get; set; } = "Cash";

        [Column("PaymentStatus", TypeName = "nvarchar(30)")]
        public string PaymentStatus { get; set; } = "Paid";

        [Column("ReceivedById")]
        public Guid? ReceivedById { get; set; }

        [Column("PaidAt")]
        public DateTimeOffset? PaidAt { get; set; }

        [Column("Notes", TypeName = "nvarchar(MAX)")]
        public string? Notes { get; set; }

        [Column("InvoicePrintedAt")]
        public DateTimeOffset? InvoicePrintedAt { get; set; }
    }
}
