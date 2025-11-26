using InputEntity = Domain.Entities.Input;

namespace Application.Interfaces.Repositories.Input;

public interface IInputUpdateRepository
{
    void Update(InputEntity input);

    void Restore(InputEntity input);

    void Restore(IEnumerable<InputEntity> inputs);
}
