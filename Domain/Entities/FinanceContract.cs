using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class FinanceContract : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string ContractNumber { get; set; } = string.Empty;
        
        public Guid? CustomerId { get; set; }
        
        public string BankName { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
public decimal LoanAmount { get; set; }
        
        public int TermMonths { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
public decimal InterestRate { get; set; }
        
        public string DisbursementStatus { get; set; } = "Pending"; // Pending, Disbursed
        
        public string CavetLocation { get; set; } = string.Empty; // e.g. "Bank", "Store", "Customer"
        
        public DateTime? SignedDate { get; set; }
    }
}
