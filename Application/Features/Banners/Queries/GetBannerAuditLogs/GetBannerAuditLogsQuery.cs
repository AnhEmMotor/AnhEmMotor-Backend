using Application.Common.Models;
using Application.Interfaces.Repositories.Banner;
using Domain.Entities;
using MediatR;

namespace Application.Features.Banners.Queries.GetBannerAuditLogs
{
    public record GetBannerAuditLogsQuery(int BannerId) : IRequest<Result<List<BannerAuditLog>>>;

    
}
