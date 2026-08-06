using Application.Common.Models;
using Application.DTOs.StoreChat;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.StoreChat;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.StoreChat.Commands.RequestHandoff;

public class RequestHandoffCommandHandler(
    IStoreChatReadRepository storeChatReadRepository,
    IStoreChatUpdateRepository storeChatUpdateRepository,
    IStoreChatInsertRepository storeChatInsertRepository,
    IUnitOfWork unitOfWork,
    ILogger<RequestHandoffCommandHandler> logger) : IRequestHandler<RequestHandoffCommand, Result<RequestHandoffResultDto>>
{
    public async Task<Result<RequestHandoffResultDto>> Handle(
        RequestHandoffCommand request,
        CancellationToken cancellationToken)
    {
        var session = await storeChatReadRepository.GetSessionByIdAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            return Error.NotFound("Phiên chat không tồn tại.");
        }
        StoreChatMessage? systemMessage = null;
        if (session.Mode == StoreChatMode.Ai)
        {
            session.Mode = StoreChatMode.Waiting;
            logger.LogInformation(
                "[StoreChat] Action=RequestHandoff SessionId={SessionId} TriggeredBy={TriggeredBy}",
                request.SessionId,
                request.TriggeredBy);
            if (request.TriggeredBy == "Ai")
            {
                systemMessage = new StoreChatMessage
                {
                    SessionId = session.Id,
                    Sender = StoreChatSender.System,
                    Content = "Đã chuyển phiên cho nhân viên hỗ trợ, vui lòng chờ trong giây lát."
                };
                storeChatInsertRepository.AddMessage(systemMessage);
            }
        }
        if (!string.IsNullOrWhiteSpace(request.ContactName))
        {
            session.ContactName = request.ContactName;
        }
        if (!string.IsNullOrWhiteSpace(request.ContactPhone))
        {
            session.ContactPhone = request.ContactPhone;
        }
        storeChatUpdateRepository.UpdateSession(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var systemMessageDto = systemMessage == null
            ? null
            : new StoreChatMessageDto
            {
                Id = systemMessage.Id,
                Sender = systemMessage.Sender,
                Content = systemMessage.Content,
                CreatedAt = systemMessage.CreatedAt,
                CardsJson = systemMessage.CardsJson
            };
        return new RequestHandoffResultDto(true, systemMessageDto);
    }
}
