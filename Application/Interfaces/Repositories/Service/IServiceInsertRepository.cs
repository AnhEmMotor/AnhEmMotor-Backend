using ServiceEntity = Domain.Entities.Service;

namespace Application.Interfaces.Repositories.Service
{
    public interface IServiceInsertRepository
    {
        void Add(ServiceEntity service);
    }
}
