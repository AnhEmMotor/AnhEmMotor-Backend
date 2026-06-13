using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.FinanceContracts.Commands.UpdateDisbursementPayment;

public class UpdateDisbursementPaymentCommandHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateDisbursementPaymentCommand>
{
    public async Task Handle(UpdateDisbursementPaymentCommand request, CancellationToken cancellationToken)
    {
        // TODO: Implement 100% amount validation, status transition PendingDisbursement -> Disbursed, audit log, soft delete, trigger Cavet reminder.
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

