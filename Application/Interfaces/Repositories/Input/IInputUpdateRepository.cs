using InputEntity = Domain.Entities.Input;

namespace Application.Interfaces.Repositories.Input;

public interface IInputUpdateRepository
{
    public void Update(InputEntity input);

    public void Restore(InputEntity input);

    public void Restore(IEnumerable<InputEntity> inputs);
}
