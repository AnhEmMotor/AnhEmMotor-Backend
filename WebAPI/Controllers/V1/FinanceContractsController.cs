using Application.ApiContracts.FinanceContract.Requests;
using Application.Common.Models;
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
    public async Task<IActionResult> GetFinanceContractDetail([FromRoute] Guid financeContractId, CancellationToken cancellationToken)
    {
        var query = new GetFinanceContractDetailQuery(new GetFinanceContractDetailRequest(financeContractId), Guid.Empty);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("{financeContractId:guid}/disbursement/payment")]
    public async Task<IActionResult> UpdateDisbursementPayment(
        [FromRoute] Guid financeContractId,
        [FromBody] Application.ApiContracts.FinanceContract.Requests.UpdateDisbursementPaymentRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new Application.Features.FinanceContracts.Commands.UpdateDisbursementPayment.UpdateDisbursementPaymentCommand(
                financeContractId,
                request,
                Guid.Empty
            ),
            cancellationToken
        ).ConfigureAwait(false);

        return Ok(new { success = true });
    }

    [HttpPost("{financeContractId:guid}/cavet/state")]
    public async Task<IActionResult> UpdateCavetState(
        [FromRoute] Guid financeContractId,
        [FromBody] Application.ApiContracts.FinanceContract.Requests.UpdateCavetStateRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new Application.Features.FinanceContracts.Commands.UpdateCavetState.UpdateCavetStateCommand(
                financeContractId,
                request,
                Guid.Empty
            ),
            cancellationToken
        ).ConfigureAwait(false);

        return Ok(new { success = true });
    }

    [HttpPost("{financeContractId:guid}/disbursement/evidence/upload")]
    public async Task<IActionResult> UploadDisbursementEvidence(
        [FromRoute] Guid financeContractId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null) return BadRequest(new { success = false, message = "File is required" });

        using var stream = file.OpenReadStream();
        await sender.Send(
            new Application.Features.FinanceContracts.Commands.UploadDisbursementEvidence.UploadDisbursementEvidenceCommand(
                financeContractId,
                new Application.ApiContracts.FinanceContract.Requests.UploadDisbursementEvidenceRequest
                {
                    FileContent = stream,
                    FileName = file.FileName
                },
                Guid.Empty
            ),
            cancellationToken
        ).ConfigureAwait(false);

        return Ok(new { success = true });
    }
}


