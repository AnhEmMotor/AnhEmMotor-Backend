using Application.Interfaces.Repositories.HR.Employee;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.HR.Employee;

public sealed class EmployeeDeleteRepository(ApplicationDBContext context) : IEmployeeDeleteRepository
{
    public void Delete(EmployeeProfile employee)
    {
        context.EmployeeProfiles.Remove(employee);
    }
}
