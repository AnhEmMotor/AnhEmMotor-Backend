using InputEntity = Domain.Entities.Input;

namespace Application.Interfaces.Repositories.Input;

public interface IInputInsertRepository
{
    void Add(InputEntity input);
}
