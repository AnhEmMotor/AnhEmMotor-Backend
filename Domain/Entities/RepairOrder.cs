using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("RepairOrder")]
    public class RepairOrder : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("VehicleId")]
        [ForeignKey("Vehicle")]
        public int? VehicleId { get; set; }

        public Vehicle? Vehicle { get; set; }

        [Required]
        [Column("CustomerName", TypeName = "nvarchar(100)")]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [Column("CustomerPhone", TypeName = "nvarchar(20)")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Column("Mileage")]
        public int Mileage { get; set; }

        [Column("Description", TypeName = "nvarchar(MAX)")]
        public string Description { get; set; } = string.Empty;

        [Column("StartTime")]
        public DateTimeOffset? StartTime { get; set; }

        [Column("ExpectedCompletionTime")]
        public DateTimeOffset? ExpectedCompletionTime { get; set; }

        [Column("TechnicianId")]
        [ForeignKey("Technician")]
        public int? TechnicianId { get; set; }

        public EmployeeProfile? Technician { get; set; }

        [Required]
        [Column("Status", TypeName = "nvarchar(20)")]
        public string Status { get; set; } = RepairOrderStatus.Pending;

        [Column("LaborCost", TypeName = "decimal(18,2)")]
        public decimal LaborCost { get; set; }

        [Column("PartsCost", TypeName = "decimal(18,2)")]
        public decimal PartsCost { get; set; }

        [Column("TotalAmount", TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column("PaymentStatus", TypeName = "nvarchar(20)")]
        public string PaymentStatus { get; set; } = "Unpaid";

        [Column("PaymentMethod", TypeName = "nvarchar(50)")]
        public string? PaymentMethod { get; set; }

        [Column("Notes", TypeName = "nvarchar(MAX)")]
        public string? Notes { get; set; }

        [Column("CompletedDate")]
        public DateTimeOffset? CompletedDate { get; set; }

        public ICollection<RepairOrderDetail> Details { get; set; } = new List<RepairOrderDetail>();
    }
}
