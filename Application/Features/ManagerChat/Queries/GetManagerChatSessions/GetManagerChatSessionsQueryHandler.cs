using Application.Common.Models;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.ManagerChat.Queries.GetManagerChatSessions;

public class GetManagerChatSessionsQueryHandler(
    IChatReadRepository chatReadRepository,
    IPermissionReadRepository permissionReadRepository,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<GetManagerChatSessionsQuery, Result<List<ManagerChatSessionDto>>>
{
    public async Task<Result<List<ManagerChatSessionDto>>> Handle(GetManagerChatSessionsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.GetUserId();
        bool hasPermission = await permissionReadRepository.HasAnyPermissionAsync(userId, cancellationToken);
        if (!hasPermission)
        {
            return Result<List<ManagerChatSessionDto>>.Failure(Error.Forbidden());
        }

        var sessions = await chatReadRepository.GetSessionsByUserIdAsync(userId, cancellationToken);
        
        var dtos = sessions
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new ManagerChatSessionDto
            {
                Id = s.Id,
                Title = s.Title
            })
            .ToList();

        return Result<List<ManagerChatSessionDto>>.Success(dtos);
    }
}
