using Application.ApiContracts.Leads.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Lead;
using MediatR;

namespace Application.Features.Leads.Queries.GetLeadPipeline;

public sealed class GetLeadPipelineQuery : IRequest<Result<List<LeadPipelineGroupResponse>>>
{
}



