using TechnologyEntity = Domain.Entities.Technology;

namespace Application.Interfaces.Repositories.Technology.Technology;

public interface ITechnologyInsertRepository
{
    public void Add(TechnologyEntity TechnologyEntity);
}
