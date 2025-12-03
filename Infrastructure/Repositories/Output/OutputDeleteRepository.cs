using Application.Interfaces.Repositories.Output;
using Infrastructure.DBContexts;
using OutputEntity = Domain.Entities.Output;
using OutputInfoEntity = Domain.Entities.OutputInfo;

namespace Infrastructure.Repositories.Output;

public class OutputDeleteRepository(ApplicationDBContext context) : IOutputDeleteRepository
{
    public void Delete(OutputEntity output) { context.SoftDeleteUsingSetColumn(output); }

    public void Delete(IEnumerable<OutputEntity> outputs) { context.SoftDeleteUsingSetColumnRange(outputs); }

    public void DeleteOutputInfo(OutputInfoEntity outputInfo) { context.OutputInfos.Remove(outputInfo); }
}
