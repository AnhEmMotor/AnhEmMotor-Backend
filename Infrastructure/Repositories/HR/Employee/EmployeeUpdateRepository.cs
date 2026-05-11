using Application.Interfaces.Repositories.HR.Employee;
using Domain.Entities;
using Infrastructure.DBContexts;

using Domain.Entities;

namespace Infrastructure.Repositories.HR.Employee;

public class EmployeeUpdateRepository(ApplicationDBContext context) : IEmployeeUpdateRepository
{
    public void Update(EmployeeProfile employee)
    {
        context.EmployeeProfiles.Update(employee);
    }
}
