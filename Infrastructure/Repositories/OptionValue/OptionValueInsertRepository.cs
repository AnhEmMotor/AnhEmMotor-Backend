using Application.Interfaces.Repositories.OptionValue;
using Infrastructure.DBContexts;
using System;
using OptionValueEntity = Domain.Entities.OptionValue;

namespace Infrastructure.Repositories.OptionValue
{
    public class OptionValueInsertRepository(ApplicationDBContext context) : IOptionValueInsertRepository
    {
        public void Add(OptionValueEntity optionValue) { context.OptionValues.Add(optionValue); }
    }
}
