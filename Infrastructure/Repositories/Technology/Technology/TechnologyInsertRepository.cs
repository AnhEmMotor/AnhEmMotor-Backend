using Application.Interfaces.Repositories.Technology.Technology;
using Domain.Entities;
using Infrastructure.DBContexts;
using TechnologyEntity = Domain.Entities.Technology;

namespace Infrastructure.Repositories.Technology.Technology;

public class TechnologyInsertRepository(ApplicationDBContext context) : ITechnologyInsertRepository
{
    public void Add(TechnologyEntity technology)
    {
        context.Technologies.Add(technology);
    }
}
