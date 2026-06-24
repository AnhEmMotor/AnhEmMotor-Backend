using Application.ApiContracts.Client.Support;
using Application.Features.Client.Support;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.V1.Client
{
    [ApiController]
    [Route("api/v1/client/support")]
    public class SupportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SupportController(IMediator mediator) => _mediator = mediator;

        [HttpGet("faq")]
        public async Task<IActionResult> GetFaqs([FromQuery] string search)
        {
            var result = await _mediator.Send(new GetFaqsQuery(search));
            return Ok(result);
        }

        [HttpPost("callback")]
        public async Task<IActionResult> RequestCallback([FromBody] CallbackRequest request)
        {
            var result = await _mediator.Send(new RequestCallbackCommand(request));
            return result ? Ok() : BadRequest();
        }

        [HttpPost("feedback")]
        [Authorize]
        public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackRequest request)
        {
            var result = await _mediator.Send(new SubmitFeedbackCommand(request));
            return result ? Ok() : BadRequest();
        }
    }
}
