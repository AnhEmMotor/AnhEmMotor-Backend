using Domain.Constants;
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

        public int? OutputId { get; set; }

        [ForeignKey("OutputId")]
        public Output? Output { get; set; }

        public Guid? CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public ApplicationUser? Customer { get; set; }

        public string? ShowroomName { get; set; }

        public string? ShowroomTaxCode { get; set; }

        public string? ShowroomAddress { get; set; }

        public string? ShowroomRepresentative { get; set; }

        public string? CustomerFullName { get; set; }

        public string? CustomerCCCD { get; set; }

        public string? CustomerAddress { get; set; }

        public string? CustomerPhone { get; set; }

        public string? VehicleModel { get; set; }

        public string? VehicleVersion { get; set; }

        public string? VehicleColor { get; set; }

        public string? FrameNumber { get; set; }

        public string? EngineNumber { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ActualSalePrice { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DepositAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal RemainingAmount { get; set; }

        public DateTimeOffset? FinalPaymentDeadline { get; set; }

        public string? WarrantyPeriod { get; set; }

        public string? WarrantyScope { get; set; }

        public string? SpecialTerms { get; set; }

        public string Status { get; set; } = SalesContractStatus.Draft;

        public DateTimeOffset? SignedDate { get; set; }

        public string? ScannedFileUrl { get; set; }

        public string? Note { get; set; }
    }
}
