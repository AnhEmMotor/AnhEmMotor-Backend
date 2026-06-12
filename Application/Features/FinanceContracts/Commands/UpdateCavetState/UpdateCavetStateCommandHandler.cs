using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.FinanceContracts.Commands.UpdateCavetState;

public sealed class UpdateCavetStateCommandHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateCavetStateCommand>
{
    public async Task Handle(UpdateCavetStateCommand request, CancellationToken cancellationToken)
    {
        // TODO: Implement strict business rules + soft delete + audit log + Cavet transition.
        // Placeholder handler to make API compile.
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

