using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WarrantyTerm;
using MediatR;

namespace Application.Features.WarrantyTerms.Commands.UpdateWarrantyTerm;

public class UpdateWarrantyTermCommandHandler(
    IWarrantyTermReadRepository readRepository,
    IWarrantyTermWriteRepository writeRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateWarrantyTermCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateWarrantyTermCommand request, CancellationToken cancellationToken)
    {
        var term = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (term == null)
        {
            return Result<bool>.Failure(Error.NotFound($"Warranty term with Id {request.Id} not found.", "Id"));
        }
        term.TermName = request.TermName.Trim();
        term.TermNameJson = request.TermNameJson?.Trim();
        term.BrandId = request.BrandId;
        term.VehicleType = request.VehicleType.Trim();
        term.ErrorCategory = request.ErrorCategory.Trim();
        term.Description = request.Description?.Trim();
        term.DescriptionJson = request.DescriptionJson?.Trim();
        term.DurationMonths = request.DurationMonths;
        term.DurationKm = request.DurationKm;
        term.Coverage = request.Coverage?.Trim();
        term.Status = request.Status;
        term.EffectiveDate = request.EffectiveDate;
        term.ExpirationDate = request.ExpirationDate;
        term.MediaUrl = request.MediaUrl?.Trim();
        await writeRepository.UpdateAsync(term, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<bool>.Success(true);
    }
}
