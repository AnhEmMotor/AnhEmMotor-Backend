using System;
using System.Collections.Generic;

namespace Application.ApiContracts.Statistical.Responses;

public class CustomerServiceAnalyticsResponse
{
    public CustomerServiceKpi Kpi { get; set; } = new();
    public List<CustomerComplaintDto> Complaints { get; set; } = [];
}

public class CustomerServiceKpi
{
    public double AvgRating { get; set; }
    public int NewComplaints { get; set; }
    public double AvgResponseHours { get; set; }
    public int ResolvedCount { get; set; }
}

public class CustomerComplaintDto
{
    public int Id { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
    public double? ResponseHours { get; set; }
}
