using OptionValueEntity = Domain.Entities.OptionValue;
using VariantOptionValueEntity = Domain.Entities.VariantOptionValue;

namespace Application.Interfaces.Repositories.OptionValue
{
    public interface IOptionValueDeleteRepository
    {
        public void Delete(VariantOptionValueEntity variantOptionValue);
        public void Delete(OptionValueEntity optionValue);
    }
}
