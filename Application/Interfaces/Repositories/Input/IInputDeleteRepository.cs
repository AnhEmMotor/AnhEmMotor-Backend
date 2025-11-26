using InputEntity = Domain.Entities.Input;
using InputInfoEntity = Domain.Entities.InputInfo;

namespace Application.Interfaces.Repositories.Input;

public interface IInputDeleteRepository
{
    void Delete(InputEntity input);

    void Delete(IEnumerable<InputEntity> inputs);

    void DeleteInputInfo(InputInfoEntity inputInfo);
}
