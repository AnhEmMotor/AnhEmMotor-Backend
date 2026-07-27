using Application.Common.Models;
using Application.DTOs.Chat;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Constants;
using MediatR;

namespace Application.Features.ManagerChat.Queries.GetChatRunEvents;

public class GetChatRunEventsQueryHandler(
    IChatReadRepository chatReadRepository,
    IPermissionReadRepository permissionReadRepository,
    ICurrentUserContext currentUserContext) : IRequestHandler<GetChatRunEventsQuery, Result<ChatRunEventsResult>>
{
    public async Task<Result<ChatRunEventsResult>> Handle(GetChatRunEventsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.GetUserId();
        bool hasPermission = await permissionReadRepository.HasAnyPermissionAsync(userId, cancellationToken);
        if (!hasPermission)
        {
            return Result<ChatRunEventsResult>.Failure(Error.Forbidden());
        }

        var run = await chatReadRepository.GetRunByIdAsync(request.RunId, cancellationToken);
        if (run == null || run.Session?.UserId != userId)
        {
            return Result<ChatRunEventsResult>.Failure(Error.NotFound("Run not found or forbidden"));
        }

        var events = await chatReadRepository.GetRunEventsAsync(request.RunId, request.AfterSeq, cancellationToken);
        
        var dtos = events.Select(e => new ChatRunEventDto(e.Seq, e.Type, e.Payload)).ToList();
        
        bool isTerminal = run.Status == ChatRunStatus.Completed || 
                          run.Status == ChatRunStatus.Cancelled || 
                          run.Status == ChatRunStatus.Failed || 
                          run.Status == ChatRunStatus.Orphaned;

        var result = new ChatRunEventsResult(dtos, isTerminal);
        
        return Result<ChatRunEventsResult>.Success(result);
    }
}
