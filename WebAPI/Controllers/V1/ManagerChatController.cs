using Application.ApiContracts.ManagerChat.Requests;
using Application.Features.ManagerChat.Commands.ApproveChatPlan;
using Application.Features.ManagerChat.Commands.CreateChatFeedback;
using Application.Features.ManagerChat.Commands.CreateManagerChatSession;
using Application.Features.ManagerChat.Commands.DeleteManagerChatSession;
using Application.Features.ManagerChat.Commands.RejectChatPlan;
using Application.Features.ManagerChat.Commands.SendPlanChatMessage;
using Application.Features.ManagerChat.Commands.UpdateChatPlan;
using Application.Features.ManagerChat.Commands.UpdateManagerChatSession;
using Application.Features.ManagerChat.Queries.GetActiveChatRun;
using Application.Features.ManagerChat.Queries.GetChatPlan;
using Application.Features.ManagerChat.Queries.GetChatToolCatalog;
using Application.Features.ManagerChat.Queries.GetManagerChatSessionHistory;
using Application.Features.ManagerChat.Queries.GetManagerChatSessions;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[SwaggerTag("Quản lý tương tác với AI Chat Manager")]
[Route("api/v{version:apiVersion}/manager-chat")]
[Authorize]
public class ManagerChatController(ISender sender) : ApiController
{
    [HttpGet("sessions")]
    [SwaggerOperation(Summary = "Lấy danh sách các phiên chat của người dùng")]
    public async Task<IActionResult> GetSessions(CancellationToken cancellationToken)
    {
        var query = new GetManagerChatSessionsQuery();
        var result = await sender.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("sessions")]
    [SwaggerOperation(Summary = "Tạo mới phiên chat")]
    public async Task<IActionResult> CreateSession([FromBody] CreateManagerChatSessionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateManagerChatSessionCommand(request.Title, request.InitialMessage);
        var result = await sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("sessions/{id}")]
    [SwaggerOperation(Summary = "Xóa phiên chat")]
    public async Task<IActionResult> DeleteSession(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteManagerChatSessionCommand(id);
        var result = await sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("sessions/{id}")]
    [SwaggerOperation(Summary = "Cập nhật tiêu đề phiên chat")]
    public async Task<IActionResult> UpdateSession(Guid id, [FromBody] UpdateManagerChatSessionRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateManagerChatSessionCommand(id, request.Title);
        var result = await sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("sessions/{id}/history")]
    [SwaggerOperation(Summary = "Lấy lịch sử tin nhắn trong phiên chat")]
    public async Task<IActionResult> GetSessionHistory(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetManagerChatSessionHistoryQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("sessions/{id}/active-run")]
    [SwaggerOperation(Summary = "Lấy run đang chạy (nếu có) của phiên chat, dùng để khôi phục khi mở lại")]
    public async Task<IActionResult> GetActiveRun(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetActiveChatRunQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("runs/{runId}/feedback")]
    [SwaggerOperation(Summary = "Ghi nhận phản hồi \"Số liệu chưa đúng\" cho một run chat (Stage 16.9)")]
    public async Task<IActionResult> CreateFeedback(Guid runId, [FromBody] CreateChatFeedbackRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateChatFeedbackCommand(runId, request.Comment);
        var result = await sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("tool-catalog")]
    [SwaggerOperation(Summary = "Lấy tên hiển thị tiếng Việt của các tool AI dùng khi tra cứu dữ liệu, để FE hiện trạng thái đang xử lý")]
    public async Task<IActionResult> GetToolCatalog(CancellationToken cancellationToken)
    {
        var query = new GetChatToolCatalogQuery();
        var result = await sender.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("runs/{runId}/plan")]
    [SwaggerOperation(Summary = "Lấy kế hoạch (Plan Mode) hiện tại của một run chat (Stage 10)")]
    public async Task<IActionResult> GetPlan(Guid runId, CancellationToken cancellationToken)
    {
        var query = new GetChatPlanQuery(runId);
        var result = await sender.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPatch("runs/{runId}/plan")]
    [SwaggerOperation(Summary = "Sửa kế hoạch: thêm/sửa/xoá/đổi thứ tự bước (Stage 10)")]
    public async Task<IActionResult> UpdatePlan(Guid runId, [FromBody] UpdateChatPlanRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateChatPlanCommand(runId, request.Version, request.Operations);
        var result = await sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("runs/{runId}/plan/approve")]
    [SwaggerOperation(Summary = "Duyệt kế hoạch và bắt đầu thực thi (Stage 10)")]
    public async Task<IActionResult> ApprovePlan(Guid runId, [FromBody] ChatPlanVersionRequest request, CancellationToken cancellationToken)
    {
        var command = new ApproveChatPlanCommand(runId, request.Version);
        var result = await sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("runs/{runId}/plan/reject")]
    [SwaggerOperation(Summary = "Huỷ kế hoạch đang chờ duyệt (Stage 10)")]
    public async Task<IActionResult> RejectPlan(Guid runId, CancellationToken cancellationToken)
    {
        var command = new RejectChatPlanCommand(runId);
        var result = await sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("runs/{runId}/plan/chat")]
    [SwaggerOperation(Summary = "Chat để duyệt/huỷ/sửa/bình luận kế hoạch, thay cho nút bấm (Stage 10.9)")]
    public async Task<IActionResult> SendPlanChat(Guid runId, [FromBody] SendPlanChatRequest request, CancellationToken cancellationToken)
    {
        var command = new SendPlanChatMessageCommand(runId, request.Content, request.TargetStepId);
        var result = await sender.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
