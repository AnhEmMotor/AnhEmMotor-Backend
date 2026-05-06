using Application.Interfaces.Repositories.Banner;
using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Features.Banners.Queries.GetBannerAuditLogs
{
    public record GetBannerAuditLogsQuery(int BannerId) : IRequest<Result<List<BannerAuditLog>>>;

    public class GetBannerAuditLogsQueryHandler(IBannerAuditRepository auditRepository) 
        : IRequestHandler<GetBannerAuditLogsQuery, Result<List<BannerAuditLog>>>
    {
        public async Task<Result<List<BannerAuditLog>>> Handle(GetBannerAuditLogsQuery request, CancellationToken cancellationToken)
        {
            var logs = await auditRepository.GetLogsByBannerIdAsync(request.BannerId, cancellationToken);
            return Result<List<BannerAuditLog>>.Success(logs);
        }
    }
}
