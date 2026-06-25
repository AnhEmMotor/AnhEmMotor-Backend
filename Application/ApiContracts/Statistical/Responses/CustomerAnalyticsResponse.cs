using System;

namespace Application.ApiContracts.Statistical.Responses;

public class CustomerAnalyticsResponse
{
    public CustomerKpi Kpi { get; set; } = new();

    public List<CustomerLeadDto> Leads { get; set; } = [];
}

public class CustomerKpi
{
    public int TotalLeads { get; set; }

    public int NewCustomers { get; set; }

    public int HotLeads { get; set; }
}

public class CustomerLeadDto
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public int LeadScore { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset? LastContact { get; set; }
}
