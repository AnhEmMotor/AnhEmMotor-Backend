using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WarrantyTerm;
using MediatR;

namespace Application.Features.WarrantyTerms.Commands.DeleteWarrantyTerm;

public class DeleteWarrantyTermCommandHandler(
    IWarrantyTermReadRepository readRepository,
    IWarrantyTermWriteRepository writeRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteWarrantyTermCommand, Result>
{
    public async Task<Result> Handle(DeleteWarrantyTermCommand request, CancellationToken cancellationToken)
    {
        var term = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (term == null)
        {
            return Result.Failure(
                Error.NotFound($"Warranty term with Id {request.Id} not found.", "Id"));
        }

        await writeRepository.DeleteAsync(request.Id, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
