using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class SalesContract : BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [MaxLength(100)]
        public string ContractNumber { get; set; } = string.Empty;
        
        // Relates to Output (Sales Order)
        public int? OutputId { get; set; }
        [ForeignKey("OutputId")]
        public Output? Output { get; set; }

        public Guid? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public ApplicationUser? Customer { get; set; }

        // --- Thông tin Bên bán (Showroom Snapshot) ---
        public string? ShowroomName { get; set; }
        public string? ShowroomTaxCode { get; set; }
        public string? ShowroomAddress { get; set; }
        public string? ShowroomRepresentative { get; set; }

        // --- Thông tin Bên mua (Customer Snapshot) ---
        public string? CustomerFullName { get; set; }
        public string? CustomerCCCD { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerPhone { get; set; }

        // --- Thông tin Phương tiện giao dịch ---
        public string? VehicleModel { get; set; }
        public string? VehicleVersion { get; set; }
        public string? VehicleColor { get; set; }
        public string? FrameNumber { get; set; }
        public string? EngineNumber { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ActualSalePrice { get; set; }

        // --- Điều khoản Tài chính & Thanh toán ---
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DepositAmount { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal RemainingAmount { get; set; }
        public DateTimeOffset? FinalPaymentDeadline { get; set; }

        // --- Điều khoản Cam kết & Bảo hành ---
        public string? WarrantyPeriod { get; set; }
        public string? WarrantyScope { get; set; }

        // --- Điều khoản Đặc biệt (Bổ sung) ---
        public string? SpecialTerms { get; set; }

        // --- Trạng thái & File ---
        // Draft, Signed, Fulfilled
        public string Status { get; set; } = "Draft"; 
        
        public DateTimeOffset? SignedDate { get; set; }
        
        public string? ScannedFileUrl { get; set; }
        
        public string? Note { get; set; }
    }
}
