using Application.ApiContracts.Leads.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Lead.Lead;
using Domain.Constants.Lead;
using MediatR;
using System;

namespace Application.Features.Leads.Queries.GetLeadPipeline
{
    public class GetLeadPipelineQueryHandler(ILeadReadRepository leadRepository) : IRequestHandler<GetLeadPipelineQuery, Result<List<LeadPipelineGroupResponse>>>
    {
        public async Task<Result<List<LeadPipelineGroupResponse>>> Handle(
            GetLeadPipelineQuery request,
            CancellationToken cancellationToken)
        {
            var leads = await leadRepository.GetAllLeadsWithActivitiesAsync(cancellationToken).ConfigureAwait(false);
            var statuses = new[]
            {
                new { Key = LeadStatus.New, Display = "Mới" },
                new { Key = LeadStatus.Consulting, Display = "Đang tư vấn" },
                new { Key = LeadStatus.TestDriving, Display = "Đang lái thử" },
                new { Key = LeadStatus.Deposited, Display = "Đã đặt cọc" },
                new { Key = LeadStatus.Paperwork, Display = "Đang chờ giấy tờ" },
                new { Key = LeadStatus.Delivered, Display = "Đã giao xe" }
            };
            var result = statuses.Select(
                s => new LeadPipelineGroupResponse
                {
                    Status = s.Key,
                    StatusDisplayName = s.Display,
                    Leads =
                        [.. leads
                    .Where(l => string.Compare(l.Status, s.Key) == 0)
                                .Select(
                                    l => new LeadResponse
                                {
                                    Id = l.Id,
                                    FullName = l.FullName,
                                    PhoneNumber = l.PhoneNumber,
                                    Email = l.Email,
                                    Score = l.Score,
                                    Status = l.Status,
                                    Source = l.Source,
                                    InterestedVehicle = l.InterestedVehicle,
                                    Address = l.Address,
                                    AddressDetail = l.AddressDetail,
                                    Ward = l.Ward,
                                    District = l.District,
                                    Province = l.Province,
                                    Gender = l.Gender,
                                    Birthday = l.Birthday,
                                    IdentificationNumber = l.IdentificationNumber,
                                    CreatedAt = l.CreatedAt ?? DateTimeOffset.MinValue
                                })]
                })
                .ToList();
            return result;
        }
    }
}
