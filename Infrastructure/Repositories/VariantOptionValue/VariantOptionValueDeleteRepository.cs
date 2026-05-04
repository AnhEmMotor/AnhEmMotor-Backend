using Application.Interfaces.Repositories.VariantOptionValue;
using Infrastructure.DBContexts;
using System.Linq;

namespace Infrastructure.Repositories.VariantOptionValue
{
    public class VariantOptionValueDeleteRepository(ApplicationDBContext context) : IVariantOptionValueDeleteRepository
    {
        public void Delete(Domain.Entities.VariantOptionValue variantOptionValue)
        {
            context.VariantOptionValues.Remove(variantOptionValue);
        }
    }
}
