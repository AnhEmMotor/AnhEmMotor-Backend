using Application.ApiContracts.Leads.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Lead;
using MediatR;
using System.Linq;

namespace Application.Features.Leads.Queries.GetLeadById;

public record GetLeadByIdQuery(int Id) : IRequest<Result<LeadResponse>>;

public class GetLeadByIdQueryHandler(ILeadReadRepository leadReadRepository) : IRequestHandler<GetLeadByIdQuery, Result<LeadResponse>>
{
    public async Task<Result<LeadResponse>> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
    {
        var lead = await leadReadRepository.GetByIdAsync(request.Id, cancellationToken);
        if (lead == null)
            return Result<LeadResponse>.Failure("Không tìm thấy khách hàng.");
        return Result<LeadResponse>.Success(
            new LeadResponse
            {
                Id = lead.Id,
                FullName = lead.FullName,
                Email = lead.Email,
                PhoneNumber = lead.PhoneNumber,
                Score = lead.Score,
                Status = lead.Status,
                Source = lead.Source,
                InterestedVehicle = lead.InterestedVehicle,
                Address = lead.Address,
                AddressDetail = lead.AddressDetail,
                Ward = lead.Ward,
                District = lead.District,
                Province = lead.Province,
                Gender = lead.Gender,
                Birthday = lead.Birthday,
                IdentificationNumber = lead.IdentificationNumber,
                CreatedAt = lead.CreatedAt ?? DateTimeOffset.MinValue,
                Activities =
                    lead.Activities?.Select(
                                a => new LeadActivityResponse
                                {
                                    Id = a.Id,
                                    ActivityType = a.ActivityType,
                                    Description = a.Description,
                                    CreatedAt = a.CreatedAt ?? DateTimeOffset.MinValue
                                })
                                .ToList() ??
                            []
            });
    }
}
