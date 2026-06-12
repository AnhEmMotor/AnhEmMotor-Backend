using Application.ApiContracts.Evaluation.Requests;
using Application.ApiContracts.Evaluation.Responses;
using Application.Common.Models;
using Application.Features.ServiceWorkshopEvaluations.Commands.CreateServiceEvaluationReply;
using Application.Features.ServiceWorkshopEvaluations.Commands.MarkServiceEvaluationProcessed;
using Application.Features.ServiceWorkshopEvaluations.Commands.UpdateServiceEvaluationInternalNotes;
using Application.Features.ServiceWorkshopEvaluations.Queries.GetServiceWorkshopEvaluations;
using Application.Features.ServiceWorkshopEvaluations.Queries.GetServiceWorkshopEvaluationDetail;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller for managing service workshop evaluations.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ServiceWorkshopEvaluationsController : ControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceWorkshopEvaluationsController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator service.</param>
    public ServiceWorkshopEvaluationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private readonly IMediator _mediator;

    /// <summary>
    /// Retrieves a paged list of service workshop evaluations.
    /// </summary>
    /// <param name="status">The status to filter by.</param>
    /// <param name="criteria">The criteria to filter by.</param>
    /// <param name="search">The search term.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged list of evaluations.</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ServiceEvaluationListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<ServiceEvaluationListResponse>>> GetPagedAsync(
        [FromQuery] string? status,
        [FromQuery] string? criteria,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetServiceWorkshopEvaluationsQuery
        {
            Status = status,
            Criteria = criteria,
            Search = search,
            Page = page,
            PageSize = pageSize,
        };

        var result = await _mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the details of a specific service workshop evaluation.
    /// </summary>
    /// <param name="evaluationId">The identifier of the evaluation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The evaluation details.</returns>
    [HttpGet("{evaluationId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ServiceEvaluationDetailResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<ServiceEvaluationDetailResponse>>> GetDetailAsync(
        int evaluationId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetServiceWorkshopEvaluationDetailQuery { EvaluationId = evaluationId };
        var result = await _mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Creates a reply to a service workshop evaluation.
    /// </summary>
    /// <param name="request">The reply request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the created reply.</returns>
    [HttpPost("reply")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<int>>> CreateReplyAsync(
        [FromBody] CreateServiceEvaluationReplyRequest request,
        CancellationToken cancellationToken = default)
    {
        var cmd = new CreateServiceEvaluationReplyCommand
        {
            EvaluationId = request.EvaluationId,
            Message = request.Message,
            MarkAsProcessed = request.MarkAsProcessed,
        };
        var result = await _mediator.Send(cmd, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Marks a service workshop evaluation as processed.
    /// </summary>
    /// <param name="request">The process request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A boolean indicating success.</returns>
    [HttpPost("processed")]
    [Authorize]
    public async Task<ActionResult<Result<bool>>> MarkProcessedAsync(
        [FromBody] MarkServiceEvaluationProcessedRequest request,
        CancellationToken cancellationToken = default)
    {
        var cmd = new MarkServiceEvaluationProcessedCommand { EvaluationId = request.EvaluationId };
        var result = await _mediator.Send(cmd, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Updates the internal notes of a service workshop evaluation.
    /// </summary>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A boolean indicating success.</returns>
    [HttpPut("internal-notes")]
    [Authorize]
    public async Task<ActionResult<Result<bool>>> UpdateInternalNotesAsync(
        [FromBody] UpdateServiceEvaluationInternalNotesRequest request,
        CancellationToken cancellationToken = default)
    {
        var cmd = new UpdateServiceEvaluationInternalNotesCommand
        {
            EvaluationId = request.EvaluationId,
            InternalNotes = request.InternalNotes,
        };

        var result = await _mediator.Send(cmd, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}

