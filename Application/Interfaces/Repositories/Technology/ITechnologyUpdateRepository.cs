using TechnologyEntity = Domain.Entities.Technology;

namespace Application.Interfaces.Repositories.Technology;

public interface ITechnologyUpdateRepository
{
    public void Add(TechnologyEntity technology);

    public void Update(TechnologyEntity technology);

    public void Remove(TechnologyEntity technology);
}
