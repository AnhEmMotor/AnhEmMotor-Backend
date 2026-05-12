using Application.Interfaces.Repositories.HR.Employee;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.HR.Employee;

public class EmployeeUpdateRepository(ApplicationDBContext context) : IEmployeeUpdateRepository
{
    public void Update(EmployeeProfile employee)
    {
        context.EmployeeProfiles.Update(employee);
    }
}
