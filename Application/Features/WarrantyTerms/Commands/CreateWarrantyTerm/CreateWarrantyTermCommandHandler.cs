using Application.ApiContracts.Admin.Warranty;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WarrantyTerm;
using Domain.Entities;
using MediatR;

namespace Application.Features.WarrantyTerms.Commands.CreateWarrantyTerm;

public class CreateWarrantyTermCommandHandler(
    IWarrantyTermWriteRepository writeRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateWarrantyTermCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateWarrantyTermCommand request, CancellationToken cancellationToken)
    {
        var term = new WarrantyTerm
        {
            TermName = request.TermName.Trim(),
            TermNameJson = request.TermNameJson?.Trim(),
            BrandId = request.BrandId,
            VehicleType = request.VehicleType.Trim(),
            ErrorCategory = request.ErrorCategory.Trim(),
            Description = request.Description?.Trim(),
            DescriptionJson = request.DescriptionJson?.Trim(),
            DurationMonths = request.DurationMonths,
            DurationKm = request.DurationKm,
            Coverage = request.Coverage?.Trim(),
            Status = request.Status,
            EffectiveDate = request.EffectiveDate,
            ExpirationDate = request.ExpirationDate,
            MediaUrl = request.MediaUrl?.Trim()
        };

        await writeRepository.AddAsync(term, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<int>.Success(term.Id);
    }
}
