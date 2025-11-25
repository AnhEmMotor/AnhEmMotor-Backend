using System;
using System.Collections.Generic;
using System.Text;
using OptionValueEntity = Domain.Entities.OptionValue;
using Application.Interfaces.Repositories.OptionValue;
using Application;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.OptionValue
{
    public class OptionValueInsertRepository(ApplicationDBContext context) : IOptionValueInsertRepository
    {
        public void Add(OptionValueEntity optionValue)
        {
            context.OptionValues.Add(optionValue);
        }
    }
}
