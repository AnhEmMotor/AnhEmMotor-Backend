using Application.ApiContracts.Evaluation.Requests;
using Application.ApiContracts.Evaluation.Responses;
using Application.Common.Models;
using Application.Features.ServiceWorkshopEvaluations.Commands.CreateServiceEvaluationReply;
using Application.Features.ServiceWorkshopEvaluations.Commands.MarkServiceEvaluationProcessed;
using Application.Features.ServiceWorkshopEvaluations.Commands.UpdateServiceEvaluationInternalNotes;
using Application.Features.ServiceWorkshopEvaluations.Queries.GetServiceWorkshopEvaluationDetail;
using Application.Features.ServiceWorkshopEvaluations.Queries.GetServiceWorkshopEvaluations;
using Asp.Versioning;
using Domain.Primitives;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller for managing service workshop evaluations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ServiceWorkshopEvaluationsController" /> class.
/// </remarks>
/// <param name="mediator">The mediator service.</param>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ServiceWorkshopEvaluationsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves a paged list of service workshop evaluations.
    /// </summary>
    /// <param name="sieveModel">The Sieve query model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged list of evaluations.</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ServiceEvaluationListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<PagedResult<ServiceEvaluationListRowResponse>>>> GetPagedAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken = default)
    {
        var query = new GetServiceWorkshopEvaluationsQuery { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
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
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
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
        var result = await mediator.Send(cmd, cancellationToken).ConfigureAwait(false);
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
        var result = await mediator.Send(cmd, cancellationToken).ConfigureAwait(false);
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
        var result = await mediator.Send(cmd, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}

