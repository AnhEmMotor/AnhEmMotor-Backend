using Application.ApiContracts.Leads.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Leads.Queries.GetLeadPipeline;

public class GetLeadPipelineQuery : IRequest<Result<List<LeadPipelineGroupResponse>>>
{
}

