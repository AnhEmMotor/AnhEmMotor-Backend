using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.FinanceContracts.Commands.UpdateCavetState;

public class UpdateCavetStateCommandHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateCavetStateCommand>
{
    public async Task Handle(UpdateCavetStateCommand request, CancellationToken cancellationToken)
    {
        // TODO: Implement strict business rules + soft delete + audit log + Cavet transition.
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

