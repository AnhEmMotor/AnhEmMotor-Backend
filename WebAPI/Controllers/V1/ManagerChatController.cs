using Application.ApiContracts.ManagerChat.Requests;
using Application.Features.ManagerChat.Commands.CreateManagerChatSession;
using Application.Features.ManagerChat.Commands.DeleteManagerChatSession;
using Application.Features.ManagerChat.Commands.UpdateManagerChatSession;
using Application.Features.ManagerChat.Commands.SendManagerChatMessage;
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

    [HttpPost("sessions/{id}/message")]
    [SwaggerOperation(Summary = "Gửi tin nhắn trong phiên chat")]
    public async Task<IActionResult> SendMessage(Guid id, [FromBody] SendManagerChatMessageRequest request, CancellationToken cancellationToken)
    {
        var command = new SendManagerChatMessageCommand(id, request.Content);
        var result = await sender.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
