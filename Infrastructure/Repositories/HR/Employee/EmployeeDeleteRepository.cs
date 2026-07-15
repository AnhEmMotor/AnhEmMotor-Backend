using Application.Interfaces.Repositories.HR.Employee;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.HR.Employee;

public class EmployeeDeleteRepository(ApplicationDBContext context) : IEmployeeDeleteRepository
{
    public void Delete(EmployeeProfile entity)
    {
        context.EmployeeProfiles.Remove(entity);
    }

    public void Delete(IEnumerable<EmployeeProfile> entities)
    {
        context.EmployeeProfiles.RemoveRange(entities);
    }
}
