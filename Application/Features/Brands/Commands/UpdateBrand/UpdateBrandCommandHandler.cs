using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Mapster;
using MediatR;

namespace Application.Features.Brands.Commands.UpdateBrand;

public sealed class UpdateBrandCommandHandler(
    IBrandReadRepository readRepository,
    IBrandUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateBrandCommand, Result<BrandResponse?>>
{
    public async Task<Result<BrandResponse?>> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(brand == null)
        {
            return Error.NotFound($"Brand with Id {request.Id} not found.", "Id");
        }

        string? cleanName = request.Name?.Trim();
        string? cleanDescription = request.Description?.Trim();

        var duplicateCandidates = await readRepository.GetByNameAsync(cleanName!, cancellationToken)
            .ConfigureAwait(false);

        if(duplicateCandidates.Any(x => x.Id != request.Id))
        {
            return Error.Validation("Brand name already exists.", "Name");
        }

        request.Adapt(brand);
        brand.Name = cleanName;
        brand.Description = cleanDescription;

        updateRepository.Update(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return brand.Adapt<BrandResponse>();
    }
}
