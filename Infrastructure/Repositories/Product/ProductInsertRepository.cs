using Application.Interfaces.Repositories.Product;
using Infrastructure.DBContexts;
using ProductEntity = Domain.Entities.Product;
using System.Collections.Generic;


namespace Infrastructure.Repositories.Product;

public class ProductInsertRepository(ApplicationDBContext context) : IProductInsertRepository
{
    public void Add(ProductEntity product) { context.Products.Add(product); }
}
