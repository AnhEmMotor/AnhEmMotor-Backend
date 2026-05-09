using Application.Common.Models;
using Application.Interfaces.Repositories.Lead;
using Domain.Entities;
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
}


