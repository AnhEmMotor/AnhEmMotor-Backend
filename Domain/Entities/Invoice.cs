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
        public string Type { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerIdCard { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string VehicleColor { get; set; } = string.Empty;
        public string ChassisNo { get; set; } = string.Empty;
        public string EngineNo { get; set; } = string.Empty;
        public decimal VehiclePrice { get; set; }
        public decimal RegistrationFee { get; set; }
        public decimal InsuranceFee { get; set; }
        public string PaymentMethod { get; set; } = "transfer";
        public string? BankName { get; set; }
        public string Status { get; set; } = "pending";
        public string? ProcessedBy { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string SalesPerson { get; set; } = string.Empty;
        public DateTime? DeliveryDate { get; set; }
    }
}
