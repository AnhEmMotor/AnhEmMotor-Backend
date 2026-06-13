using Application.ApiContracts.Contacts.Requests;
using Application.Features.Contacts.Commands.CreateContact;
using Application.Features.Contacts.Commands.CreateContactReply;
using Application.Features.Contacts.Commands.CreateFeedback;
using Application.Features.Contacts.Commands.CreateJobApplication;
using Application.Features.Contacts.Commands.CreateSupportRequest;
using Application.Features.Contacts.Commands.UpdateContactStatus;
using Application.Features.Contacts.Commands.UpdateInternalNote;
using Application.Features.Contacts.Queries.GetContacts;
using Application.Features.Contacts.Queries.GetPaginatedContacts;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller quản lý liên hệ khách hàng (Support, Feedback, JobApplication).
/// </summary>
/// <param name="sender"></param>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý liên hệ khách hàng (CRM)")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ContactsController(ISender sender) : ApiController
{
    /// <summary>
    /// Tạo yêu cầu liên hệ chung (Storefront).
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateAsync(CreateContactCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy toàn bộ danh sách liên hệ.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetContactsQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo yêu cầu hỗ trợ (Support Request).
    /// </summary>
    [HttpPost("support-request")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateSupportRequestAsync(
        CreateSupportRequestCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo đóng góp ý kiến (Customer Feedback).
    /// </summary>
    [HttpPost("feedback")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateFeedbackAsync(
        CreateFeedbackCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo hồ sơ ứng viên tuyển dụng (Job Application).
    /// </summary>
    [HttpPost("job-application")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateJobApplicationAsync(
        CreateJobApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách liên hệ phân trang (hỗ trợ lọc theo loại và trạng thái).
    /// </summary>
    [HttpGet("paginated")]
    [Authorize]
    public async Task<IActionResult> GetPaginatedAsync(
        [FromQuery] string? contactType,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPaginatedContactsQuery(contactType, status, page, pageSize);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật trạng thái liên hệ ( hỗ trợ chuyển trạng thái hồ sơ).
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatusAsync(
        int id,
        [FromQuery] string contactType,
        UpdateContactStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateContactStatusCommand(contactType, id, request);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Phản hồi yêu cầu liên hệ.
    /// </summary>
    [HttpPost("reply")]
    [Authorize]
    public async Task<IActionResult> ReplyAsync(CreateContactReplyCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật ghi chú nội bộ cho yêu cầu liên hệ.
    /// </summary>
    [HttpPatch("internal-note")]
    [Authorize]
    public async Task<IActionResult> UpdateInternalNoteAsync(
        UpdateInternalNoteCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}
