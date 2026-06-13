using Application.ApiContracts.FinanceContract.Requests;
using Application.Features.FinanceContracts.Commands.UpdateCavetState;
using Application.Features.FinanceContracts.Commands.UpdateDisbursementPayment;
using Application.Features.FinanceContracts.Commands.UploadDisbursementEvidence;
using Application.Features.FinanceContracts.Queries.GetFinanceContractDetail;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[SwaggerTag("Finance Contracts")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class FinanceContractsController(ISender sender) : ApiController
{
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

    [HttpPost("{financeContractId:guid}/disbursement/evidence/upload")]
    public async Task<IActionResult> UploadDisbursementEvidence(
        [FromRoute] Guid financeContractId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null)
            return BadRequest(new { success = false, message = "File is required" });
        using var stream = file.OpenReadStream();
        await sender.Send(
            new UploadDisbursementEvidenceCommand(
                financeContractId,
                new UploadDisbursementEvidenceRequest { FileContent = stream, FileName = file.FileName },
                Guid.Empty),
            cancellationToken)
            .ConfigureAwait(false);
        return Ok(new { success = true });
    }
}

