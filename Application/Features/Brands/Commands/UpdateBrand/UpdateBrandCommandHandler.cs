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
        if (brand == null)
        {
            return Error.NotFound($"Brand with Id {request.Id} not found.", "Id");
        }
        string? cleanName = request.Name?.Trim();
        if (!string.Equals(brand.Name?.Trim(), cleanName, StringComparison.OrdinalIgnoreCase))
        {
            var duplicateCandidates = await readRepository.GetByNameAsync(cleanName!, cancellationToken)
                .ConfigureAwait(false);
            if (duplicateCandidates.Any(x => x.Id != request.Id))
            {
                return Error.Validation("Brand name already exists.", "Name");
            }
        }
        request.Adapt(brand);
        brand.Name = request.Name?.Trim();
        brand.Description = request.Description?.Trim();
        brand.Origin = request.Origin?.Trim();
        brand.LogoUrl = request.LogoUrl?.Trim();
        updateRepository.Update(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return brand.Adapt<BrandResponse>();
    }
}
