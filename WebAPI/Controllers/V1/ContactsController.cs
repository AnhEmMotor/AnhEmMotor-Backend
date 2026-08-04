using Application.ApiContracts.Contacts.Requests;
using Application.ApiContracts.Contacts.Responses;
using Application.Common.Models;
using Application.Features.Contacts.Commands.AssignSupportRequest;
using Application.Features.Contacts.Commands.CreateContact;
using Application.Features.Contacts.Commands.CreateContactReply;
using Application.Features.Contacts.Commands.CreateFeedback;
using Application.Features.Contacts.Commands.CreateJobApplication;
using Application.Features.Contacts.Commands.CreateSupportRequest;
using Application.Features.Contacts.Commands.RateSupportCustomer;
using Application.Features.Contacts.Commands.RateSupportEmployee;
using Application.Features.Contacts.Commands.UpdateContactStatus;
using Application.Features.Contacts.Commands.UpdateInternalNote;
using Application.Features.Contacts.Commands.UploadCv;
using Application.Features.Contacts.Queries.GetContacts;
using Application.Features.Contacts.Queries.GetPaginatedContacts;
using Application.Features.Contacts.Queries.GetSupportRequestTracking;
using Asp.Versioning;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller quản lý liên hệ khách hàng — bao gồm yêu cầu hỗ trợ (Support), phản hồi (Feedback), ứng tuyển (Job
/// Application). Cho phép khách hàng không đăng nhập tạo liên hệ, và nhân viên xử lý nội bộ khi đã đăng nhập.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý liên hệ khách hàng (CRM)")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class ContactsController(ISender sender) : ApiController
{
    /// <summary>
    /// Tạo yêu cầu liên hệ chung từ cửa hàng (Storefront) — khách hàng không cần đăng nhập.
    /// </summary>
    /// <param name="command">Thông tin yêu cầu liên hệ (tên, email, nội dung).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả tạo yêu cầu liên hệ.</returns>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(CreateContactCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo yêu cầu hỗ trợ (Support Request) — khách hàng không cần đăng nhập.
    /// </summary>
    /// <param name="command">Thông tin yêu cầu hỗ trợ (tiêu đề, mô tả, thông tin liên hệ).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả tạo yêu cầu hỗ trợ.</returns>
    [HttpPost("support-request")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<CreateSupportRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSupportRequestAsync(
        CreateSupportRequestCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Theo dõi tiến độ một yêu cầu hỗ trợ bằng mã bí mật được cấp lúc tạo yêu cầu.
    /// </summary>
    [HttpGet("support-request/{id:int}/tracking")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SupportRequestTrackingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupportRequestTrackingAsync(
        int id,
        [FromQuery] Guid token,
        CancellationToken cancellationToken)
    {
        var result = await sender
            .Send(new GetSupportRequestTrackingQuery(id, token), cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Khách hàng đánh giá nhân viên được phân công sau khi yêu cầu hoàn tất.
    /// </summary>
    [HttpPost("support-request/{id:int}/customer-rating")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RateSupportEmployeeAsync(
        int id,
        CustomerSupportRatingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender
            .Send(new RateSupportEmployeeCommand(id, request), cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy toàn bộ danh sách liên hệ (Support, Feedback, Job Application). Yêu cầu đăng nhập.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách tất cả các liên hệ trong hệ thống.</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetContactsQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo đóng góp ý kiến — khách hàng gửi phản hồi về sản phẩm/dịch vụ. Không cần đăng nhập.
    /// </summary>
    /// <param name="command">Thông tin phản hồi (tên, nội dung, đánh giá).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả tạo phản hồi.</returns>
    [HttpPost("feedback")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFeedbackAsync(
        CreateFeedbackCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo hồ sơ ứng viên tuyển dụng — khách hàng gửi CV ứng tuyển. Không cần đăng nhập.
    /// </summary>
    /// <param name="command">Thông tin ứng viên (tên, email, vị trí ứng tuyển).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả tạo hồ sơ ứng tuyển.</returns>
    [HttpPost("job-application")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<JobApplicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateJobApplicationAsync(
        CreateJobApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tải lên tệp CV ứng viên (PDF, DOCX, hình ảnh). Không cần đăng nhập.
    /// </summary>
    /// <param name="file">Tệp CV cần tải lên.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>URL hoặc ID của CV đã tải lên.</returns>
    [HttpPost("upload-cv")]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadCvAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty.");
        }
        var command = new UploadCvCommand { FileContent = file.OpenReadStream(), FileName = file.FileName };
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách liên hệ phân trang — hỗ trợ lọc theo loại (Support/Feedback/JobApplication) và trạng thái. Yêu cầu
    /// đăng nhập.
    /// </summary>
    /// <param name="contactType">Loại liên hệ: "support", "feedback", "job-application".</param>
    /// <param name="status">Trạng thái lọc: "new", "in-progress", "resolved"...</param>
    /// <param name="assignedUserId">ID nhân viên được phân công (tùy chọn).</param>
    /// <param name="page">Số trang (mặc định 1).</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định 20).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách liên hệ phân trang.</returns>
    [HttpGet("paginated")]
    [Authorize]
    public async Task<IActionResult> GetPaginatedAsync(
        [FromQuery] string? contactType,
        [FromQuery] string? status,
        [FromQuery] Guid? assignedUserId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPaginatedContactsQuery(contactType, status, assignedUserId, page, pageSize);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật trạng thái một liên hệ (chuyển trạng thái — ví dụ: New → In-Progress → Resolved). Yêu cầu đăng nhập.
    /// </summary>
    /// <param name="id">ID của liên hệ cần cập nhật.</param>
    /// <param name="contactType">Loại liên hệ: "support", "feedback", "job-application".</param>
    /// <param name="request">Thông tin trạng thái mới.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả cập nhật trạng thái.</returns>
    [HttpPatch("{id:int}/status")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
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
    /// Phân công yêu cầu hỗ trợ cho một nhân viên xử lý. Yêu cầu đăng nhập.
    /// </summary>
    /// <param name="id">ID của yêu cầu hỗ trợ cần phân công.</param>
    /// <param name="command">Thông tin phân công (ID nhân viên xử lý).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả phân công.</returns>
    [HttpPatch("{id:int}/assign")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignAsync(
        int id,
        AssignSupportRequestCommand command,
        CancellationToken cancellationToken)
    {
        var cmd = command with { SupportRequestId = id };
        var result = await sender.Send(cmd, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Nhân viên được phân công đánh giá mức độ hợp tác của khách hàng sau khi hỗ trợ hoàn tất.
    /// </summary>
    [HttpPost("support-request/{id:int}/employee-rating")]
    [HasPermission(Permissions.Marketing.ContactManagement.Edit)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RateSupportCustomerAsync(
        int id,
        SupportRatingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender
            .Send(new RateSupportCustomerCommand(id, request), cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Phản hồi (trả lời) một yêu cầu liên hệ. Yêu cầu đăng nhập.
    /// </summary>
    /// <param name="command">Nội dung phản hồi và thông tin liên kết.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả tạo phản hồi.</returns>
    [HttpPost("reply")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReplyAsync(CreateContactReplyCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật ghi chú nội bộ cho một yêu cầu liên hệ (chỉ bên trong công ty). Yêu cầu đăng nhập.
    /// </summary>
    /// <param name="command">Nội dung ghi chú nội bộ và thông tin liên kết.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả cập nhật ghi chú.</returns>
    [HttpPatch("internal-note")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateInternalNoteAsync(
        UpdateInternalNoteCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
