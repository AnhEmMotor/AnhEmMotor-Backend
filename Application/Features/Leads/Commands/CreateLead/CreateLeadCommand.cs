using Application.Common.Models;
using Domain.Constants.Lead;
using MediatR;
using System;

namespace Application.Features.Leads.Commands.CreateLead;

public record CreateLeadCommand : IRequest<Result<int>>
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string IdentificationNumber { get; set; } = string.Empty;

    public DateTime? Birthday { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string AddressDetail { get; set; } = string.Empty;

    public string Ward { get; set; } = string.Empty;

    public string Province { get; set; } = LeadAddressDefaults.Province;

    public string District { get; set; } = LeadAddressDefaults.District;

    public string Status { get; set; } = LeadStatus.New;

    public string Source { get; set; } = LeadSource.WebStore;

    public string InterestedVehicle { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public int Score { get; set; }

    public bool IsVerified { get; set; }
}
