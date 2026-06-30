using Domain.Entities;

namespace Application.Interfaces.Repositories.ConversionTool;

public interface IConversionToolReadRepository
{
    Task<List<ConversionTool>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ConversionTool?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
