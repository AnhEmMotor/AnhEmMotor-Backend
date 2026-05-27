using TechnologyEntity = Domain.Entities.Technology;

namespace Application.Interfaces.Repositories.Technology.Technology;

public interface ITechnologyUpdateRepository
{
    public void Update(TechnologyEntity TechnologyEntity);

    public void Restore(TechnologyEntity TechnologyEntity);
}
