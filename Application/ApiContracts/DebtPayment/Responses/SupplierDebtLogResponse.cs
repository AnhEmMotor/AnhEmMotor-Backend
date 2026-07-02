using Domain.Entities;

namespace Application.ApiContracts.DebtPayment.Responses;

public class SupplierDebtLogResponse
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public decimal AmountPaid { get; set; }

    public decimal RemainingDebt { get; set; }

    public DateTimeOffset PaymentDate { get; set; }

    public string CreatedById { get; set; } = null!;

    public ApplicationUser CreatedBy { get; set; } = null!;

    public Domain.Entities.Supplier Supplier { get; set; } = null!;

    public bool HasProofImage { get; set; }
}
