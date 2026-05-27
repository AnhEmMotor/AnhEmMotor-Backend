using Application.Interfaces.Repositories.OptionValue;
using Infrastructure.DBContexts;
using System;
using VariantOptionValueEntity = Domain.Entities.VariantOptionValue;

namespace Infrastructure.Repositories.OptionValue
{
    public class OptionValueDeleteRepository(ApplicationDBContext context) : IOptionValueDeleteRepository
    {
        public void Delete(VariantOptionValueEntity variantOptionValue)
        {
            context.VariantOptionValues.Remove(variantOptionValue);
        }

        public void Delete(Domain.Entities.OptionValue optionValue)
        {
            context.OptionValues.Remove(optionValue);
        }
    }
}
