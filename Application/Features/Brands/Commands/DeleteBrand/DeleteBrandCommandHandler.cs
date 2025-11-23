using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Commands.DeleteBrand;

public sealed class DeleteBrandCommandHandler(IBrandReadRepository readRepository, IBrandDeleteRepository deleteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBrandCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (brand == null)
        {
            return new ErrorResponse
            {
                Errors = [new ErrorDetail { Field = "Id", Message = $"Brand with Id {request.Id} not found." }]
            };
        }

        deleteRepository.Delete(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
