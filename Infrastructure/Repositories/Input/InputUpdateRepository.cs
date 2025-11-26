using Application.Interfaces.Repositories.Input;
using Infrastructure.DBContexts;
using InputEntity = Domain.Entities.Input;

namespace Infrastructure.Repositories.Input;

public class InputUpdateRepository(ApplicationDBContext context) : IInputUpdateRepository
{
    public void Update(InputEntity input)
    {
        context.InputReceipts.Update(input);
    }

    public void Restore(InputEntity input)
    {
        context.Restore(input);
    }

    public void Restore(IEnumerable<InputEntity> inputs)
    {
        context.RestoreDeleteUsingSetColumnRange(inputs);
    }
}
