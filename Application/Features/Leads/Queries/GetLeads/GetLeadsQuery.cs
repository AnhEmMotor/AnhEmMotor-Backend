using Application.ApiContracts.Leads.Responses;
using Application.Interfaces.Repositories.Lead;
using MediatR;
using System.Linq;
using MediatR;

namespace Application.Features.Leads.Queries.GetLeads;

public record GetLeadsQuery : IRequest<List<LeadResponse>>;