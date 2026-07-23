using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WarrantyTerm;
using MediatR;

namespace Application.Features.WarrantyTerms.Commands.DeleteWarrantyTerm;

public class DeleteWarrantyTermCommandHandler(
    IWarrantyTermReadRepository readRepository,
    IWarrantyTermDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteWarrantyTermCommand, Result<int>>
{
    public async Task<Result<int>> Handle(DeleteWarrantyTermCommand request, CancellationToken cancellationToken)
    {
        var warrantyTerm = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (warrantyTerm == null)
            return Error.NotFound("Warranty term not found.");
        deleteRepository.Delete(warrantyTerm);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(warrantyTerm.Id);
    }
}
