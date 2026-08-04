using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.ManagerChat.Commands.CreateChatFeedback;

public class CreateChatFeedbackCommandHandler(
    IChatReadRepository chatReadRepository,
    IChatInsertRepository chatInsertRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateChatFeedbackCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateChatFeedbackCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.GetUserId();
        var run = await chatReadRepository.GetRunByIdAsync(request.ChatRunId, cancellationToken);
        if (run == null || run.Session?.UserId != userId)
        {
            return Result<Guid>.Failure(Error.NotFound("Không tìm thấy run hoặc không thuộc về bạn"));
        }
        var feedback = new ChatFeedback
        {
            ChatRunId = request.ChatRunId,
            Comment = request.Comment,
            ReportedBy = userId
        };
        chatInsertRepository.AddFeedback(feedback);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return feedback.Id;
    }
}
