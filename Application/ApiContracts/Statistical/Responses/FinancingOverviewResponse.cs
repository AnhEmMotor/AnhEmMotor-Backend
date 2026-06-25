using System;

namespace Application.ApiContracts.Statistical.Responses;

public class FinancingOverviewResponse
{
    public FinancingKpi Kpi { get; set; } = new();

    public List<FinancingInstallmentDto> Installments { get; set; } = [];
}

public class FinancingKpi
{
    public int TotalApplications { get; set; }

    public int DisbursedCount { get; set; }

    public int PendingCount { get; set; }

    public int OverdueCount { get; set; }
}

public class FinancingInstallmentDto
{
    public int Id { get; set; }

    public string ApplicationCode { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string PartnerName { get; set; } = string.Empty;

    public string VehicleName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? CavetStatus { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
}
