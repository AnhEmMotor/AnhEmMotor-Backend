using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using MediatR;

namespace Application.Features.ManagerChat.Commands.StartChatRun;

public class StartChatRunCommandHandler(
    IChatReadRepository chatReadRepository,
    IChatInsertRepository chatInsertRepository,
    IPermissionReadRepository permissionReadRepository,
    IChatRunQueue chatRunQueue,
    IChatRunTokenStore tokenStore,
    IUnitOfWork unitOfWork) : IRequestHandler<StartChatRunCommand, Guid>
{
    public async Task<Guid> Handle(StartChatRunCommand request, CancellationToken cancellationToken)
    {
        bool hasPermission = await permissionReadRepository.HasAnyPermissionAsync(request.UserId, cancellationToken);
        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("Forbidden");
        }

        var session = await chatReadRepository.GetSessionByIdAsync(request.SessionId, cancellationToken);
        if (session == null || session.UserId != request.UserId)
        {
            throw new InvalidOperationException("Phiên chat không tồn tại hoặc không thuộc quyền sở hữu.");
        }

        var activeRun = await chatReadRepository.GetActiveRunForUserAsync(request.UserId, cancellationToken);
        if (activeRun != null)
        {
            throw new InvalidOperationException("Đang có một tiến trình AI khác đang chạy. Vui lòng chờ hoặc huỷ tiến trình hiện tại.");
        }

        var userMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = request.SessionId,
            Role = ChatRole.User,
            Message = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        var runId = Guid.NewGuid();
        var run = new ChatRun
        {
            Id = runId,
            SessionId = request.SessionId,
            Status = ChatRunStatus.Pending,
            UserMessage = request.Content,
            StartedAt = DateTime.UtcNow
        };

        chatInsertRepository.AddMessage(userMessage);
        chatInsertRepository.AddRun(run);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        tokenStore.Store(runId, request.Token);
        await chatRunQueue.EnqueueAsync(runId, cancellationToken);

        return runId;
    }
}
