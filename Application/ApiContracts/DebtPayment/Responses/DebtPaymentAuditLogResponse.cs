using System;

namespace Application.ApiContracts.DebtPayment.Responses;

public class DebtPaymentAuditLogResponse
{
    public int Id { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? ChangedByFullName { get; set; }

    public DateTimeOffset ChangedAt { get; set; }

    public decimal? OldAmount { get; set; }

    public decimal? NewAmount { get; set; }

    public string? OldNotes { get; set; }

    public string? NewNotes { get; set; }

    public DateTimeOffset? OldPaymentDate { get; set; }

    public DateTimeOffset? NewPaymentDate { get; set; }
}
