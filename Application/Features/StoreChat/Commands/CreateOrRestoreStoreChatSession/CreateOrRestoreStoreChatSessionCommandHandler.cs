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
            // Phiên cũ cùng VisitorKey có thể đã bị nhân viên xoá mềm (vẫn còn trong DB, chỉ ẩn
            // khỏi truy vấn) — VisitorKey vẫn giữ unique index nên phải giải phóng trước khi tạo
            // phiên mới, nếu không insert bên dưới sẽ đụng khoá duy nhất.
            var deletedSession = await storeChatReadRepository
                .GetDeletedSessionByVisitorKeyAsync(request.VisitorKey, cancellationToken);
            // VisitorKey là nvarchar(64) — không nối thêm gốc vào vì khách có thể đã dùng gần hết độ
            // dài đó; Id của chính phiên đã đủ để đảm bảo duy nhất, phần audit dựa vào Id/DeletedAt
            // chứ không cần đọc lại VisitorKey gốc.
            if (deletedSession != null)
            {
                deletedSession.VisitorKey = $"deleted-{deletedSession.Id:N}";
            }

            session = new StoreChatSession
            {
                VisitorKey = request.VisitorKey,
                LastMessageAt = DateTime.UtcNow
            };

            // Khách bấm "Xoá cuộc trò chuyện" — VisitorKey đổi mới nên luôn rơi vào nhánh tạo phiên
            // này; liên kết lại phiên cũ để quản trị lần theo được, và giữ nguyên Tên/SĐT đã có để
            // khách không phải điền lại.
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
