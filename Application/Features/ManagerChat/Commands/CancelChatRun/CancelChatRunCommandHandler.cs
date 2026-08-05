using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.ManagerChat.Commands.CancelChatRun;

public class CancelChatRunCommandHandler(
    IChatReadRepository chatReadRepository,
    IPermissionReadRepository permissionReadRepository,
    IChatRunCancellationRegistry cancellationRegistry,
    ISidecarStreamClient sidecarStreamClient) : IRequestHandler<CancelChatRunCommand>
{
    public async Task Handle(CancelChatRunCommand request, CancellationToken cancellationToken)
    {
        bool hasPermission = await permissionReadRepository.HasAnyPermissionAsync(request.UserId, cancellationToken);
        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("Forbidden");
        }
        var run = await chatReadRepository.GetRunByIdAsync(request.RunId, cancellationToken);
        if (run == null || run.Session?.UserId != request.UserId)
        {
            throw new InvalidOperationException("Run không tồn tại hoặc không thuộc quyền sở hữu.");
        }
        await sidecarStreamClient.CancelAsync(request.RunId, cancellationToken);
        cancellationRegistry.TryCancel(request.RunId);
    }
}
