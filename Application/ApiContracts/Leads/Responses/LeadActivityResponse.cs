using System;

namespace Application.ApiContracts.Leads.Responses;

public class LeadActivityResponse
{
    public int Id { get; set; }

    public string ActivityType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
