using Application.Interfaces.Repositories.OptionValue;
using Infrastructure.DBContexts;
using System;
using VariantOptionValueEntity = Domain.Entities.VariantOptionValue;

namespace Infrastructure.Repositories.OptionValue
{
    public class OptionValueDeleteRepository(ApplicationDBContext context) : IOptionValueDeleteRepository
    {
        public void Delete(VariantOptionValueEntity variantOptionValue)
        { context.VariantOptionValues.Remove(variantOptionValue); }
    }
}
