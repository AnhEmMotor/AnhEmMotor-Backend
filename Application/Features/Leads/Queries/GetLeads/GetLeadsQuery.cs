using Application.ApiContracts.Leads.Responses;
using MediatR;

namespace Application.Features.Leads.Queries.GetLeads;

public record GetLeadsQuery : IRequest<List<LeadResponse>>
{
}
