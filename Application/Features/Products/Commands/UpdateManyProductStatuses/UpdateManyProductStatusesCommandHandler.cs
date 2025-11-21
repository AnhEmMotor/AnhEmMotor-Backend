using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.UpdateManyProductStatuses;

public sealed class UpdateManyProductStatusesCommandHandler(
    IProductSelectRepository selectRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateManyProductStatusesCommand, (List<int>? Data, ErrorResponse? Error)>
{
    public async Task<(List<int>? Data, ErrorResponse? Error)> Handle(UpdateManyProductStatusesCommand command, CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetail>();
        var ids = command.Ids.Distinct().ToList();

        var products = await selectRepository.GetActiveProducts()
            .Where(p => ids.Contains(p.Id))
            .Select(p => new { p.Id, p.Name, p.StatusId, p.BrandId, p.CategoryId })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var productIds = products.Select(p => p.Id).ToHashSet();

        foreach (var id in ids)
        {
            if (!productIds.Contains(id))
            {
                errors.Add(new ErrorDetail
                {
                    Field = $"Id: {id}",
                    Message = $"Sản phẩm với Id {id} không tồn tại."
                });
            }
        }

        if (errors.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errors });
        }

        // Fetch entities without navigation properties to avoid tracking conflicts
        var productEntities = await selectRepository.GetActiveProducts()
            .Where(p => ids.Contains(p.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        foreach (var product in productEntities)
        {
            product.StatusId = command.StatusId;
            updateRepository.Update(product);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (ids, null);
    }
}
