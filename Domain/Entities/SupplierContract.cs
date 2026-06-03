using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
public class SupplierContract : BaseEntity
{
	[Key]
	public Guid Id { get; set; } = Guid.NewGuid();

	public int? SupplierId { get; set; }
	[ForeignKey("SupplierId")]
	public Supplier? Supplier { get; set; }

	[MaxLength(100)]
	public string ContractNumber { get; set; } = string.Empty;

	[MaxLength(500)]
	public string? ContractFilePath { get; set; }

	public DateTime EffectiveDate { get; set; }

	public DateTime? ExpirationDate { get; set; }

	[Column(TypeName = "decimal(18, 2)")]
	public decimal ContractValue { get; set; }

	// Draft, Active, Expired, Terminated, Completed
	[MaxLength(50)]
	public string Status { get; set; } = "Draft";

	[MaxLength(500)]
	public string? Terms { get; set; }

	[MaxLength(1000)]
	public string? Note { get; set; }

	// --- NEW FIELDS ADDED ACCORDING TO BACKUP1.MD ---

	// Credit & Payment Terms
	[Column(TypeName = "decimal(18, 2)")]
	public decimal? CreditLimit { get; set; } // Han muc cong no toi da

	public int? PaymentWindowDays { get; set; } // Thoi han thanh toan (ngay)

	[MaxLength(50)]
	public string? BankAccountNumber { get; set; } // Tai khoan thu huong

	[MaxLength(200)]
	public string? BankName { get; set; }

	// Price Matrix & Volume Commitments
	public int? MinimumVolumePerMonth { get; set; } // San luong toi thieu/Thang

	[Column(TypeName = "decimal(5, 2)")]
	public decimal? DiscountRate { get; set; } // Ty le chiet khau thuong mai (%)

	// Phu luc hop dong
	public Guid? ParentContractId { get; set; }
	[ForeignKey("ParentContractId")]
	public SupplierContract? ParentContract { get; set; }

	public ICollection<SupplierContract> Addendums { get; set; } = [];

	// Bang lien ket SKU gia nhap si
	public ICollection<SupplierContractItem> ContractItems { get; set; } = [];

	public ICollection<SupplierContractAuditLog> AuditLogs { get; set; } = [];
}
}
