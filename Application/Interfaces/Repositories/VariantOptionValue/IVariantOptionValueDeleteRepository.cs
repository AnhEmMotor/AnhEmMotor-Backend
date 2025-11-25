using System;
using System.Collections.Generic;
using VariantOptionValueEntity = Domain.Entities.VariantOptionValue;
using System.Text;

namespace Application.Interfaces.Repositories.VariantOptionValue
{
    public interface IVariantOptionValueDeleteRepository
    {
        void Delete(VariantOptionValueEntity variantOptionValue);
    }
}
