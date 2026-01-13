using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using MediatR;

namespace Application.Features.Brands.Commands.DeleteBrand;

public sealed class DeleteBrandCommandHandler(
    IBrandReadRepository readRepository,
    IBrandDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteBrandCommand, Result>
{
    public async Task<Result> Handle(
        DeleteBrandCommand request,
        CancellationToken cancellationToken)
    {
        var brand = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(brand == null)
        {
            return Result.Failure(Error.NotFound($"Brand with Id {request.Id} not found.", "Id"));
        }

        deleteRepository.Delete(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
