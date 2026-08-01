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
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateOrRestoreStoreChatSessionCommand, Result<StoreChatSessionDto>>
{
    public async Task<Result<StoreChatSessionDto>> Handle(CreateOrRestoreStoreChatSessionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.VisitorKey))
        {
            return Error.Validation("VisitorKey không được để trống.");
        }

        var session = await storeChatReadRepository.GetSessionByVisitorKeyAsync(request.VisitorKey, cancellationToken);
        if (session == null)
        {
            session = new StoreChatSession
            {
                VisitorKey = request.VisitorKey,
                LastMessageAt = DateTime.UtcNow
            };
            storeChatInsertRepository.AddSession(session);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new StoreChatSessionDto
        {
            Id = session.Id,
            VisitorKey = session.VisitorKey,
            Mode = session.Mode
        };
    }
}
