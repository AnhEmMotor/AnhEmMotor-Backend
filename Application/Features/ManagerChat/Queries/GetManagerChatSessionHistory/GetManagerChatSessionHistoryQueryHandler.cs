using Application.Common.Models;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.ManagerChat.Queries.GetManagerChatSessionHistory;

public class GetManagerChatSessionHistoryQueryHandler(
    IChatReadRepository chatReadRepository,
    IPermissionReadRepository permissionReadRepository,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<GetManagerChatSessionHistoryQuery, Result<List<ManagerChatMessageDto>>>
{
    public async Task<Result<List<ManagerChatMessageDto>>> Handle(GetManagerChatSessionHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.GetUserId();
        bool hasPermission = await permissionReadRepository.HasAnyPermissionAsync(userId, cancellationToken);
        if (!hasPermission)
        {
            return Error.Forbidden();
        }

        var session = await chatReadRepository.GetSessionByIdAsync(request.SessionId, cancellationToken);
        if (session == null || session.UserId != userId)
        {
            return Error.NotFound("Phiên chat không tồn tại hoặc không thuộc quyền sở hữu.");
        }

        var messages = await chatReadRepository.GetMessagesBySessionIdAsync(request.SessionId, cancellationToken);
        
        var dtos = messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ManagerChatMessageDto
            {
                Role = m.Role.ToString(),
                Message = m.Message,
                CreatedAt = m.CreatedAt
            }).ToList();

        return dtos;
    }
}
