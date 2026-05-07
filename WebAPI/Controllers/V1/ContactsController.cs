using Application.Features.Contacts.Commands.CreateContact;
using Application.Features.Contacts.Commands.CreateContactReply;
using Application.Features.Contacts.Commands.UpdateInternalNote;
using Application.Features.Contacts.Queries.GetContacts;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller quản lý liên hệ khách hàng.
/// </summary>
/// <param name="sender"></param>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý liên hệ khách hàng (CRM)")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ContactsController(ISender sender) : ApiController
{
    /// <summary>
    /// Tạo yêu cầu liên hệ mới.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateAsync(CreateContactCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách yêu cầu liên hệ.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetContactsQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Phản hồi yêu cầu liên hệ.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="command"></param>
    /// <returns></returns>
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
    /// <param name="cancellationToken"></param>
    /// <param name="command"></param>
    /// <returns></returns>
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
