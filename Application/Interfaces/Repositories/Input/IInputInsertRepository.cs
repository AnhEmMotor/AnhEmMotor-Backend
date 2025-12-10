using InputEntity = Domain.Entities.Input;

namespace Application.Interfaces.Repositories.Input;

public interface IInputInsertRepository
{
    public void Add(InputEntity input);
}
