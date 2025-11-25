using System;
using OptionValueEntity = Domain.Entities.OptionValue;

namespace Application.Interfaces.Repositories.OptionValue
{
    public interface IOptionValueInsertRepository
    {
        void Add(OptionValueEntity optionValue);
    }
}
