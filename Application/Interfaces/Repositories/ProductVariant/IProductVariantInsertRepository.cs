using ProductVariantEntity = Domain.Entities.ProductVariant;
using System.Collections.Generic;

namespace Application.Interfaces.Repositories.ProductVariant
{
    public interface IProductVariantInsertRepository
    {
        public void Add(ProductVariantEntity variant);
        public void AddOption(Domain.Entities.Option option);
        public void AddOptionRange(IEnumerable<Domain.Entities.Option> options);
    }
}
