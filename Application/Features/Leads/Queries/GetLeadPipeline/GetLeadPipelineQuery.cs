using Application.ApiContracts.Leads.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Lead;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Leads.Queries.GetLeadPipeline;

public sealed class GetLeadPipelineQuery : IRequest<Result<List<LeadPipelineGroupResponse>>>
{
}

public class LeadPipelineGroupResponse
{
    public string Status { get; set; } = string.Empty;
    public string StatusDisplayName { get; set; } = string.Empty;
    public List<LeadResponse> Leads { get; set; } = new();
}

public sealed class GetLeadPipelineQueryHandler(ILeadReadRepository leadRepository) 
    : IRequestHandler<GetLeadPipelineQuery, Result<List<LeadPipelineGroupResponse>>>
{
    public async Task<Result<List<LeadPipelineGroupResponse>>> Handle(GetLeadPipelineQuery request, CancellationToken cancellationToken)
    {
        var leads = await leadRepository.GetAllLeadsWithActivitiesAsync(cancellationToken);
        
        var statuses = new[]
        {
            new { Key = "New", Display = "Mới" },
            new { Key = "Consulting", Display = "Đang tư vấn" },
            new { Key = "TestDriving", Display = "Đang lái thử" },
            new { Key = "Deposited", Display = "Đã đặt cọc" },
            new { Key = "Paperwork", Display = "Đang chờ giấy tờ" },
            new { Key = "Delivered", Display = "Đã giao xe" }
        };

        var result = statuses.Select(s => new LeadPipelineGroupResponse
        {
            Status = s.Key,
            StatusDisplayName = s.Display,
            Leads = leads
                .Where(l => l.Status == s.Key)
                .Select(l => new LeadResponse
                {
                    Id = l.Id,
                    FullName = l.FullName,
                    PhoneNumber = l.PhoneNumber,
                    Email = l.Email,
                    Score = l.Score,
                    Status = l.Status,
                    Source = l.Source,
                    Address = l.Address,
                    AddressDetail = l.AddressDetail,
                    Ward = l.Ward,
                    District = l.District,
                    Province = l.Province,
                    Gender = l.Gender,
                    Birthday = l.Birthday,
                    IdentificationNumber = l.IdentificationNumber,
                    CreatedAt = l.CreatedAt ?? DateTimeOffset.MinValue
                }).ToList()
        })
        .ToList();

        return result;
    }
}
