using Application.Interfaces.Repositories.Product;
using Infrastructure.DBContexts;
using OptionValueEntity = Domain.Entities.OptionValue;
using ProductEntity = Domain.Entities.Product;

namespace Infrastructure.Repositories.Product;

public class ProductInsertRepository(ApplicationDBContext context) : IProductInsertRepository
{
    public void Add(ProductEntity product)
    {
        context.Products.Add(product);
    }

    public void AddOptionValue(OptionValueEntity optionValue)
    {
        context.OptionValues.Add(optionValue);
    }
}
