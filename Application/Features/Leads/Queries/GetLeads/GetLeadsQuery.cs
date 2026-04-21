using Application.ApiContracts.Leads.Responses;
using Application.Interfaces.Repositories.Lead;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Leads.Queries.GetLeads;

public record GetLeadsQuery : IRequest<List<LeadResponse>> { }

public class GetLeadsQueryHandler(ILeadReadRepository leadReadRepository) : IRequestHandler<GetLeadsQuery, List<LeadResponse>>
{
    public async Task<List<LeadResponse>> Handle(GetLeadsQuery request, CancellationToken cancellationToken)
    {
        var leads = await leadReadRepository.GetAllAsync(cancellationToken);
        
        return leads.Select(l => new LeadResponse
        {
            Id = l.Id,
            FullName = l.FullName,
            Email = l.Email,
            PhoneNumber = l.PhoneNumber,
            Score = l.Score,
            Status = l.Status,
            Source = l.Source,
            CreatedAt = l.CreatedAt ?? System.DateTimeOffset.MinValue,
            Activities = l.Activities.Select(a => new LeadActivityResponse
            {
                Id = a.Id,
                ActivityType = a.ActivityType,
                Description = a.Description,
                CreatedAt = a.CreatedAt ?? System.DateTimeOffset.MinValue
            }).ToList()
        }).ToList();
    }
}
