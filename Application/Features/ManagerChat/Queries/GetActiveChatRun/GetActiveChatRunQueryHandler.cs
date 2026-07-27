using Application.Common.Models;
using Application.DTOs.Chat;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.ManagerChat.Queries.GetActiveChatRun;

public class GetActiveChatRunQueryHandler(
    IChatReadRepository chatReadRepository,
    IPermissionReadRepository permissionReadRepository,
    ICurrentUserContext currentUserContext) : IRequestHandler<GetActiveChatRunQuery, Result<ActiveRunDto?>>
{
    public async Task<Result<ActiveRunDto?>> Handle(GetActiveChatRunQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.GetUserId();
        bool hasPermission = await permissionReadRepository.HasAnyPermissionAsync(userId, cancellationToken);
        if (!hasPermission)
        {
            return Result<ActiveRunDto?>.Failure(Error.Forbidden());
        }

        var session = await chatReadRepository.GetSessionByIdAsync(request.SessionId, cancellationToken);
        if (session == null || session.UserId != userId)
        {
            return Result<ActiveRunDto?>.Failure(Error.NotFound("Session not found or forbidden"));
        }

        var activeRun = await chatReadRepository.GetActiveRunForUserAsync(userId, cancellationToken);
        
        if (activeRun == null || activeRun.SessionId != request.SessionId)
        {
            return Result<ActiveRunDto?>.Success(null);
        }

        var dto = new ActiveRunDto(
            activeRun.Id,
            activeRun.Status,
            activeRun.LastSeq,
            activeRun.StartedAt,
            activeRun.UserMessage,
            activeRun.PartialOutput
        );

        return Result<ActiveRunDto?>.Success(dto);
    }
}
