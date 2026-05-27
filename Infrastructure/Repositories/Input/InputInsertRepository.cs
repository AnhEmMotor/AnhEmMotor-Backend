using Application.Interfaces.Repositories.Input;
using Infrastructure.DBContexts;
using InputEntity = Domain.Entities.Input;

namespace Infrastructure.Repositories.Input;

public class InputInsertRepository(ApplicationDBContext context) : IInputInsertRepository
{
    public void Add(InputEntity input)
    {
        context.InputReceipts.Add(input);
    }
}
