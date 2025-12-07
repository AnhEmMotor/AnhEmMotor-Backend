using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using MediatR;

namespace Application.Features.Brands.Commands.DeleteBrand;

public sealed class DeleteBrandCommandHandler(
    IBrandReadRepository readRepository,
    IBrandDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteBrandCommand, Common.Models.ErrorResponse?>
{
    public async Task<Common.Models.ErrorResponse?> Handle(
        DeleteBrandCommand request,
        CancellationToken cancellationToken)
    {
        var brand = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(brand == null)
        {
            return new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail { Field = "Id", Message = $"Brand with Id {request.Id} not found." } ]
            };
        }

        deleteRepository.Delete(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
