using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Entities;
using MediatR;
using System;

namespace Application.Features.Products.Commands.AttachTechnologies
{
    public class AttachTechnologiesCommandHandler(
        IProductReadRepository readRepository,
        IProductUpdateRepository updateRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<AttachTechnologiesCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(AttachTechnologiesCommand request, CancellationToken cancellationToken)
        {
            var product = await readRepository.GetByIdWithDetailsAsync(request.ProductId, cancellationToken)
                .ConfigureAwait(false);
            if (product == null)
            {
                return Result<Unit>.Failure(Error.NotFound("Sản phẩm không tồn tại."));
            }
            var techIds = request.TechIds.Distinct().ToList();
            var existingTechIds = product.ProductTechnologies.Select(pt => pt.TechnologyId).ToHashSet();
            foreach (var tId in techIds)
            {
                if (existingTechIds.Contains(tId))
                {
                    return Result<Unit>.Failure(Error.BadRequest($"Công nghệ ID {tId} đã được gán cho sản phẩm này."));
                }
                product.ProductTechnologies
                    .Add(
                        new ProductTechnology
                        {
                            ProductId = product.Id,
                            TechnologyId = tId,
                            DisplayOrder = product.ProductTechnologies.Count + 1
                        });
            }
            updateRepository.Update(product);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}