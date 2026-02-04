using ProductEntity = Domain.Entities.Product;
using System.Collections.Generic;

namespace Application.Interfaces.Repositories.Product;

public interface IProductInsertRepository
{
    public void Add(ProductEntity product);
}
