using Application.Interfaces.Repositories.Option;
using Infrastructure.DBContexts;
using OptionEntity = Domain.Entities.Option;

namespace Infrastructure.Repositories.Option;

public class OptionInsertRepository(ApplicationDBContext context) : IOptionInsertRepository
{
    public void Add(OptionEntity option)
    {
        context.Options.Add(option);
    }
}
