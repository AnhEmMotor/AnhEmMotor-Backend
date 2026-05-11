using Application.Interfaces.Repositories.Technology.Technology;
using Domain.Entities;
using Infrastructure.DBContexts;
using TechnologyEntity = Domain.Entities.Technology;

namespace Infrastructure.Repositories.Technology.Technology;

public class TechnologyDeleteRepository(ApplicationDBContext context) : ITechnologyDeleteRepository
{
    public void Remove(TechnologyEntity technology)
    {
        context.Technologies.Remove(technology);
    }
}
