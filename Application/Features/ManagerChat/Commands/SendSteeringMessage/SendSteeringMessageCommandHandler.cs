using System.Text.Json;
using Application.Common.Models;
using Application.DTOs.Chat;
using Application.Features.ManagerChat.Commands.StartChatRun;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using MediatR;

namespace Application.Features.ManagerChat.Commands.SendSteeringMessage;

public class SendSteeringMessageCommandHandler(
    IChatReadRepository chatReadRepository,
    IChatInsertRepository chatInsertRepository,
    IPermissionReadRepository permissionReadRepository,
    IChatRunWriter chatRunWriter,
    IChatRunCancellationRegistry cancellationRegistry,
    ISidecarStreamClient sidecarStreamClient,
    ISender sender,
    IUnitOfWork unitOfWork) : IRequestHandler<SendSteeringMessageCommand, Result<SteeringResultDto>>
{
    private const int MaxPendingSteering = 5;

    public async Task<Result<SteeringResultDto>> Handle(SendSteeringMessageCommand request, CancellationToken cancellationToken)
    {
        var hasPermission = await permissionReadRepository.HasAnyPermissionAsync(request.UserId, cancellationToken);
        if (!hasPermission) return Result<SteeringResultDto>.Failure(Error.Forbidden());

        var run = await chatReadRepository.GetRunByIdAsync(request.RunId, cancellationToken);
        if (run == null || run.Session?.UserId != request.UserId)
            return Result<SteeringResultDto>.Failure(Error.NotFound("Run không tồn tại hoặc không thuộc quyền sở hữu."));

        if (run.Status is not (ChatRunStatus.Running or ChatRunStatus.Pending))
        {
            // Run vừa kết thúc đúng lúc user gửi tiếp — tự tạo run mới, không báo lỗi.
            return await StartNewRunAsync(run.SessionId, request, cancellationToken);
        }

        var mode = SteeringClassifier.Classify(request.Content) ?? ChatSteeringMode.Queue;

        if (mode != ChatSteeringMode.Restart)
        {
            var steeringCount = await chatReadRepository.CountSteeringMessagesAsync(request.RunId, cancellationToken);
            if (steeringCount >= MaxPendingSteering)
            {
                return Result<SteeringResultDto>.Failure(Error.Validation(
                    "Đã gửi quá nhiều đính chính cho lần trả lời này. Hãy bấm Dừng và hỏi lại từ đầu."));
            }
        }

        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = run.SessionId,
            Role = ChatRole.User,
            Message = request.Content,
            IsSteering = true,
            RunId = request.RunId,
            CreatedAt = DateTime.UtcNow,
        };
        chatInsertRepository.AddMessage(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await chatRunWriter.AppendAsync(request.RunId, ChatRunEventType.SteeringReceived,
            JsonSerializer.Serialize(new SteeringQueueItem(request.Content, mode)));

        if (mode == ChatSteeringMode.Restart)
        {
            return await RestartRunAsync(run, request, cancellationToken);
        }

        var appendResult = await chatRunWriter.AppendPendingSteeringAsync(
            request.RunId, new SteeringQueueItem(request.Content, mode), MaxPendingSteering);

        return appendResult switch
        {
            PendingSteeringAppendResult.Appended =>
                Result<SteeringResultDto>.Success(new SteeringResultDto(request.RunId, mode)),
            PendingSteeringAppendResult.RunNotActive =>
                await StartNewRunAsync(run.SessionId, request, cancellationToken),
            PendingSteeringAppendResult.TooMany =>
                Result<SteeringResultDto>.Failure(Error.Validation(
                    "Đã gửi quá nhiều đính chính cho lần trả lời này. Hãy bấm Dừng và hỏi lại từ đầu.")),
            _ => Result<SteeringResultDto>.Failure(Error.Failure("Hệ thống đang bận, vui lòng thử lại.")),
        };
    }

    private async Task<Result<SteeringResultDto>> StartNewRunAsync(
        Guid sessionId, SendSteeringMessageCommand request, CancellationToken cancellationToken)
    {
        var newRunId = await sender.Send(
            new StartChatRunCommand(sessionId, request.Content, request.UserId, request.Token), cancellationToken);
        return Result<SteeringResultDto>.Success(new SteeringResultDto(newRunId, ChatSteeringMode.Restart));
    }

    private async Task<Result<SteeringResultDto>> RestartRunAsync(
        ChatRun run, SendSteeringMessageCommand request, CancellationToken cancellationToken)
    {
        await sidecarStreamClient.CancelAsync(run.Id, cancellationToken);
        cancellationRegistry.TryCancel(run.Id);

        // ponytail: chờ ChatRunExecutor tự ghi trạng thái Cancelled thay vì ghi đè trực tiếp ở đây —
        // tránh ghi trùng tin nhắn AI khi executor cũng đang ghi cùng lúc. Nâng cấp bằng tín hiệu
        // hoàn tất huỷ thay vì polling nếu 2s không đủ trong thực tế.
        for (var attempt = 0; attempt < 20; attempt++)
        {
            try
            {
                return await StartNewRunAsync(run.SessionId, request, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                await Task.Delay(100, cancellationToken);
            }
        }
        return Result<SteeringResultDto>.Failure(Error.Failure("Không thể khởi động lại phiên trả lời, vui lòng thử lại."));
    }
}
