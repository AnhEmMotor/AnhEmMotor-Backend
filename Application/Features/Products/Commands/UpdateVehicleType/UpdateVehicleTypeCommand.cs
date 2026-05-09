using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Products.Commands.UpdateVehicleType;

public sealed record UpdateVehicleTypeCommand : IRequest<Result<Unit>>
{
    public int ProductId { get; init; }
    
    [JsonPropertyName("vehicle_type_id")]
    public int VehicleTypeId { get; init; }
}

public sealed class UpdateVehicleTypeCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateVehicleTypeCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateVehicleTypeCommand request, CancellationToken cancellationToken)
    {
        var product = await readRepository.GetByIdAsync(request.ProductId, cancellationToken).ConfigureAwait(false);
        if (product == null) return Result<Unit>.Failure(Error.NotFound("Sản phẩm không tồn tại."));

        product.VehicleTypeId = request.VehicleTypeId;
        updateRepository.Update(product);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<Unit>.Success(Unit.Value);
    }
}
