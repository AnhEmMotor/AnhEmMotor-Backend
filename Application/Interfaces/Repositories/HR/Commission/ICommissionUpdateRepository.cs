using Application.Common.Models;

namespace Application.Interfaces.Repositories.HR.Commission;

public interface ICommissionUpdateRepository
{
    public Task<Result> CalculatePendingCommissionAsync(int outputId, CancellationToken cancellationToken = default);

    public Task<Result> CalculateAndRecordCommissionAsync(int outputId, CancellationToken cancellationToken = default);

    public Task<Result> MarkCommissionAsPaidAsync(int outputId, CancellationToken cancellationToken = default);

    public Task<Result> VoidCommissionAsync(int outputId, CancellationToken cancellationToken = default);
}
