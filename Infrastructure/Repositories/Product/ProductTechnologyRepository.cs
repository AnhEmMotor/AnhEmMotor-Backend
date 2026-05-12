using Application.Interfaces.Repositories.Technology;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Product;

public class ProductTechnologyRepository(ApplicationDBContext context) : IProductTechnologyRepository
{
    public void Remove(ProductTechnology tech)
    {
        context.ProductTechnologies.Remove(tech);
    }

    public void RemoveRange(IEnumerable<ProductTechnology> techs)
    {
        context.ProductTechnologies.RemoveRange(techs);
    }
}
