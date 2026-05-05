using System;

namespace Application.ApiContracts.Leads.Responses;

public class LeadResponse
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public int Score { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public List<LeadActivityResponse> Activities { get; set; } = [];
}


