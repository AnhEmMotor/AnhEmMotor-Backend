using OutputEntity = Domain.Entities.Output;

namespace Application.Interfaces.Repositories.Output;

public interface IOutputUpdateRepository
{
    void Update(OutputEntity output);

    void Restore(OutputEntity output);

    void Restore(IEnumerable<OutputEntity> outputs);

    Task ProcessCOGSForCompletedOrderAsync(int outputId, CancellationToken cancellationToken);
}
