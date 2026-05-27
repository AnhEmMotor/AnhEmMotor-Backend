using Domain.Entities;

namespace Application.Interfaces.Repositories.Technology;

public interface IProductTechnologyRepository
{
    public void Remove(ProductTechnology tech);

    public void RemoveRange(IEnumerable<ProductTechnology> techs);
}
