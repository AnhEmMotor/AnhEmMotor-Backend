using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile.File;
using MediatR;

namespace Application.Features.FinanceContracts.Commands.UploadDisbursementEvidence;

public class UploadDisbursementEvidenceCommandHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<UploadDisbursementEvidenceCommand>
{
    public async Task Handle(UploadDisbursementEvidenceCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
