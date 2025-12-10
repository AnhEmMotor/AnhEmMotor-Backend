using OutputEntity = Domain.Entities.Output;

namespace Application.Interfaces.Repositories.Output;

public interface IOutputUpdateRepository
{
    public void Update(OutputEntity output);

    public void Restore(OutputEntity output);

    public void Restore(IEnumerable<OutputEntity> outputs);

    public Task ProcessCOGSForCompletedOrderAsync(int outputId, CancellationToken cancellationToken);
}
