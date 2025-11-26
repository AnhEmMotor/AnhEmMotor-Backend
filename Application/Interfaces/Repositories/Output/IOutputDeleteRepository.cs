using OutputEntity = Domain.Entities.Output;
using OutputInfoEntity = Domain.Entities.OutputInfo;

namespace Application.Interfaces.Repositories.Output;

public interface IOutputDeleteRepository
{
    void Delete(OutputEntity output);

    void Delete(IEnumerable<OutputEntity> outputs);

    void DeleteOutputInfo(OutputInfoEntity outputInfo);
}
