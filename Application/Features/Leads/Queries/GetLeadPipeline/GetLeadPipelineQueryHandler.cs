using Application.ApiContracts.Leads.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Lead;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Leads.Queries.GetLeadPipeline
{
    public sealed class GetLeadPipelineQueryHandler(ILeadReadRepository leadRepository) : IRequestHandler<GetLeadPipelineQuery, Result<List<LeadPipelineGroupResponse>>>
    {
        public async Task<Result<List<LeadPipelineGroupResponse>>> Handle(
            GetLeadPipelineQuery request,
            CancellationToken cancellationToken)
        {
            var leads = await leadRepository.GetAllLeadsWithActivitiesAsync(cancellationToken).ConfigureAwait(false);
            var statuses = new[]
            {
                new { Key = "New", Display = "Mới" },
                new { Key = "Consulting", Display = "Đang tư vấn" },
                new { Key = "TestDriving", Display = "Đang lái thử" },
                new { Key = "Deposited", Display = "Đã đặt cọc" },
                new { Key = "Paperwork", Display = "Đang chờ giấy tờ" },
                new { Key = "Delivered", Display = "Đã giao xe" }
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
