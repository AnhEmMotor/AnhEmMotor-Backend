using Application.Common.Models;
using Application.Interfaces.Repositories.Lead;
using MediatR;
using System;

namespace Application.Features.Leads.Commands.UpdateLead;

public record UpdateLeadCommand : IRequest<Result<int>>
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public DateTime? Birthday { get; set; }

    public string IdentificationNumber { get; set; } = string.Empty;

    public string AddressDetail { get; set; } = string.Empty;

    public string Ward { get; set; } = string.Empty;

    public string District { get; set; } = "Biên Hòa";

    public string Province { get; set; } = "Đồng Nai";

    public string Status { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string InterestedVehicle { get; set; } = string.Empty;

    public int Score { get; set; }
}

