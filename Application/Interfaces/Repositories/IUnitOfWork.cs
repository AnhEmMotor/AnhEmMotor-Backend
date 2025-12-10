namespace Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
