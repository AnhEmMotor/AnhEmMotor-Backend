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
/// Quản lý liên hệ khách hàng (CRM)
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý liên hệ khách hàng (CRM)")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ContactsController(ISender sender) : ApiController
{
    /// <summary>
    /// Gửi thông tin liên hệ mới
    /// </summary>
    /// <param name="command">Dữ liệu liên hệ</param>
    /// <returns>Kết quả gửi</returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(CreateContactCommand command)
    {
        var result = await sender.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách tất cả các liên hệ
    /// </summary>
    /// <returns>Danh sách liên hệ</returns>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var result = await sender.Send(new GetContactsQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Phản hồi liên hệ khách hàng
    /// </summary>
    /// <param name="command">Nội dung phản hồi</param>
    /// <returns>Kết quả phản hồi</returns>
    [HttpPost("reply")]
    [Authorize]
    public async Task<IActionResult> Reply(CreateContactReplyCommand command)
    {
        var result = await sender.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật ghi chú nội bộ cho liên hệ
    /// </summary>
    /// <param name="command">Nội dung ghi chú</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPatch("internal-note")]
    [Authorize]
    public async Task<IActionResult> UpdateInternalNote(UpdateInternalNoteCommand command)
    {
        var result = await sender.Send(command);
        return HandleResult(result);
    }
}
