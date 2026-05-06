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

    public string InterestedVehicle { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string AddressDetail { get; set; } = string.Empty;

    public string Ward { get; set; } = string.Empty;

    public string District { get; set; } = string.Empty;

    public string Province { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public DateTime? Birthday { get; set; }

    public string IdentificationNumber { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public List<LeadActivityResponse> Activities { get; set; } = new();
}

public class LeadActivityResponse
{
    public int Id { get; set; }

    public string ActivityType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
