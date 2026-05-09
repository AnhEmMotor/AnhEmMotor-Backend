using Domain.Constants;

namespace Application.Interfaces.Repositories.Vehicle;

public interface IVehicleReadRepository
{
    public IQueryable<Domain.Entities.Vehicle> GetQuery(DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<List<Domain.Entities.Vehicle>> GetVehiclesAsync(
        string? search,
        CancellationToken cancellationToken = default);

    public IQueryable<Domain.Entities.Vehicle> All();
}
