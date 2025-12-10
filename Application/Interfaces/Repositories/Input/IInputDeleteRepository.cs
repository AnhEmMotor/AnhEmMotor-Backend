using InputEntity = Domain.Entities.Input;
using InputInfoEntity = Domain.Entities.InputInfo;

namespace Application.Interfaces.Repositories.Input;

public interface IInputDeleteRepository
{
    public void Delete(InputEntity input);

    public void Delete(IEnumerable<InputEntity> inputs);

    public void DeleteInputInfo(InputInfoEntity inputInfo);
}
