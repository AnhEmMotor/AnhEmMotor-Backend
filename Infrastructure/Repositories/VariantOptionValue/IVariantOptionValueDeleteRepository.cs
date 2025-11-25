using VariantOptionValueEntity = Domain.Entities.VariantOptionValue;
using Application.Interfaces.Repositories.VariantOptionValue;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.VariantOptionValue
{
    public class VariantOptionValueDeleteRepository(ApplicationDBContext context) : IVariantOptionValueDeleteRepository
    {
        public void Delete(VariantOptionValueEntity variantOptionValue)
        {
            context.VariantOptionValues.Remove(variantOptionValue);
        }
    }
}
