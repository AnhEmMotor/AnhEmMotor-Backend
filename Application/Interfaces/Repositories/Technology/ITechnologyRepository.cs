
namespace Application.Interfaces.Repositories.Technology;

public interface ITechnologyRepository
{
    public IQueryable<Domain.Entities.Technology> GetQueryable();

    public Task<Domain.Entities.Technology?> GetByIdAsync(int id, CancellationToken cancellationToken);

    public void Add(Domain.Entities.Technology technology);
}
