using System;
using VariantOptionValueEntity = Domain.Entities.VariantOptionValue;

namespace Application.Interfaces.Repositories.OptionValue
{
    public interface IOptionValueDeleteRepository
    {
        public void Delete(VariantOptionValueEntity variantOptionValue);
    }
}
