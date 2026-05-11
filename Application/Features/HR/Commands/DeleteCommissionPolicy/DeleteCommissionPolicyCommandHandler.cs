using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.CommissionPolicy;
using MediatR;

namespace Application.Features.HR.Commands.DeleteCommissionPolicy;

public class DeleteCommissionPolicyCommandHandler(
    ICommissionPolicyReadRepository readRepository,
    ICommissionPolicyDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteCommissionPolicyCommand, Result>
{
    public async Task<Result> Handle(DeleteCommissionPolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(true);
        if (policy == null)
            return Result.Failure(Error.NotFound($"Commission policy with ID {request.Id} not found."));
        deleteRepository.Remove(policy);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(true);
        return Result.Success();
    }
}
