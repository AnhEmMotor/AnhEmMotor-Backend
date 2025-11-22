using OptionValueEntity = Domain.Entities.OptionValue;
using ProductEntity = Domain.Entities.Product;

namespace Application.Interfaces.Repositories.Product;

public interface IProductInsertRepository
{
    void Add(ProductEntity product);
    void AddOptionValue(OptionValueEntity optionValue);
}
