using Application.Interfaces.Repositories.Lead;
using Domain.Entities;
using MediatR;

namespace Application.Features.Leads.Commands.AddLeadActivity;

public record AddLeadActivityCommand(int LeadId, string ActivityType, string Description) : IRequest<int>;

