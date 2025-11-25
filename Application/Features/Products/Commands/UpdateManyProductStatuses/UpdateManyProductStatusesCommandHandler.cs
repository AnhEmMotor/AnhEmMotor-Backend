using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.VariantOptionValue;
using Domain.Enums;
using Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.UpdateManyProductStatuses;

public sealed class UpdateManyProductStatusesCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateManyProductStatusesCommand, (List<int>? Data, ErrorResponse? Error)>
{
    public async Task<(List<int>? Data, ErrorResponse? Error)> Handle(UpdateManyProductStatusesCommand command, CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetail>();
        var ids = command.Ids.Distinct().ToList();

        // First query to check which IDs exist
        var existingIds = await readRepository.GetQueryable()
            .Where(p => ids.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        foreach (var id in ids)
        {
            if (!existingIds.Contains(id))
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

        // Fetch entities WITHOUT navigation properties and with AsNoTracking to avoid tracking conflicts
        var productEntities = await readRepository.GetByIdAsync(ids, cancellationToken, DataFetchMode.ActiveOnly);

        foreach (var product in productEntities)
        {
            product.StatusId = command.StatusId;
            updateRepository.Update(product);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (ids, null);
    }
}
