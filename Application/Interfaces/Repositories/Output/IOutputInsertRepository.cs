using OutputEntity = Domain.Entities.Output;

namespace Application.Interfaces.Repositories.Output;

public interface IOutputInsertRepository
{
    void Add(OutputEntity output);
}
