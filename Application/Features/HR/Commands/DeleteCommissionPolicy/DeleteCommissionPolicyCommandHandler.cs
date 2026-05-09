using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR;
using MediatR;

namespace Application.Features.HR.Commands.DeleteCommissionPolicy;

public class DeleteCommissionPolicyCommandHandler(ICommissionPolicyRepository repository, IUnitOfWork unitOfWork) : IRequestHandler<DeleteCommissionPolicyCommand, Result>
{
    public async Task<Result> Handle(DeleteCommissionPolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(true);
        if (policy == null)
            return Result.Failure(Error.NotFound($"Commission policy with ID {request.Id} not found."));
        repository.Remove(policy);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(true);
        return Result.Success();
    }
}
