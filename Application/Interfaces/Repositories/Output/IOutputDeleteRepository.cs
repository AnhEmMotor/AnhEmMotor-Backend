using OutputEntity = Domain.Entities.Output;
using OutputInfoEntity = Domain.Entities.OutputInfo;

namespace Application.Interfaces.Repositories.Output;

public interface IOutputDeleteRepository
{
    public void Delete(OutputEntity output);

    public void Delete(IEnumerable<OutputEntity> outputs);

    public void DeleteOutputInfo(OutputInfoEntity outputInfo);
}
