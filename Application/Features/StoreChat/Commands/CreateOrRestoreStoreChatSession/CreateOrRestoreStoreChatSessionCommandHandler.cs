using Application.Common.Models;
using Application.DTOs.StoreChat;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.StoreChat;
using Domain.Entities;
using MediatR;

namespace Application.Features.StoreChat.Commands.CreateOrRestoreStoreChatSession;

public class CreateOrRestoreStoreChatSessionCommandHandler(
    IStoreChatReadRepository storeChatReadRepository,
    IStoreChatInsertRepository storeChatInsertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOrRestoreStoreChatSessionCommand, Result<StoreChatSessionDto>>
{
    public async Task<Result<StoreChatSessionDto>> Handle(
        CreateOrRestoreStoreChatSessionCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.VisitorKey))
        {
            return Error.Validation("VisitorKey không được để trống.");
        }
        var session = await storeChatReadRepository.GetSessionByVisitorKeyAsync(request.VisitorKey, cancellationToken);
        if (session == null)
        {
            var deletedSession = await storeChatReadRepository
                .GetDeletedSessionByVisitorKeyAsync(request.VisitorKey, cancellationToken);
            if (deletedSession != null)
            {
                deletedSession.VisitorKey = $"deleted-{deletedSession.Id:N}";
            }
            session = new StoreChatSession { VisitorKey = request.VisitorKey, LastMessageAt = DateTime.UtcNow };
            if (request.PreviousSessionId.HasValue)
            {
                var previousSession = await storeChatReadRepository
                    .GetSessionByIdAsync(request.PreviousSessionId.Value, cancellationToken);
                if (previousSession != null)
                {
                    session.PreviousSessionId = previousSession.Id;
                    session.ContactName = previousSession.ContactName;
                    session.ContactPhone = previousSession.ContactPhone;
                }
            }
            storeChatInsertRepository.AddSession(session);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        var staffName = session.AssignedStaffId.HasValue
            ? await storeChatReadRepository.GetStaffNameAsync(session.AssignedStaffId.Value, cancellationToken)
            : null;
        return new StoreChatSessionDto
        {
            Id = session.Id,
            VisitorKey = session.VisitorKey,
            Mode = session.Mode,
            ContactName = session.ContactName,
            ContactPhone = session.ContactPhone,
            AssignedStaffName = staffName
        };
    }
}
