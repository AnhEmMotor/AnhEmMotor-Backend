using Application.Interfaces.Repositories.Input;
using Infrastructure.DBContexts;
using InputEntity = Domain.Entities.Input;
using InputInfoEntity = Domain.Entities.InputInfo;

namespace Infrastructure.Repositories.Input;

public class InputDeleteRepository(ApplicationDBContext context) : IInputDeleteRepository
{
    public void Delete(InputEntity input)
    {
        context.SoftDeleteUsingSetColumn(input);
    }

    public void Delete(IEnumerable<InputEntity> inputs)
    {
        context.SoftDeleteUsingSetColumnRange(inputs);
    }

    public void DeleteInputInfo(InputInfoEntity inputInfo)
    {
        context.InputInfos.Remove(inputInfo);
    }
}
