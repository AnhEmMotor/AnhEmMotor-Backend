using Application.Interfaces.Repositories.Technology;
using Infrastructure.DBContexts;
using TechnologyEntity = Domain.Entities.Technology;

namespace Infrastructure.Repositories.Technology;

public class TechnologyUpdateRepository(ApplicationDBContext context) : ITechnologyUpdateRepository
{
    public void Add(TechnologyEntity technology)
    {
        context.Technologies.Add(technology);
    }

    public void Update(TechnologyEntity technology)
    {
        context.Technologies.Update(technology);
    }

    public void Remove(TechnologyEntity technology)
    {
        context.Technologies.Remove(technology);
    }
}
