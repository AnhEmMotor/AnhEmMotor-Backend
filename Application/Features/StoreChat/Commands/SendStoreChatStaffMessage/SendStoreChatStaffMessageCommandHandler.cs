using Application.Common.Models;
using Application.DTOs.StoreChat;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.StoreChat;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using MediatR;

namespace Application.Features.StoreChat.Commands.SendStoreChatStaffMessage;

public class SendStoreChatStaffMessageCommandHandler(
    IStoreChatReadRepository storeChatReadRepository,
    IStoreChatInsertRepository storeChatInsertRepository,
    IStoreChatUpdateRepository storeChatUpdateRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SendStoreChatStaffMessageCommand, Result<SendStaffMessageResultDto>>
{
    public async Task<Result<SendStaffMessageResultDto>> Handle(SendStoreChatStaffMessageCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content) && string.IsNullOrWhiteSpace(request.CardsJson))
        {
            return Error.Validation("Nội dung tin nhắn không được để trống.");
        }

        var session = await storeChatReadRepository.GetSessionByIdAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            return Error.NotFound("Phiên chat không tồn tại.");
        }

        var staffId = currentUserContext.GetUserId();
        if (session.Mode == StoreChatMode.Human && session.AssignedStaffId != staffId)
        {
            return Error.Forbidden("Phiên đang có nhân viên khác phụ trách.");
        }

        // Gộp bước "Nhận" cũ vào đây: tự nhận phiên (Ai/Waiting -> Human) ngay khi gửi tin nhắn đầu
        // tiên; race-safe nếu 2 nhân viên cùng gửi gần như đồng thời, chỉ 1 người thắng.
        var assigned = await storeChatUpdateRepository.TryAssignStaffAsync(request.SessionId, staffId, cancellationToken);
        if (!assigned)
        {
            return Error.Conflict("Nhân viên khác vừa nhận phiên này, vui lòng thử lại.");
        }

        var message = new StoreChatMessage
        {
            SessionId = session.Id,
            Sender = StoreChatSender.Staff,
            Content = request.Content,
            CardsJson = request.CardsJson
        };
        storeChatInsertRepository.AddMessage(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var staffName = await storeChatReadRepository.GetStaffNameAsync(staffId, cancellationToken) ?? "Nhân viên";

        return new SendStaffMessageResultDto(
            new StoreChatMessageDto
            {
                Id = message.Id,
                Sender = message.Sender,
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                CardsJson = message.CardsJson
            },
            staffName);
    }
}
