using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Entities;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Products.Commands.SetProductCompatibility;

public sealed record SetProductCompatibilityCommand : IRequest<Result<Unit>>
{
    public int ProductId { get; init; }
    
    [JsonPropertyName("compatible_vehicle_ids")]
    public List<int> CompatibleVehicleIds { get; init; } = [];
}

public sealed class SetProductCompatibilityCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SetProductCompatibilityCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(SetProductCompatibilityCommand request, CancellationToken cancellationToken)
    {
        var product = await readRepository.GetByIdWithDetailsAsync(request.ProductId, cancellationToken).ConfigureAwait(false);

        if (product == null) return Result<Unit>.Failure(Error.NotFound("Sản phẩm không tồn tại."));

        // Clear existing
        product.CompatibleWith.Clear();

        // Add new
        foreach (var vId in request.CompatibleVehicleIds.Distinct())
        {
            product.CompatibleWith.Add(new ProductCompatibility 
            { 
                BaseProductId = product.Id,
                CompatibleVehicleModelId = vId 
            });
        }

        updateRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<Unit>.Success(Unit.Value);
    }
}
