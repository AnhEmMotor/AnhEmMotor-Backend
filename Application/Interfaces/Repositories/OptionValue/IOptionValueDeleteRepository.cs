using System;
using System.Collections.Generic;
using System.Text;
using VariantOptionValueEntity = Domain.Entities.VariantOptionValue;

namespace Application.Interfaces.Repositories.OptionValue
{
    public interface IOptionValueDeleteRepository
    {
        void Delete(VariantOptionValueEntity variantOptionValue);
    }
}
