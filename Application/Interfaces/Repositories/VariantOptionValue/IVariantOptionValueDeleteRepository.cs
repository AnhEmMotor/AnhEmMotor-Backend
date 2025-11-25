using System;
using VariantOptionValueEntity = Domain.Entities.VariantOptionValue;

namespace Application.Interfaces.Repositories.VariantOptionValue
{
    public interface IVariantOptionValueDeleteRepository
    {
        void Delete(VariantOptionValueEntity variantOptionValue);
    }
}
