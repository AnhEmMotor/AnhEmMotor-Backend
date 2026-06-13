using Application.Interfaces.Repositories.Service;
using Infrastructure.DBContexts;
using ServiceEntity = Domain.Entities.Service;

namespace Infrastructure.Repositories.Service;

public class ServiceInsertRepository(ApplicationDBContext context) : IServiceInsertRepository
{
    public void Add(ServiceEntity service)
    {
        context.Services.Add(service);
    }
}
