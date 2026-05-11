using Application.Interfaces.Repositories.Technology.Technology;
using Domain.Entities;
using Infrastructure.DBContexts;
using TechnologyEntity = Domain.Entities.Technology;

namespace Infrastructure.Repositories.Technology.Technology;

public class TechnologyUpdateRepository(ApplicationDBContext context) : ITechnologyUpdateRepository
{
    public void Update(TechnologyEntity technology)
    {
        context.Technologies.Update(technology);
    }

    public void Restore(TechnologyEntity technology)
    {
        technology.DeletedAt = null;
        context.Technologies.Update(technology);
    }
}
