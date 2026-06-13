using Application.ApiContracts.FinanceContract.Requests;
using MediatR;

namespace Application.Features.FinanceContracts.Commands.UploadDisbursementEvidence;

public sealed record UploadDisbursementEvidenceCommand(
    Guid FinanceContractId,
    UploadDisbursementEvidenceRequest Request,
    Guid CurrentUserId) : IRequest;

