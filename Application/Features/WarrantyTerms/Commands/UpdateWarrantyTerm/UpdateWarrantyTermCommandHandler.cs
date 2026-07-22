using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WarrantyTerm;
using MediatR;
using Mapster;

namespace Application.Features.WarrantyTerms.Commands.UpdateWarrantyTerm;

public class UpdateWarrantyTermCommandHandler(
    IWarrantyTermReadRepository readRepository,
    IWarrantyTermUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateWarrantyTermCommand, Result<int>>
{
    public async Task<Result<int>> Handle(UpdateWarrantyTermCommand request, CancellationToken cancellationToken)
    {
        var warrantyTerm = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (warrantyTerm == null)
            return Error.NotFound("Warranty term not found.");

        request.Adapt(warrantyTerm);
        updateRepository.Update(warrantyTerm);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(warrantyTerm.Id);
    }
}
