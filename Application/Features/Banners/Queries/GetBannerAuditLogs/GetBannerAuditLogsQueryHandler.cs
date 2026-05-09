using Application.Common.Models;
using Application.Interfaces.Repositories.Banner;
using Domain.Entities;
using MediatR;
using System;

namespace Application.Features.Banners.Queries.GetBannerAuditLogs
{
    public class GetBannerAuditLogsQueryHandler(IBannerAuditRepository auditRepository) : IRequestHandler<GetBannerAuditLogsQuery, Result<List<BannerAuditLog>>>
    {
        public async Task<Result<List<BannerAuditLog>>> Handle(
            GetBannerAuditLogsQuery request,
            CancellationToken cancellationToken)
        {
            var logs = await auditRepository.GetLogsByBannerIdAsync(request.BannerId, cancellationToken)
                .ConfigureAwait(false);
            return Result<List<BannerAuditLog>>.Success(logs);
        }
    }
}
