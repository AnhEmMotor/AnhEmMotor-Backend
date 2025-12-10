using OutputEntity = Domain.Entities.Output;

namespace Application.Interfaces.Repositories.Output;

public interface IOutputInsertRepository
{
    public void Add(OutputEntity output);
}
