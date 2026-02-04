using Application.Interfaces.Repositories.ProductVariant;
using ProductVariantEntity = Domain.Entities.ProductVariant;
using Infrastructure.DBContexts;
using System.Collections.Generic;

namespace Infrastructure.Repositories.ProductVariant
{
    public class ProductVariantInsertRepository(ApplicationDBContext context) : IProductVariantInsertRepository
    {
        public void Add(ProductVariantEntity variant) { context.ProductVariants.Add(variant); }
        public void AddOption(Domain.Entities.Option option) { context.Options.Add(option); }
        public void AddOptionRange(IEnumerable<Domain.Entities.Option> options) { context.Options.AddRange(options); }
    }
}
