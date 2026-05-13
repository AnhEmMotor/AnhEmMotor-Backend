using TechnologyEntity = Domain.Entities.Technology;

namespace Application.Interfaces.Repositories.Technology.Technology;

public interface ITechnologyDeleteRepository
{
    public void Remove(TechnologyEntity TechnologyEntity);
}
