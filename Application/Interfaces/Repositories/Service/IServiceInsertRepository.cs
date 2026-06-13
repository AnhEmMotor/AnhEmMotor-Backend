using ServiceEntity = Domain.Entities.Service;

namespace Application.Interfaces.Repositories.Service
{
    public interface IServiceInsertRepository
    {
        public void Add(ServiceEntity service);
    }
}
