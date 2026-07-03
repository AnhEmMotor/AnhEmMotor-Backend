using Application.ApiContracts.FinanceContract.Requests;
using Application.Features.FinanceContracts.Commands.UpdateCavetState;
using Application.Features.FinanceContracts.Commands.UpdateDisbursementPayment;
using Application.Features.FinanceContracts.Commands.UploadDisbursementEvidence;
using Application.Features.FinanceContracts.Queries.GetFinanceContractDetail;
using Application.Features.FinanceContracts.Queries.GetFinanceContractsList;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;
using WebAPI.Models;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Manages finance contract operations including retrieval, disbursement updates, and evidence uploads.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Finance Contracts")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class FinanceContractsController(ISender sender) : ApiController
{
    /// <summary>
    /// Retrieves a paginated and filterable list of finance contracts.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get Finance Contracts List")]
    public async Task<IActionResult> GetFinanceContractsList(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetFinanceContractsListQuery { SieveModel = sieveModel };
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Retrieves the details of a specific finance contract by its identifier.
    /// </summary>
    [HttpGet("{financeContractId:guid}")]
    [SwaggerOperation(Summary = "Get Finance Contract Detail")]
    public async Task<IActionResult> GetFinanceContractDetail(
        [FromRoute] Guid financeContractId,
        CancellationToken cancellationToken)
    {
        var query = new GetFinanceContractDetailQuery(
            new GetFinanceContractDetailRequest(financeContractId),
            Guid.Empty);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Updates the disbursement payment information for a finance contract.
    /// </summary>
    [HttpPost("{financeContractId:guid}/disbursement/payment")]
    public async Task<IActionResult> UpdateDisbursementPayment(
        [FromRoute] Guid financeContractId,
        [FromBody] UpdateDisbursementPaymentRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new UpdateDisbursementPaymentCommand(financeContractId, request, Guid.Empty),
            cancellationToken)
            .ConfigureAwait(false);
        return Ok(new { success = true });
    }

    /// <summary>
    /// Updates the state of a CAVET document associated with a finance contract.
    /// </summary>
    [HttpPost("{financeContractId:guid}/cavet/state")]
    public async Task<IActionResult> UpdateCavetState(
        [FromRoute] Guid financeContractId,
        [FromBody] UpdateCavetStateRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateCavetStateCommand(financeContractId, request, Guid.Empty), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new { success = true });
    }

    /// <summary>
    /// Uploads evidence documentation for a finance contract's disbursement.
    /// </summary>
    [HttpPost("{financeContractId:guid}/disbursement/evidence/upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDisbursementEvidence(
        [FromRoute] Guid financeContractId,
        [FromForm] UploadDisbursementEvidenceForm form,
        CancellationToken cancellationToken)
    {
        if (form?.File == null)
            return BadRequest(new { success = false, message = "File is required" });
        using var stream = form.File.OpenReadStream();
        await sender.Send(
            new UploadDisbursementEvidenceCommand(
                financeContractId,
                new UploadDisbursementEvidenceRequest { FileContent = stream, FileName = form.File.FileName },
                Guid.Empty),
            cancellationToken)
            .ConfigureAwait(false);
        return Ok(new { success = true });
    }
}

