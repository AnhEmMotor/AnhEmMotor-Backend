using System;
using VariantOptionValueEntity = Domain.Entities.VariantOptionValue;

namespace Application.Interfaces.Repositories.VariantOptionValue
{
    public interface IVariantOptionValueDeleteRepository
    {
        public void Delete(VariantOptionValueEntity variantOptionValue);
    }
}
