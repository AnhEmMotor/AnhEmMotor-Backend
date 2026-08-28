using Application.ApiContracts.Client.Support;
using Application.Features.Client.Support;
using Application.Features.Contacts.Queries.GetMyFeedbacks;
using Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.V1.Client;

/// <summary>
/// Hỗ trợ khách hàng — FAQ, yêu cầu gọi lại, phản hồi (Client Portal).
/// </summary>
[ApiController]
[Route("api/v1/client/support")]
public class SupportController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public SupportController(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Lấy danh sách câu hỏi thường gặp (FAQ), có thể tìm kiếm.
    /// </summary>
    /// <param name="search">Từ khóa tìm kiếm (tùy chọn).</param>
    [HttpGet("faq")]
    public async Task<IActionResult> GetFaqs([FromQuery] string search)
    {
        var result = await _mediator.Send(new GetFaqsQuery(search));
        return Ok(result);
    }

    /// <summary>
    /// Gửi yêu cầu cửa hàng gọi lại cho khách hàng.
    /// </summary>
    /// <param name="request">Thông tin yêu cầu gọi lại (họ tên, SĐT, lý do).</param>
    [HttpPost("callback")]
    public async Task<IActionResult> RequestCallback([FromBody] CallbackRequest request)
    {
        var result = await _mediator.Send(new RequestCallbackCommand(request));
        return result ? Ok() : BadRequest();
    }

    /// <summary>
    /// Gửi phản hồi/đánh giá của khách hàng (yêu cầu đăng nhập).
    /// </summary>
    /// <param name="request">Nội dung phản hồi và đánh giá sao.</param>
    [HttpPost("feedback")]
    [Authorize]
    public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackRequest request)
    {
        var result = await _mediator.Send(new SubmitFeedbackCommand(request));
        return result ? Ok() : BadRequest();
    }

    /// <summary>
    /// Lấy danh sách ý kiến đóng góp của khách hàng đang đăng nhập.
    /// </summary>
    [HttpGet("my-feedbacks")]
    [Authorize]
    public async Task<IActionResult> GetMyFeedbacks()
    {
        var result = await _mediator.Send(new GetMyFeedbacksQuery(_currentUserContext.GetUserId()));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
