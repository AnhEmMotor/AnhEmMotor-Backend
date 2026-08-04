using Application.Features.Products.Notifications;
using Application.Interfaces.Repositories;
using Infrastructure.DBContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Domain.Entities.Product;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Infrastructure.Repositories;

public class UnitOfWork(ApplicationDBContext context, IPublisher publisher) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var changedProductIds = context.ChangeTracker
            .Entries<ProductEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => e.Entity.Id)
            .Union(
                context.ChangeTracker
                    .Entries<ProductVariantEntity>()
                    .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                    .Select(e => e.Entity.ProductId))
            .Distinct()
            .ToList();
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        foreach (var productId in changedProductIds)
        {
            await publisher.Publish(new ProductChangedNotification(productId), cancellationToken).ConfigureAwait(false);
        }
    }
}
