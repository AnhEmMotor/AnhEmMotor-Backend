using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.ManagerChat.Commands.DeleteManagerChatSession;

public class DeleteManagerChatSessionCommandHandler(
    IChatReadRepository chatReadRepository,
    IChatDeleteRepository chatDeleteRepository,
    IPermissionReadRepository permissionReadRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManagerChatSessionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteManagerChatSessionCommand request, CancellationToken cancellationToken)
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
        chatDeleteRepository.DeleteSession(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
